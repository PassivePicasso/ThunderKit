
using System;
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderKitTests
{
    internal class PackageManagerFixture : IDisposable
    {
        public VisualElement Root { get; private set; }
        public PackageManager Window { get; private set; }
        public VisualElement PackageSourceList { get; private set; }

        public PackageManagerFixture()
        {
            Window = ScriptableObject.CreateInstance<PackageManager>();
            Window.ShowUtility(); // Creates rootVisualElement
            Window.OnEnable();    // Loads the UI

            Root = Window.rootVisualElement;
            PackageSourceList = Root.Q(name: "tkpm-package-source-list");
        }

        public void Dispose()
        {
            Window.Close();
        }

        public List<TemplateContainer> GetPackageSourceViews()
        {
            return PackageSourceList.Query<TemplateContainer>().ToList()
                    .Where(tc => tc.name.StartsWith("tkpm-package-source-"))
                    .ToList();
        }

        public List<PackageSource> GetListedSources()
        {
            var packageSourceViews = GetPackageSourceViews();
            return packageSourceViews.Select(view => view.userData as PackageSource).ToList();
        }
    }
}