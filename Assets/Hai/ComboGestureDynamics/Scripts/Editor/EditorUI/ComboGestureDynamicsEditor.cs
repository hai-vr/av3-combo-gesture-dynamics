using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(Components.ComboGestureDynamics))]
    [CanEditMultipleObjects]
    public class ComboGestureDynamicsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!serializedObject.isEditingMultipleObjects && GUILayout.Button("Compile"))
            {
                new CgdCompiler((Components.ComboGestureDynamics) target).Compile();
            }
        }
    }
}