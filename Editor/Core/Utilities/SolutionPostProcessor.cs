using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using ThunderKit.Core.Data;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
using PackageSource = UnityEditor.PackageManager.PackageSource;

public class SolutionPostProcessor : AssetPostprocessor
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
    static readonly string[] rootArray = new string[1];

    static readonly Dictionary<string, (string asmDefPath, PackageInfo packageInfo)> localPackageSources = new Dictionary<string, (string asmDefPath, PackageInfo packageInfo)>();

    /// <summary>
    /// Generate a dictionary with information necessary to clean up project file
    /// Paths
    /// </summary>
    static SolutionPostProcessor()
    {
        ListRequest lr = null;
        EditorApplication.update += OnUpdate;
        void OnUpdate()
        {
            if (lr == null)
            {
                lr = Client.List();
                return;
            }

            if (!lr.IsCompleted) return;
            EditorApplication.update -= OnUpdate;
            var packages = lr.Result;
            foreach (var package in packages)
            {
                if (package.source != PackageSource.Local) continue;

                foreach (var asmDef in Directory.EnumerateFiles(package.resolvedPath, "*.asmdef", SearchOption.AllDirectories))
                {
                    localPackageSources[Path.GetFileNameWithoutExtension(asmDef)] = (asmDef, package);
                }
            }
        }
    }

    const string XML_HEADER = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r";
    const string PROJECT_OPEN = "<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\">\r";
    const string PROJECT_CLOSE = "</Project>";
    const string ITEMGROUP_OPEN = "  <ItemGroup>\r";
    const string ITEMGROUP_CLOSE = "  </ItemGroup>\r";
    const string DOC_PROJ_NAME = "ThunderKit.Documentation";

    private static string GenerateSlnProjectEntry(string name) =>
        $"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{name}\", \"{name}.csproj\", \"{Guid.NewGuid().ToString("B")}\"\rEndProject\r";

    /// <summary>
    /// Creates a simple CSProj file with the most basic requirements and a single ItemGroup containing None entries for each member of <paramref name="files"/>
    /// </summary>
    /// <param name="files">A collection of FilePaths and their Project Display Paths (linkPath)</param>
    /// <returns>A String with the contents for a CSProj file as described</returns>
    public static string GenerateProjectFile(IEnumerable<(string linkPath, string filePath)> files)
    {
        var includes = new List<string>();

        foreach (var (linkPath, filePath) in files)
            includes.Add($"    <None Include=\"{filePath}\">\r    " +
                         $"      <Link>{linkPath}</Link>\r" +
                         $"    </None>");

        var includesString = includes.Aggregate((a, b) => $"{a}\r{b}") + "\r";
        return
            XML_HEADER +
            PROJECT_OPEN +
              ITEMGROUP_OPEN +
                includesString +
              ITEMGROUP_CLOSE +
            PROJECT_CLOSE;
    }

    /// <summary>
    /// Returns an enumeration of Markdown Documents managed by a DocumentationRoot asset.
    /// </summary>
    /// <param name="root">A tuple containing the path to a DocumentationRoot asset and the corresponding loaded Asset</param>
    /// <returns>An enumeration of link and file paths where the link is relative to the DocumentationRoot path</returns>
    public static IEnumerable<(string linkPath, string filePath)> GetDistinctMarkdownDocuments((string path, DocumentationRoot asset) root)
    {
        rootArray[0] = root.path;
        var rootFullPath = Path.GetFullPath(Path.GetDirectoryName(root.path)).Replace("\\", "/");

        var documentguids = AssetDatabase.FindAssets($"t:TextAsset", rootArray);
        var documentPaths = documentguids.Select(AssetDatabase.GUIDToAssetPath).Where(p => Path.GetExtension(p).Equals(".md"));
        var distinctDocuments = documentPaths.Distinct().Select(Path.GetFullPath)
            .Select(filePath => (linkPath: filePath.Substring(rootFullPath.Length), filePath));

        return distinctDocuments;
    }

    /// <summary>
    /// Process Unity generation Solution files to add a project for ThunderKit
    /// documentation to improve utility for viewing and editing Markdown
    /// documentation for ThunderKit and ThunderKit based projects.
    /// </summary>
    /// <param name="path">Path to generated Solution File</param>
    /// <param name="content">Content of solution file generated by unity</param>
    /// <returns>Modified Solution file content</returns>
    public static string OnGeneratedSlnSolution(string path, string content)
    {
        var directoryPath = Path.GetDirectoryName(path);

        var docFiles = AssetDatabase.FindAssets($"t:{nameof(DocumentationRoot)}")
                .Distinct()
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .Select(p => (path: Path.GetDirectoryName(p).Replace("\\", "/"), asset: AssetDatabase.LoadAssetAtPath<DocumentationRoot>(p)))
                .SelectMany(GetDistinctMarkdownDocuments);

        File.WriteAllText(Path.Combine(directoryPath, $"{DOC_PROJ_NAME}.csproj"), GenerateProjectFile(docFiles));

        var lines = content.Split('\n').ToList();
        for (int i = 0; i < lines.Count; i++)
        {
            // If line is the end of the last project entry
            if (!lines[i].StartsWith("EndProject")) continue;
            if (lines[i + 1].StartsWith("Project")) continue;
            //Add all Documentation projects
            lines.Insert(i + 1, GenerateSlnProjectEntry(DOC_PROJ_NAME));
            break;
        }

        var newContent = lines.Aggregate((a, b) => $"{a}{b}");

        return newContent;
    }

    /// <summary>
    /// Process CSProj files generated by Unity to fix local disk packages not
    /// having their folder structure
    /// </summary>
    /// <param name="path">Path to generated CSProj File</param>
    /// <param name="content">Content of CSProj file generated by unity</param>
    /// <returns>Modified CSProj file content</returns>
    public static string OnGeneratedCSProject(string path, string content)
    {
        var assemblyName = Path.GetFileNameWithoutExtension(path);
        if (!localPackageSources.ContainsKey(assemblyName))
            return content;

        // parse the document and  make some changes
        XDocument document = XDocument.Parse(content);
        AdjustDocument(document, assemblyName);

        // save the changes using the Utf8StringWriter
        Utf8StringWriter str = new Utf8StringWriter();
        document.Save(str);

        var result = str.ToString();
        return result;
    }

    /// <summary>
    /// Adjusts a CSProj file so that its Compile and None elements will use a 
    /// link path which represents their lcoation relative to the AsmRef they
    /// are managed by
    /// </summary>
    /// <param name="document">A Parsed CSProj file in XDocument format</param>
    /// <param name="assemblyName">The name of the CSProj file / assembly</param>
    static void AdjustDocument(XDocument document, string assemblyName)
    {
        var ns = document.Root.Name.Namespace;
        var data = localPackageSources[assemblyName];
        var directoryName = data.packageInfo.resolvedPath.Replace("\\", "\\\\"); //Path.GetDirectoryName(data.asmDefPath).Replace("\\", "\\\\");
        var compileReferenceRefex = new Regex($"{directoryName}.*?\\\\(.*\\.*$)");

        // get all Compile elements
        var firstGroup = document.Root.Descendants(ns + "PropertyGroup").First();
        var element = new XElement(ns + "PathRoot") { Value = data.packageInfo.resolvedPath };
        firstGroup.Add(element);

        IEnumerable<XElement> compileElements = document.Root.Descendants(ns + "Compile");
        IEnumerable<XElement> noneElements = document.Root.Descendants(ns + "None");

        // regex to find which part of Include attribute of Compile element to use for Link element value
        // check for Editor or Runtime (recommended folders: https://docs.unity3d.com/Manual/cus-layout.html)

        // add child Link element to each Compile element
        foreach (XElement el in compileElements.Union(noneElements))
        {
            XAttribute include = el.Attribute("Include");
            string fileName = include.Value;

            Match match = compileReferenceRefex.Match(fileName);

            if (match.Success)
            {
                var capture = match.Groups[1].Value.Replace("\\", "/");
                // substr from 1 to exclude initial slash character
                XElement link = new XElement(ns + "Link")
                {
                    Value = capture
                };

                include.Value = $"$(PathRoot)/{capture}";

                el.Add(link);
            }
        }
    }
}