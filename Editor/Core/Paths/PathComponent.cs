using System;
using ThunderKit.Core.Pipelines;

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
                throw new InvalidOperationException($"Error Invoking PathComponent: {name}", e);
            }
        }

        protected virtual string GetPathInternal(PathReference output, Pipeline pipeline) => "";
    }
}