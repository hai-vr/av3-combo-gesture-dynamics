using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    internal class CgdEditorConfigurationLayout
    {
        private readonly CgdEditor _cgdEditor;
        private Rect m_focusAreaRect;

        public CgdEditorConfigurationLayout(CgdEditor cgdEditor)
        {
            _cgdEditor = cgdEditor;
        }

        public void Layout()
        {
            EditorGUILayout.BeginVertical("GroupBox");
            CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 7, EditorGUIUtility.singleLineHeight * 7), rect => m_focusAreaRect = rect);
            GUILayout.BeginArea(m_focusAreaRect);
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Component)), _cgdEditor.cgd, typeof(Components.ComboGestureDynamics), true);
            EditorGUI.EndDisabledGroup();

            var cgdSerialized = new SerializedObject(_cgdEditor.cgd);
            EditorGUILayout.PropertyField(cgdSerialized.FindProperty(nameof(Components.ComboGestureDynamics.avatar)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.AvatarDescriptor)));
            cgdSerialized.ApplyModifiedProperties();

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            EditorGUILayout.EndVertical();
        }
    }
}