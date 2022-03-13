using System;
using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI
{
    [CustomEditor(typeof(CgdRule))]
    [CanEditMultipleObjects]
    public class CgdRuleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var conditions = serializedObject.FindProperty(nameof(CgdRule.conditions));
            if (!serializedObject.isEditingMultipleObjects)
            {
                for (var i = 0; i < conditions.arraySize; i++)
                {
                    var element = conditions.GetArrayElementAtIndex(i);
                    var conditionType = element.FindPropertyRelative(nameof(Cgd.Condition.conditionType));
                    EditorGUILayout.PropertyField(conditionType);
                    var actualConditionType = (Cgd.ConditionType)conditionType.intValue;
                    ConcreteConditionField(actualConditionType, element);
                }
            }
            EditorGUILayout.PropertyField(conditions);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(CgdRule.parts)));
            EffectBehaviourField(serializedObject.FindProperty(nameof(CgdRule.effectBehaviour)));
            var tweeningType = serializedObject.FindProperty(nameof(CgdRule.tweeningType));
            if (tweeningType.hasMultipleDifferentValues)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(tweeningType);
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUILayout.PropertyField(tweeningType);
                if ((Cgd.TweeningType) tweeningType.intValue == Cgd.TweeningType.Custom)
                {
                    var tweening = serializedObject.FindProperty(nameof(CgdRule.tweening));
                    EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.entranceDurationSeconds)));
                    EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.shape)));
                    var importance = tweening.FindPropertyRelative(nameof(Cgd.Tweening.importance));
                    EditorGUILayout.PropertyField(importance);
                    if (importance.hasMultipleDifferentValues)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.hasCustomExitDuration)));
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        var hasCustomExitDuration = tweening.FindPropertyRelative(nameof(Cgd.Tweening.hasCustomExitDuration));
                        if (hasCustomExitDuration.hasMultipleDifferentValues)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.PropertyField(hasCustomExitDuration);
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(hasCustomExitDuration);
                            EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.entranceDurationSeconds)));
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void ConcreteConditionField(Cgd.ConditionType actualConditionType, SerializedProperty condition)
        {
            EditorGUILayout.PropertyField(condition.FindPropertyRelative(PropertyNameOf(actualConditionType)));
        }

        private static string PropertyNameOf(Cgd.ConditionType actualConditionType)
        {
            switch (actualConditionType)
            {
                case Cgd.ConditionType.HandGesture: return nameof(Cgd.Condition.handGesture);
                case Cgd.ConditionType.DefaultMoodSelector: return nameof(Cgd.Condition.defaultMoodSelector);
                case Cgd.ConditionType.SpecificMoodSelector: return nameof(Cgd.Condition.specificMoodSelector);
                case Cgd.ConditionType.ParameterBoolValue: return nameof(Cgd.Condition.parameterBoolValue);
                case Cgd.ConditionType.ParameterIntValue: return nameof(Cgd.Condition.parameterIntValue);
                case Cgd.ConditionType.ParameterFloatValue: return nameof(Cgd.Condition.parameterFloatValue);
                case Cgd.ConditionType.ParameterIntBetween: return nameof(Cgd.Condition.parameterIntBetween);
                case Cgd.ConditionType.ParameterFloatBetween: return nameof(Cgd.Condition.parameterFloatBetween);
                case Cgd.ConditionType.ConditionFromComponent: return nameof(Cgd.Condition.conditionFromComponent);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EffectBehaviourField(SerializedProperty effectBehaviour)
        {
            var effectBehaviourType = effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.effectBehaviourType));
            if (!effectBehaviourType.hasMultipleDifferentValues)
            {
                EditorGUILayout.PropertyField(effectBehaviourType);
                var actualEffectBehaviourType = (Cgd.EffectBehaviourType)effectBehaviourType.intValue;
                ConcreteEffectField(actualEffectBehaviourType, effectBehaviour);
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(effectBehaviourType);
                EditorGUI.EndDisabledGroup();
            }
        }

        private void ConcreteEffectField(Cgd.EffectBehaviourType effectBehaviourType, SerializedProperty effectBehaviour)
        {
            switch (effectBehaviourType)
            {
                case Cgd.EffectBehaviourType.Normal:
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.expression)));
                    break;
                case Cgd.EffectBehaviourType.Analog:
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.analogParameterName)));
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.analogMin)));
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.analogMax)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effectBehaviourType), effectBehaviourType, null);
            }
        }
    }
}