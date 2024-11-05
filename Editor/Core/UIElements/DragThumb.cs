using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using static UnityEngine.GraphicsBuffer;
#else
using UnityEngine.Experimental.UIElements;
#endif

namespace ThunderKit.Core.UIElements
{
    public enum RelativePosition { Left, Right };
    public class DragThumb : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<DragThumb, UxmlTraits>
        {
        }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription m_targetName = new UxmlStringAttributeDescription { name = "target-name" };
            private readonly UxmlEnumAttributeDescription<RelativePosition> m_relativePosition = new UxmlEnumAttributeDescription<RelativePosition> { name = "target-relation" };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var dragThumb = (DragThumb)ve;
                dragThumb.targetName = m_targetName.GetValueFromBag(bag, cc);
                dragThumb.TargetRelation = m_relativePosition.GetValueFromBag(bag, cc);
            }


            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription =>
                Enumerable.Empty<UxmlChildElementDescription>();
        }
        private string targetName;
        public VisualElement Target
        {
            get => target;
            set
            {
                target = value;
                if (dragManipulator == null) dragManipulator = new DragManipulator(this, value);

                dragManipulator.resizeTarget = value;
            }
        }
        public RelativePosition TargetRelation
        {
            get => targetRelation;
            set => targetRelation = value;
        }

        private DragManipulator dragManipulator;
        private VisualElement target;
        private RelativePosition targetRelation;

        public DragThumb()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttached);

        }
        void OnAttached(AttachToPanelEvent evt)
        {
            Target = parent.Q(targetName);
        }

    }

    public class DragManipulator : Manipulator
    {
        private bool enabled { get; set; }

        public VisualElement resizeTarget { get; set; }

        public DragManipulator(VisualElement target, VisualElement resizeTarget)
        {
            this.target = target;
            resizeTarget = target.parent;
        }
#if UNITY_2019_3_OR_NEWER
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(PointerDownHandler);
            target.RegisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.RegisterCallback<PointerUpEvent>(PointerUpHandler);
            target.RegisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(PointerDownHandler);
            target.UnregisterCallback<PointerMoveEvent>(PointerMoveHandler);
            target.UnregisterCallback<PointerUpEvent>(PointerUpHandler);
            target.UnregisterCallback<PointerCaptureOutEvent>(PointerCaptureOutHandler);
        }

        private void PointerDownHandler(PointerDownEvent evt)
        {
            target.CapturePointer(evt.pointerId);
            enabled = true;
        }

        private void PointerMoveHandler(PointerMoveEvent evt)
        {
            if (!enabled || !target.HasPointerCapture(evt.pointerId)) return;

            resizeTarget.style.width = new StyleLength(resizeTarget.resolvedStyle.width + evt.deltaPosition.x);
        }

        private void PointerUpHandler(PointerUpEvent evt)
        {
            if (enabled && target.HasPointerCapture(evt.pointerId))
                target.ReleasePointer(evt.pointerId);
        }

        private void PointerCaptureOutHandler(PointerCaptureOutEvent evt)
        {
            if (enabled)
                enabled = false;
        }
#else
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(MouseDownHandler);
            target.RegisterCallback<MouseMoveEvent>(MouseMoveHandler);
            target.RegisterCallback<MouseUpEvent>(MouseUpHandler);
            target.RegisterCallback<MouseCaptureOutEvent>(MouseCaptureOutHandler);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(MouseDownHandler);
            target.UnregisterCallback<MouseMoveEvent>(MouseMoveHandler);
            target.UnregisterCallback<MouseUpEvent>(MouseUpHandler);
            target.UnregisterCallback<MouseCaptureOutEvent>(MouseCaptureOutHandler);
        }

        private void MouseDownHandler(MouseDownEvent evt)
        {
            target.CaptureMouse();
            enabled = true;
        }

        private void MouseMoveHandler(MouseMoveEvent evt)
        {
            if (!enabled || !target.HasMouseCapture()) return;
            var width = resizeTarget.layout.width + evt.mouseDelta.x;
            resizeTarget.style.width = width;
        }

        private void MouseUpHandler(MouseUpEvent evt)
        {
            if (enabled && target.HasMouseCapture())
                target.ReleaseMouse();
        }

        private void MouseCaptureOutHandler(MouseCaptureOutEvent evt)
        {
            if (enabled)
                enabled = false;
        }
#endif
    }
}