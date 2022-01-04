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
                    EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.durationSeconds)));
                    EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.shape)));
                    EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.importance)));
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static void ConcreteConditionField(Cgd.ConditionType actualConditionType, SerializedProperty condition)
        {
            switch (actualConditionType)
            {
                case Cgd.ConditionType.HandGesture:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.handGesture)));
                    break;
                case Cgd.ConditionType.DefaultMoodSelector:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.defaultMoodSelector)));
                    break;
                case Cgd.ConditionType.SpecificMoodSelector:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.specificMoodSelector)));
                    break;
                case Cgd.ConditionType.ParameterBoolValue:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.parameterBoolValue)));
                    break;
                case Cgd.ConditionType.ParameterIntValue:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.parameterIntValue)));
                    break;
                case Cgd.ConditionType.ParameterFloatValue:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.parameterFloatValue)));
                    break;
                case Cgd.ConditionType.ParameterIntBetween:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.parameterIntBetween)));
                    break;
                case Cgd.ConditionType.ParameterFloatBetween:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.parameterFloatBetween)));
                    break;
                case Cgd.ConditionType.ConditionFromComponent:
                    EditorGUILayout.PropertyField(condition.FindPropertyRelative(nameof(Cgd.Condition.conditionFromComponent)));
                    break;
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
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.effect)));
                    break;
                case Cgd.EffectBehaviourType.Analog:
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.analogParameterName)));
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.analogMin)));
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.analogMax)));
                    break;
                case Cgd.EffectBehaviourType.AnyFist:
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.fist)));
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.restOptional)));
                    break;
                case Cgd.EffectBehaviourType.BothFists:
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.bothFists)));
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.leftFist)));
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.rightFist)));
                    EditorGUILayout.PropertyField(effectBehaviour.FindPropertyRelative(nameof(Cgd.EffectBehaviour.restOptional)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effectBehaviourType), effectBehaviourType, null);
            }
        }
    }
    /*
    [CustomPropertyDrawer(typeof(Cgd.Condition))]
    public class CgdConditionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUILayout.PropertyField(property.FindPropertyRelative(nameof(Cgd.Condition.conditionType)), new GUIContent("Condition Type"));
        // public ConditionType conditionType;
        // public HandGesture handGesture;
        // public DefaultMoodSelector defaultMoodSelector;
        // public SpecificMoodSelector specificMoodSelector;
        // public ParameterBoolValue parameterBoolValue;
        // public ParameterIntValue parameterIntValue;
        // public ParameterFloatValue parameterFloatValue;
        // public ParameterIntBetween parameterIntBetween;
        // public ParameterFloatBetween parameterFloatBetween;
            EditorGUI.EndProperty();
        }
    }
    */
}