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
    public abstract class OptionalExecutor : ImportExtension
    {
        /// <summary>
        /// Executed after the last files and data have been imported into the project, but before 
        /// </summary>
        public abstract void Execute();

        public bool enabled;

        public virtual string Description { get; }

        public VisualElement CreateUI()
        {
            var element = new VisualElement { name = "extension-item" };

            var header = new VisualElement { name = "extension-item-header" };
            header.AddToClassList("thunderkit-field");
            header.tooltip = string.IsNullOrEmpty(Description) ? $"(Import Extension Priority: {Priority})" : $"{Description}\n\n(Import Extension Priority: {Priority}";
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