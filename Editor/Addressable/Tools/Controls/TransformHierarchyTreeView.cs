using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ThunderKit.Addressable.Tools
{
    public class TransformHierarchyTreeView : TreeView
    {
        private readonly Transform root;
        private readonly TreeViewItem localRootItem;
        private readonly Dictionary<int, Transform> transformLookup = new Dictionary<int, Transform>();

        public TransformHierarchyTreeView(TreeViewState state, Transform root) : base(state)
        {
            this.root = root;
            localRootItem = new TreeViewItem { id = 0, depth = -1, displayName = "root", children = new List<TreeViewItem>() };
        }
        protected override TreeViewItem BuildRoot()
        {
            int depth = 0;
            transformLookup.Clear();
            transformLookup[root.GetInstanceID()] = root;
            var firstItem = new TreeViewItem { id = root.GetInstanceID(), depth = depth++, displayName = root.name, children = new List<TreeViewItem>() };
            localRootItem.children.Add(firstItem);

            ConstructTree(depth, firstItem, root);

            return localRootItem;
        }

        private void ConstructTree( int depth, TreeViewItem parentItem, Transform parent)
        {
            if (parentItem.children == null) parentItem.children = new List<TreeViewItem>();
            foreach (Transform child in parent)
            {
                transformLookup[child.GetInstanceID()] = child;
                var childItem = new TreeViewItem
                {
                    id = child.GetInstanceID(),
                    depth = depth,
                    parent = parentItem,
                    displayName = child.name
                };
                ConstructTree( depth + 1, childItem, child);
                parentItem.children.Add(childItem);
            }
        }
        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);
            Selection.activeGameObject = transformLookup[id].gameObject;
        }

        protected override void DoubleClickedItem(int id)
        {
            base.DoubleClickedItem(id);
            Selection.activeGameObject = transformLookup[id].gameObject;
            SceneView.lastActiveSceneView.FrameSelected();
        }

    }
}