using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ThunderKit.Core.Utilities
{
    public class UNetWeaverHelper : MarshalByRefObject
    {
        private const BindingFlags allFlags = (BindingFlags)(-1);

        public MethodInfo GetProcessMethod()
        {
            return GetProcessMethodBeforeUnity2019() ?? GetProcessMethodAfterUnity2019();
        }

        private MethodInfo GetProcessMethodBeforeUnity2019()
        {
            var editorAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "UnityEditor");
            if (editorAssembly == null)
            {
                return null;
            }

            var editorWeaverType = editorAssembly.GetType("UnityEditor.Scripting.Serialization.Weaver");
            if (editorWeaverType == null)
            {
                return null;
            }

            var weaveIntoMethod = editorWeaverType.GetMethod("WeaveInto", allFlags);
            try
            {
                // call WeaveInto so that UNetWeaver get lazily loaded into the app domain
                var paramCount = weaveIntoMethod.GetParameters().Length;
                weaveIntoMethod.Invoke(null, new object[paramCount]);
            }
            catch
            {
                var uNetWeaverAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "Unity.UNetWeaver");
                if (uNetWeaverAssembly == null)
                {
                    return null;
                }

                return GetProcessMethodFromAssembly(uNetWeaverAssembly);
            }

            return null;
        }

        private MethodInfo GetProcessMethodFromAssembly(Assembly uNetWeaverAssembly)
        {
            var uNetWeaverProgramType = uNetWeaverAssembly.GetType("Unity.UNetWeaver.Program");
            if (uNetWeaverProgramType == null)
            {
                return null;
            }

            return uNetWeaverProgramType.GetMethod("Process", allFlags);
        }

        private MethodInfo GetProcessMethodAfterUnity2019()
        {
            Assembly weaverAssembly = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "com.unity.multiplayer-weaver.Editor")
                {
                    weaverAssembly = assembly;
                }
            }

            if (weaverAssembly == null)
            {
                try
                {
                    var weaverPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library", "ScriptAssemblies", "com.unity.multiplayer-weaver.Editor.dll");
                    weaverAssembly = Assembly.Load(weaverPath);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            if (weaverAssembly == null)
            {
                return null;
            }

            return GetProcessMethodFromAssembly(weaverAssembly);
        }
    }
}