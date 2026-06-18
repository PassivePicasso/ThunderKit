using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

#if UNITY_6000_5_OR_NEWER
using TreeViewT = UnityEditor.IMGUI.Controls.TreeView<UnityEngine.EntityId>;
using TreeViewStateT = UnityEditor.IMGUI.Controls.TreeViewState<UnityEngine.EntityId>;
using TreeViewItemT = UnityEditor.IMGUI.Controls.TreeViewItem<UnityEngine.EntityId>;
#else
using TreeViewT = UnityEditor.IMGUI.Controls.TreeView;
using TreeViewStateT = UnityEditor.IMGUI.Controls.TreeViewState;
using TreeViewItemT = UnityEditor.IMGUI.Controls.TreeViewItem;
#endif

namespace ThunderKit.Addressable.Tools
{
    public class TransformHierarchyTreeView : TreeViewT
    {
        private readonly Transform root;
        private readonly TreeViewItemT localRootItem;
        #if UNITY_6000_5_OR_NEWER
        private readonly Dictionary<EntityId, Transform> transformLookup = new Dictionary<EntityId, Transform>();
        #else
        private readonly Dictionary<int, Transform> transformLookup = new Dictionary<int, Transform>();
        #endif
        public TransformHierarchyTreeView(TreeViewStateT state, Transform root) : base(state)
        {
            this.root = root;
            localRootItem = new TreeViewItemT { id = default, depth = -1, displayName = "root", children = new List<TreeViewItemT>() };
        }
        protected override TreeViewItemT BuildRoot()
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
            var firstItem = new TreeViewItemT {
                #if UNITY_6000_5_OR_NEWER
                id = root.GetEntityId(),
                #else
                id = root.GetInstanceID(),
                #endif
                depth = depth++,
                displayName = root.name,
                children = new List<TreeViewItemT>() 
            };
            localRootItem.children.Add(firstItem);

            ConstructTree(depth, firstItem, root);

            return localRootItem;
        }

        private void ConstructTree(int depth, TreeViewItemT parentItem, Transform parent)
        {
            if (parentItem.children == null) parentItem.children = new List<TreeViewItemT>();
            foreach (Transform child in parent)
            {
                transformLookup[
                    #if UNITY_6000_5_OR_NEWER
                    child.GetEntityId()
                    #else
                    child.GetInstanceID()
                    #endif
                ] = child;
                var childItem = new TreeViewItemT
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
        
        protected override void SingleClickedItem(
            #if UNITY_6000_5_OR_NEWER
            EntityId id
            #else
            int id
            #endif
        )
        {
            base.SingleClickedItem(id);
            Selection.activeGameObject = transformLookup[id].gameObject;
        }

        protected override void DoubleClickedItem(
            #if UNITY_6000_5_OR_NEWER
            EntityId id
            #else
            int id
            #endif
        )
        {
            base.DoubleClickedItem(id);
            Selection.activeGameObject = transformLookup[id].gameObject;
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.FrameSelected();
        }

    }
}
