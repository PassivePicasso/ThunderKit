
using System;
using System.Collections.Generic;
using System.Linq;
using ThunderKit.Core.Data;
using ThunderKit.Core.Windows;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKitTests
{
    internal class PackageManagerFixture : IDisposable
    {
        public PackageManager Window { get; private set; }
        public VisualElement PackageSourceList { get; private set; }

        public PackageManagerFixture()
        {
            Window = ScriptableObject.CreateInstance<PackageManager>();
            Window.ShowUtility(); // Creates rootVisualElement
            Window.OnEnable();    // Loads the UI

            PackageSourceList = Window.Root.Q(name: "tkpm-package-source-list");
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