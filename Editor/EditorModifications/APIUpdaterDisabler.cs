using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class APIUpdaterDisabler
{
    [InitializeOnLoadMethod]
    public static void Initializer()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var unityAssemblies = assemblies.Where(asm => asm.FullName.ToLower().Contains("unity")).ToArray();
        var unityEditorAsm = unityAssemblies.FirstOrDefault(asm => asm.FullName.ToLower().Contains("editor"));
        var apiUpdater = unityEditorAsm.GetType("UnityEditorInternal.APIUpdating.APIUpdaterManager");
        var runMethod = apiUpdater.GetMethod("GetAssembliesToBeUpdated", BindingFlags.NonPublic | BindingFlags.Static);
        var abortMethod = typeof(APIUpdaterDisabler).GetMethod(nameof(Abort), BindingFlags.NonPublic | BindingFlags.Static);
        //redirect runMethod to abortMethod;
        //Debug.Log("Applied redirect");
    }

    static void Abort(string[] assemblies)
    {
        Debug.Log("Aborted API Update");
    }
}
