using System;
using ThunderKit.Core.Pipelines;
using UnityEditor;
using UnityEngine.Networking;

namespace ThunderKit.Core.Paths
{
    public class PathComponent : ComposableElement
    {

        public string GetPath(PathReference output, Pipeline pipeline)
        {
            try
            {
                Errored = false;
                ErrorMessage = string.Empty;
                return GetPathInternal(output, pipeline);
            }
            catch (Exception e)
            {
                Errored = true;
                ErrorMessage = e.Message;
                ErrorStacktrace = e.StackTrace;
                throw;
            }
        }

        protected virtual string GetPathInternal(PathReference output, Pipeline pipeline) => "";
    }
}