using System;
using System.Collections.Generic;

namespace ThunderKit.Integrations.Thunderstore.Manifests
{
    [Serializable]
    public class DependencyList : List<String>
    {
        public DependencyList() { }
        public DependencyList(IEnumerable<String> data) : base(data) { }
    }
}