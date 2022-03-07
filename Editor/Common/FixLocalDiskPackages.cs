#if ENABLE_VSTU && UNITY_2019_1_OR_NEWER

using SyntaxTree.VisualStudio.Unity.Bridge;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

[InitializeOnLoad]
public class ProjectFileHook
{

    // necessary for XLinq to save the xml project file in utf8
    class Utf8StringWriter : StringWriter
    {

        // -----------------------------------------------------------
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
    static readonly Dictionary<string, (string asmDefPath, PackageInfo packageInfo)> localPackageSources = new Dictionary<string, (string asmDefPath, PackageInfo packageInfo)>();

    // -----------------------------------------------------------
    static ProjectFileHook()
    {
        var lr = Client.List();
        void OnUpdate()
        {
            if (!lr.IsCompleted) return;
            EditorApplication.update -= OnUpdate;
            var packages = lr.Result;
            foreach (var package in packages)
            {
                if (package.source == PackageSource.Local)
                {
                    foreach (var asmDef in Directory.EnumerateFiles(package.resolvedPath, "*.asmdef", SearchOption.AllDirectories))
                    {
                        localPackageSources[Path.GetFileNameWithoutExtension(asmDef)] = (asmDef, package);
                    }
                }
            }
        }

        EditorApplication.update += OnUpdate;
        ProjectFilesGenerator.ProjectFileGeneration += (string name, string content) =>
        {
            var assemblyName = Path.GetFileNameWithoutExtension(name);
            if (!localPackageSources.ContainsKey(assemblyName))
                return content;

            // parse the document and  make some changes
            XDocument document = XDocument.Parse(content);
            AdjustDocument(document, assemblyName);

            // save the changes using the Utf8StringWriter
            Utf8StringWriter str = new Utf8StringWriter();
            document.Save(str);

            return str.ToString();
        };
        ProjectFilesGenerator.SolutionFileGeneration += OnGenerateSolution;
    }

    private static string OnGenerateSolution(string fileName, string fileContent)
    {
        Debug.Log($"Generating: {fileName}\r\n\r\n{fileContent}");
        return fileContent;
    }


    // -----------------------------------------------------------
    static void AdjustDocument(XDocument document, string assemblyName)
    {
        var ns = document.Root.Name.Namespace;
        var data = localPackageSources[assemblyName];
        var directoryName = data.packageInfo.resolvedPath.Replace("\\", "\\\\"); //Path.GetDirectoryName(data.asmDefPath).Replace("\\", "\\\\");
        var compileReferenceRefex = new Regex($"{directoryName}.*?\\\\(.*\\.*$)");
        //var lastItemGroup = document.Root.Descendants(ns + "ItemGroup").Last();

        //var userExtensions = EditorSettings.projectGenerationUserExtensions
        //    .SelectMany(ext => Directory.EnumerateFiles(directoryName, $"*.{ext}", SearchOption.AllDirectories))
        //    .ToArray();

        //foreach (var ext in EditorSettings.projectGenerationUserExtensions)
        //{
        //    var extensionReferenceCheck = new Regex($@"{directoryName.Replace("\\", "\\\\")}.*?\\(.*\.{ext}$)");
        //    var itemGroup = new XElement(ns + "ItemGroup");
        //    foreach (var include in userExtensions)
        //    {
        //        var none = new XElement(ns + "None");
        //        none.SetAttributeValue("Include", include);
        //        Match match = extensionReferenceCheck.Match(include);
        //        if (match.Success)
        //        {
        //            var capture = match.Groups[1].Value.Replace("\\", "/");
        //            none.Add(new XElement(ns + "Link")
        //            {
        //                Value = capture
        //            });
        //        }
        //        itemGroup.Add(none);
        //    }
        //    lastItemGroup.AddBeforeSelf(itemGroup);
        //}

        // get all Compile elements
        IEnumerable<XElement> compileElements = document.Root.Descendants(ns + "Compile");
        IEnumerable<XElement> noneElements = document.Root.Descendants(ns + "None");

        // regex to find which part of Include attribute of Compile element to use for Link element value
        // check for Editor or Runtime (recommended folders: https://docs.unity3d.com/Manual/cus-layout.html)

        // add child Link element to each Compile element
        foreach (XElement el in compileElements.Union(noneElements))
        {

            string fileName = el.Attribute("Include").Value;

            Match match = compileReferenceRefex.Match(fileName);

            if (match.Success)
            {
                var capture = match.Groups[1].Value;
                // substr from 1 to exclude initial slash character
                XElement link = new XElement(ns + "Link")
                {
                    Value = capture.Replace("\\", "/")
                };

                el.Add(link);
            }
        }
    }
}

#endif