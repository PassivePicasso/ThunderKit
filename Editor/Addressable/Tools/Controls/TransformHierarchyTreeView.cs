using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

// TreeView/TreeViewItem/TreeViewState gained generic variants and the non-generic
// forms were deprecated in Unity 6000.2.
// Object.GetInstanceID was deprecated in favor of GetEntityId/EntityId in Unity 6000.4
// (Object.GetEntityId does not exist before 6000.4).
// These are two independent cutoffs, so 6000.2-6000.3 uses TreeView<int> + GetInstanceID.
#if UNITY_6000_4_OR_NEWER
using TreeViewT = UnityEditor.IMGUI.Controls.TreeView<UnityEngine.EntityId>;
using TreeViewStateT = UnityEditor.IMGUI.Controls.TreeViewState<UnityEngine.EntityId>;
using TreeViewItemT = UnityEditor.IMGUI.Controls.TreeViewItem<UnityEngine.EntityId>;
using IdT = UnityEngine.EntityId;
#elif UNITY_6000_2_OR_NEWER
using TreeViewT = UnityEditor.IMGUI.Controls.TreeView<int>;
using TreeViewStateT = UnityEditor.IMGUI.Controls.TreeViewState<int>;
using TreeViewItemT = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
using IdT = System.Int32;
#else
using TreeViewT = UnityEditor.IMGUI.Controls.TreeView;
using TreeViewStateT = UnityEditor.IMGUI.Controls.TreeViewState;
using TreeViewItemT = UnityEditor.IMGUI.Controls.TreeViewItem;
using IdT = System.Int32;
#endif

namespace ThunderKit.Addressable.Tools
{
    public class TransformHierarchyTreeView : TreeViewT
    {
        private readonly Transform root;
        private readonly TreeViewItemT localRootItem;
        private readonly Dictionary<IdT, Transform> transformLookup = new Dictionary<IdT, Transform>();

        public TransformHierarchyTreeView(TreeViewStateT state, Transform root) : base(state)
        {
            this.root = root;
            localRootItem = new TreeViewItemT { id = default, depth = -1, displayName = "root", children = new List<TreeViewItemT>() };
        }

        private static IdT GetId(UnityEngine.Object o)
        {
#if UNITY_6000_4_OR_NEWER
            return o.GetEntityId();
#else
            return o.GetInstanceID();
#endif
        }

        protected override TreeViewItemT BuildRoot()
        {
            int depth = 0;
            transformLookup.Clear();
            transformLookup[GetId(root)] = root;
            var firstItem = new TreeViewItemT {
                id = GetId(root),
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
                transformLookup[GetId(child)] = child;
                var childItem = new TreeViewItemT
                {
                    id = GetId(child),
                    depth = depth,
                    parent = parentItem,
                    displayName = child.name
                };
                ConstructTree(depth + 1, childItem, child);
                parentItem.children.Add(childItem);
            }
        }

        protected override void SingleClickedItem(IdT id)
        {
            base.SingleClickedItem(id);
            Selection.activeGameObject = transformLookup[id].gameObject;
        }

        protected override void DoubleClickedItem(IdT id)
        {
            base.DoubleClickedItem(id);
            Selection.activeGameObject = transformLookup[id].gameObject;
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.FrameSelected();
        }

    }
}
