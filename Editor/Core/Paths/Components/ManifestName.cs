using System;
using ThunderKit.Core.Pipelines;

namespace ThunderKit.Core.Paths.Components
{
    public class ManifestName : PathComponent
    {
        protected override string GetPathInternal(PathReference output, Pipeline pipeline)
        {
            try
            {
                return pipeline.Manifest.Identity.Name;
            }
            catch (NullReferenceException nre)
            {
                if (pipeline.Manifest == null)
                {
                    Errored = true;
                    ErrorMessage = $"Manifest not found";
                    ErrorStacktrace = nre.StackTrace;
                    throw new NullReferenceException(ErrorMessage, nre);
                }
                else if (pipeline.Manifest.Identity == null)
                {
                    Errored = true;
                    ErrorMessage = $"ManifestIdentity not found";
                    ErrorStacktrace = nre.StackTrace;
                    throw new NullReferenceException(ErrorMessage, nre);
                }
                throw nre;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
