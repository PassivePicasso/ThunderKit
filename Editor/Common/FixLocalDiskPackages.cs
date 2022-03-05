#if ENABLE_VSTU && UNITY_2019_1_OR_NEWER

using SyntaxTree.VisualStudio.Unity.Bridge;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
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
    static readonly Dictionary<string, string> localPackageSources = new Dictionary<string, string>();

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
                        localPackageSources[Path.GetFileNameWithoutExtension(asmDef)] = asmDef;
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

            // parse the document and make some changes
            XDocument document = XDocument.Parse(content);
            AdjustDocument(document, localPackageSources[assemblyName]);

            // save the changes using the Utf8StringWriter
            Utf8StringWriter str = new Utf8StringWriter();
            document.Save(str);

            return str.ToString();
        };
    }

    // -----------------------------------------------------------
    static void AdjustDocument(XDocument document, string asmRoot)
    {
        var directoryName = Path.GetDirectoryName(asmRoot).Replace("\\", "\\\\");

        // get namespace of document
        XNamespace ns = document.Root.Name.Namespace;

        // get all Compile elements
        IEnumerable<XElement> compileElements = document.Root.Descendants(ns + "Compile");

        // regex to find which part of Include attribute of Compile element to use for Link element value
        // check for Editor or Runtime (recommended folders: https://docs.unity3d.com/Manual/cus-layout.html)
        Regex regex = new Regex($@"{directoryName}.*?\\(.*\.cs$)");

        // add child Link element to each Compile element
        foreach (XElement el in compileElements)
        {

            string fileName = el.Attribute("Include").Value;

            Match match = regex.Match(fileName);

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