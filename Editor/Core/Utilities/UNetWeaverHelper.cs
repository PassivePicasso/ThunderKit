using System;
using System.Linq;
using System.Reflection;

namespace ThunderKit.Core
{
    public static class UNetWeaverHelper
    {
        public static MethodInfo GetProcessMethod()
        {
            var allFlags = (BindingFlags)(-1);

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

                var uNetWeaverProgramType = uNetWeaverAssembly.GetType("Unity.UNetWeaver.Program");
                if (uNetWeaverProgramType == null)
                {
                    return null;
                }

                return uNetWeaverProgramType.GetMethod("Process", allFlags);
            }

            return null;
        }
    }
}