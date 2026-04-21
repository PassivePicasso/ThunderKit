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
        #if UNITY_6000_5_OR_NEWER
        private readonly Dictionary<EntityId, Transform> transformLookup = new Dictionary<EntityId, Transform>();
        #else
        private readonly Dictionary<int, Transform> transformLookup = new Dictionary<int, Transform>();
        #endif
        public TransformHierarchyTreeView(TreeViewState state, Transform root) : base(state)
        {
            this.root = root;
            localRootItem = new TreeViewItem { id = 0, depth = -1, displayName = "root", children = new List<TreeViewItem>() };
        }
        protected override TreeViewItem BuildRoot()
        {
            int depth = 0;
            transformLookup.Clear();
            transformLookup[
                #if UNITY_6000_5_OR_NEWER
                root.GetEntityId()
                #else
                root.GetInstanceID()
                #endif
            ] = root;
            var firstItem = new TreeViewItem {
                #if UNITY_6000_5_OR_NEWER
                id = root.GetEntityId(),
                #else
                id = root.GetInstanceID(),
                #endif
                depth = depth++,
                displayName = root.name,
                children = new List<TreeViewItem>() 
            };
            localRootItem.children.Add(firstItem);

            ConstructTree(depth, firstItem, root);

            return localRootItem;
        }

        private void ConstructTree(int depth, TreeViewItem parentItem, Transform parent)
        {
            if (parentItem.children == null) parentItem.children = new List<TreeViewItem>();
            foreach (Transform child in parent)
            {
                transformLookup[child.GetInstanceID()] = child;
                var childItem = new TreeViewItem
                {
                    #if UNITY_6000_5_OR_NEWER
                    id = child.GetEntityId(),
                    #else
                    id = child.GetInstanceID(),
                    #endif
                    depth = depth,
                    parent = parentItem,
                    displayName = child.name
                };
                ConstructTree(depth + 1, childItem, child);
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
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.FrameSelected();
        }

    }
}
