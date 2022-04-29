using System;
using ThunderKit.Common;
using ThunderKit.Core.UIElements;
#if UNITY_2019_1_OR_NEWER
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
        public abstract bool Execute();

        public bool enabled = true;

        public virtual string Description { get; }
        protected virtual string UITemplatePath => Constants.SettingsTemplatesPath + $"/{GetType().Name}.uxml";

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
            try
            {
                var properties = CreateProperties();
                if (properties != null)
                    element.Add(properties);
            }
            catch { }

            return element;
        }

        protected virtual VisualElement CreateProperties()
        {
            var uiTemplate = TemplateHelpers.LoadTemplateInstance(UITemplatePath);
            uiTemplate.AddEnvironmentAwareSheets(Constants.ThunderKitSettingsTemplatePath);

            return uiTemplate;
        }
    }
}