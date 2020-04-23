using RainOfStages.Behaviours;
using UnityEditor;
using UnityEngine;

namespace RainOfStages.Editor
{
    [CustomEditor(typeof(JumpPad)), CanEditMultipleObjects]
    public class JumpPadEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            JumpPad jumpPad = (JumpPad)target;

            EditorGUI.BeginChangeCheck();
            Vector3 newTargetPosition = Handles.PositionHandle(jumpPad.destination, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(jumpPad, "Change JumpPad Target Position");
                jumpPad.destination = newTargetPosition;
            }
        }
    }
}