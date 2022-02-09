using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    internal class CgdEditorEffectsLayout
    {
        private readonly CgdEditor _cgdEditor;
        private Rect m_focusAreaRect;

        private CgdEffect _selectedEffect;
        private ReorderableList inheritedEffectsRList;
        private ReorderableList insertedClipsRList;
        private SerializedObject _effectSerialized;

        public CgdEditorEffectsLayout(CgdEditor cgdEditor)
        {
            _cgdEditor = cgdEditor;
        }

        public void Layout()
        {
            EditorGUILayout.BeginVertical("GroupBox");
            CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 7, EditorGUIUtility.singleLineHeight * 7), rect => m_focusAreaRect = rect);
            GUILayout.BeginArea(m_focusAreaRect);
            EditorGUILayout.BeginVertical();

            var newEffect = EditorGUILayout.ObjectField(_selectedEffect, typeof(CgdEffect));
            if (newEffect != _selectedEffect)
            {
                if (newEffect == null)
                {
                    DeselectEffect();
                }
                else
                {
                    SelectEffect((CgdEffect)newEffect);
                }
            }

            if (_effectSerialized != null)
            {
                _effectSerialized.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("GroupBox");
            if (inheritedEffectsRList != null)
            {
                inheritedEffectsRList.DoLayoutList();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("GroupBox");
            if (insertedClipsRList != null)
            {
                insertedClipsRList.DoLayoutList();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void SelectEffect(CgdEffect effect)
        {
            _selectedEffect = effect;

            // reference: https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
            _effectSerialized = new SerializedObject(_selectedEffect);
            inheritedEffectsRList = new ReorderableList(
                _effectSerialized,
                _effectSerialized.FindProperty(nameof(CgdEffect.regular)).FindPropertyRelative(nameof(Cgd.Regular.inheritedEffects)),
                true, false, true, true
            );
            inheritedEffectsRList.drawElementCallback = InheritedEffectsRListElement;
            inheritedEffectsRList.elementHeight = EditorGUIUtility.singleLineHeight;
            insertedClipsRList = new ReorderableList(
                _effectSerialized,
                _effectSerialized.FindProperty(nameof(CgdEffect.regular)).FindPropertyRelative(nameof(Cgd.Regular.insertedClips)),
                true, false, true, true
            );
            insertedClipsRList.drawElementCallback = InsertedClipsRListElement;
            insertedClipsRList.elementHeight = EditorGUIUtility.singleLineHeight;
        }

        private void DeselectEffect()
        {
            _selectedEffect = null;
            _effectSerialized = null;
            inheritedEffectsRList = null;
            insertedClipsRList = null;
        }

        private void InheritedEffectsRListElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            var element = inheritedEffectsRList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative(nameof(Cgd.InheritedEffect.effect)));
        }

        private void InsertedClipsRListElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            var element = insertedClipsRList.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative(nameof(Cgd.InsertedClip.clip)));
        }
    }
}