using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Reflection;

public class GenerateProxies : ScriptableObject
{
    [MenuItem("Tools/RainOfStages/Generate Proxies")]
    static void DoIt()
    {
        var ror2Assembly = typeof(RoR2.RoR2Application).Assembly;
        var types = ror2Assembly.GetTypes();
        var ror2MonoBehaviours = types.Where(t => typeof(MonoBehaviour).IsAssignableFrom(t)).ToArray();
        StringBuilder proxyBuilder = new StringBuilder();
        foreach(var behaviour in ror2MonoBehaviours)
        {
            var publicFields = behaviour.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var publicFieldUsings = publicFields.Select(fi => $"using {fi.FieldType.Namespace};");
            var publicFieldDefinitions = publicFields.Select(fi => $"public {fi.FieldType.Name} {fi.Name};");

            var className = behaviour.Name;
            var nameSpace = behaviour.Namespace;

            publicFieldUsings.ToList().ForEach(s => proxyBuilder.AppendLine(s));


            proxyBuilder.Append("namespace ").Append(nameSpace).AppendLine();
            proxyBuilder.AppendLine("{");
            publicFieldDefinitions.ToList().ForEach(s => proxyBuilder.AppendLine(s));
            proxyBuilder.AppendLine("}");
            break;
        }

        Debug.Log(proxyBuilder.ToString());
    }
}