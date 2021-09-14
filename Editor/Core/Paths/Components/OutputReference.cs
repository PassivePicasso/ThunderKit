using System;
using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths.Components
{
    public class OutputReference : PathComponent
    {
        public PathReference reference;
        protected override string GetPathInternal(PathReference output, Pipeline pipeline)
        {
            try
            {
                Errored = false;
                ErrorMessage = string.Empty;
                return reference.GetPath(pipeline);
            }
            catch (NullReferenceException nre)
            {
                Errored = true;
                ErrorMessage = nre.Message;
                ErrorStacktrace = nre.StackTrace;
                throw new InvalidOperationException($"Error PathReference is unassigned or null", nre);

            }
            catch (Exception e)
            {
                Errored = true;
                ErrorMessage = e.Message;
                ErrorStacktrace = e.StackTrace;
                throw new InvalidOperationException($"Error Invoking PathReference: {reference.name}", e);
            }
        }
    }
}
