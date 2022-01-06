using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    internal class CgdEditorPartsLayout
    {
        private readonly CgdEditor _cgdEditor;
        private Rect m_focusAreaRect;

        public CgdEditorPartsLayout(CgdEditor cgdEditor)
        {
            _cgdEditor = cgdEditor;
        }

        public void Layout()
        {
            EditorGUILayout.BeginVertical("GroupBox");
            CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 7, EditorGUIUtility.singleLineHeight * 7), rect => m_focusAreaRect = rect);
            GUILayout.BeginArea(m_focusAreaRect);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("GroupBox");
            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.MainPart), EditorStyles.boldLabel);

            var editorCurveBindings = CgdEditorUiExtensions.FindAllProperties(_cgdEditor.cgd);
            foreach (var editorCurveBinding in editorCurveBindings)
            {
                DisplayBinding(editorCurveBinding);
            }

            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.SecondaryParts), EditorStyles.boldLabel);
            GUILayout.EndVertical();
        }

        private void DisplayBinding(EditorCurveBinding editorCurveBinding)
        {
            var type = editorCurveBinding.type == typeof(SkinnedMeshRenderer) && editorCurveBinding.propertyName.StartsWith("blendShape") ? "::" : $"({editorCurveBinding.type})";
            var existsInAvatar = AnimationUtility.GetFloatValue(_cgdEditor.cgd.avatar.gameObject, editorCurveBinding, out var valueInAvatarOrZero);
            var value = existsInAvatar ? $"= {CgdEditorUiExtensions.NoCulture(valueInAvatarOrZero)}" : "???";

            EditorGUILayout.BeginHorizontal();
            var animatedObject = AnimationUtility.GetAnimatedObject(_cgdEditor.cgd.avatar.gameObject, editorCurveBinding);
            EditorGUILayout.LabelField($"{editorCurveBinding.path} {type}");
            EditorGUILayout.LabelField(editorCurveBinding.propertyName);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(animatedObject, typeof(Object));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField(value);
            EditorGUILayout.EndHorizontal();
            // EditorGUILayout.LabelField(content);
        }
    }
}