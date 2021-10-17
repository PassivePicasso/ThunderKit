using ThunderKit.Core.Pipelines;
using ThunderKit.Core.Paths;
using Microsoft.Win32;

namespace ThunderKit.Core.Paths.Components
{
    public class RegistryLookup : PathComponent
    {
        public string KeyName;
        public string ValueName;

        protected override string GetPathInternal(PathReference output, Pipeline pipeline)
        {
            return (string)Registry.GetValue(KeyName, ValueName, string.Empty);
        }
    }
}
