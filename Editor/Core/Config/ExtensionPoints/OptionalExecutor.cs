using System;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_2019
using UnityEngine.UIElements;
#elif UNITY_2018
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.Config
{
    [Serializable]
    public abstract class OptionalExecutor : Executor
    {
        public bool enabled;

        public sealed override VisualElement CreateUI()
        {
            var element = new VisualElement { name = "extension-item" };

            var header = new VisualElement { name = "extension-item-header" };
            header.AddToClassList("thunderkit-field");
            var label = new Label { name = "extension-label", text = Name };
            header.Add(label);
            var toggle = new Toggle { name = "extension-enabled-toggle", bindingPath = nameof(OptionalExecutor.enabled) };
            header.Add(toggle);

            element.Add(header);
            var properties = CreateProperties();
            if (properties != null)
                element.Add(properties);

            return element;
        }

        protected virtual VisualElement CreateProperties() => null;
    }
}