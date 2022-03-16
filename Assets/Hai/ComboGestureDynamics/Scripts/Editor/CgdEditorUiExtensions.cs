using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdEditorUiExtensions
    {
        public static EditorCurveBinding[] FindAllProperties(Components.ComboGestureDynamics dynamics)
        {
            return FindAllExpressions(dynamics)
                .SelectMany(FindAllPropertiesOfExpression)
                .Distinct()
                .OrderBy(binding => binding.path)
                .ThenBy(binding => binding.type)
                .ThenBy(binding => binding.propertyName)
                .ToArray();
        }

        private static EditorCurveBinding[] FindAllPropertiesOfExpression(Motion expression)
        {
            switch (expression)
            {
                case BlendTree blend:
                    return AllAnimationsOf(blend)
                        .SelectMany(FindAllPropertiesOfExpression)
                        .ToArray();
                case AnimationClip clip:
                    return AnimationUtility.GetCurveBindings(clip);
                default:
                    return new EditorCurveBinding[0];
            }
        }

        private static EditorCurveBinding ToEditorCurveBinding(Cgd.PropertyMask propertyMask)
        {
            return new EditorCurveBinding
            {
                path = propertyMask.path,
                type = propertyMask.type,
                propertyName = propertyMask.propertyName
            };
        }

        private static AnimationClip[] AllAnimationsOf(BlendTree blendTree)
        {
            return blendTree.children.SelectMany(childMotion =>
                {
                    switch (childMotion.motion)
                    {
                        case BlendTree subTree:
                            return AllAnimationsOf(subTree);
                        case AnimationClip clip:
                            return new[] {clip};
                        default:
                            return new AnimationClip[] { };
                    }
                })
                .ToArray();
        }

        private static Motion[] FindAllExpressions(Components.ComboGestureDynamics dynamics)
        {
            var mutableExpressions = new List<Motion>();
            Iterate(mutableExpressions, dynamics.rootRule.transform);
            mutableExpressions.AddRange(dynamics.rootRule.effectBehaviour.DefensiveActiveExpressions());
            return mutableExpressions.ToArray();
        }

        private static void Iterate(List<Motion> mutableEffectsResult, Transform transformToIterate)
        {
            foreach (Transform transforms in transformToIterate)
            {
                var rule = transforms.GetComponent<CgdRule>();
                if (rule == null) continue;

                Iterate(mutableEffectsResult, transforms);
                mutableEffectsResult.AddRange(rule.effectBehaviour.DefensiveActiveExpressions());
            }
        }

        public static string LocalizeCondition(Cgd.Condition condition)
        {
            switch (condition.conditionType)
            {
                case Cgd.ConditionType.HandGesture:
                    return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionHandGesture, condition.handGesture.side, condition.handGesture.pose); // TODO: Hand gestures need to be remapped to localized
                case Cgd.ConditionType.DefaultMoodSelector:
                    return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionDefaultMoodSelector, condition.defaultMoodSelector.selection);
                case Cgd.ConditionType.SpecificMoodSelector:
                    return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionSpecificMoodSelector, condition.specificMoodSelector.moodSelector?.name, condition.specificMoodSelector.selection); // Defensive due to use in editor
                case Cgd.ConditionType.ParameterBoolValue:
                    return condition.parameterBoolValue.value
                        ? CgdLocalization.Localize(CgdLocalization.Phrase.BoolIsTrue, condition.parameterName)
                        : CgdLocalization.Localize(CgdLocalization.Phrase.BoolIsFalse, condition.parameterName);
                case Cgd.ConditionType.ParameterIntValue:
                    var intOperation = condition.parameterIntValue.operation;
                    var intValue = NoCulture(condition.parameterIntValue.value);
                    switch (intOperation)
                    {
                        case Cgd.ParameterIntValue.IntOperation.Equal:
                            return condition.parameterName + " = " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.NotEqual:
                            return condition.parameterName + " ≠ " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.StrictlyGreaterThan:
                            return condition.parameterName + " > " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.StrictlyLessThan:
                            return condition.parameterName + " < " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.GreaterOrEqualTo:
                            return condition.parameterName + " ≥ " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.LessOrEqualTo:
                            return condition.parameterName + " ≤ " + intValue;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case Cgd.ConditionType.ParameterFloatValue:
                    var floatOperation = condition.parameterFloatValue.operation;
                    var floatValue = NoCulture(condition.parameterFloatValue.value);
                    switch (floatOperation)
                    {
                        case Cgd.ParameterFloatValue.FloatOperation.StrictlyGreaterThan:
                            return condition.parameterName + " > " + floatValue;
                            break;
                        case Cgd.ParameterFloatValue.FloatOperation.StrictlyLessThan:
                            return condition.parameterName + " < " + floatValue;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case Cgd.ConditionType.ParameterIntBetween:
                    return NoCulture(condition.parameterIntBetween.lowerBoundInclusive) + " ≤ " + condition.parameterName + " ≤ " + NoCulture(condition.parameterIntBetween.upperBoundInclusive);
                case Cgd.ConditionType.ParameterFloatBetween:
                    return NoCulture(condition.parameterFloatBetween.lowerBoundExclusive) + " < " + condition.parameterName + " < " + NoCulture(condition.parameterFloatBetween.upperBoundExclusive);
                case Cgd.ConditionType.ConditionFromComponent:
                    return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionConditionFromComponent, condition.conditionFromComponent.conditionComponent?.name); // Defensive due to use in editor // TODO: Describe this
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string NoCulture(int obj)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}", obj);
        }

        public static string NoCulture(float obj)
        {
            // At least one decimal place
            return string.Format(CultureInfo.InvariantCulture, "{0:0.0###########}", obj);
        }

        public static void RectOnRepaint(Func<Rect> rectFn, Action<Rect> applyFn)
        {
            var rect = rectFn();
            if (Event.current.type == EventType.Repaint)
            {
                // https://answers.unity.com/questions/515197/how-to-use-guilayoututilitygetrect-properly.html
                applyFn(rect);
            }
        }

        public static CgdPermutationRuleset[] FindPermutationRulesets(Components.ComboGestureDynamics cgd)
        {
            var mutableResults = new List<CgdPermutationRuleset>();
            FindPermutationRulesetsInTransform(mutableResults, cgd.rootRule.transform);
            return mutableResults.ToArray();
        }

        private static void FindPermutationRulesetsInTransform(List<CgdPermutationRuleset> mutableResults, Transform ruleTransform)
        {
            foreach (Transform child in ruleTransform)
            {
                var ruleset = child.GetComponent<CgdPermutationRuleset>();
                if (ruleset != null)
                {
                    mutableResults.Add(ruleset);
                }
                else
                {
                    var rule = child.GetComponent<CgdRule>();
                    if (rule != null)
                    {
                        FindPermutationRulesetsInTransform(mutableResults, child);
                    }
                }
            }
        }

        public static void TweeningBox(SerializedProperty tweeningType, SerializedProperty tweening)
        {
            LocalizedEnumPropertyField(tweeningType, new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.TweeningType)), typeof(Cgd.TweeningType));
            if (!tweeningType.hasMultipleDifferentValues && (Cgd.TweeningType) tweeningType.intValue == Cgd.TweeningType.Custom)
            {
                TweeningInner(tweening);
            }
        }

        public static void TweeningInner(SerializedProperty tweening)
        {
            LocalizedEnumPropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.shape)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.TweeningShape)), typeof(Cgd.Shape));
            EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.entranceDurationSeconds)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.TweeningEntranceDurationSeconds)));

            var importance = tweening.FindPropertyRelative(nameof(Cgd.Tweening.importance));
            LocalizedEnumPropertyField(importance, new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.TweeningImportance)), typeof(Cgd.Importance));
            if (!importance.hasMultipleDifferentValues && importance.boolValue)
            {
                var hasCustomExitDuration = tweening.FindPropertyRelative(nameof(Cgd.Tweening.hasCustomExitDuration));
                EditorGUILayout.PropertyField(hasCustomExitDuration, new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.TweeningHasCustomExitDuration)));

                if (!hasCustomExitDuration.hasMultipleDifferentValues && hasCustomExitDuration.boolValue)
                {
                    EditorGUILayout.PropertyField(tweening.FindPropertyRelative(nameof(Cgd.Tweening.exitDurationSeconds)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.TweeningExitDurationSeconds)));
                }
            }
        }

        public static void LocalizedEnumPropertyField(SerializedProperty serializedProperty, GUIContent guiContent, Type enumType, params GUILayoutOption[] guiLayoutOptions)
        {
            var names = LocalizedEnumNames(enumType);
            if (serializedProperty.hasMultipleDifferentValues)
            {
                var shifted = EditorGUILayout.Popup(guiContent, 0, new[] {""}.Concat(names).ToArray(), guiLayoutOptions);
                if (shifted != 0)
                {
                    var choice = shifted - 1;
                    serializedProperty.intValue = choice;
                }
            }
            else
            {
                var indexed = EditorGUILayout.Popup(guiContent, serializedProperty.intValue, names.ToArray(), guiLayoutOptions);
                if (indexed != serializedProperty.intValue)
                {
                    serializedProperty.intValue = indexed;
                }
            }
        }

        public static void LocalizedEnumPropertyFieldNonLayout(Rect rect, SerializedProperty serializedProperty, GUIContent guiContent, Type enumType)
        {
            var names = LocalizedEnumNames(enumType);
            if (serializedProperty.hasMultipleDifferentValues)
            {
                var shifted = EditorGUI.Popup(rect, guiContent, 0, new[] {""}.Concat(names).Select(s => new GUIContent(s)).ToArray());
                if (shifted != 0)
                {
                    var choice = shifted - 1;
                    serializedProperty.intValue = choice;
                }
            }
            else
            {
                var indexed = EditorGUI.Popup(rect, guiContent, serializedProperty.intValue, names.Select(s => new GUIContent(s)).ToArray());
                if (indexed != serializedProperty.intValue)
                {
                    serializedProperty.intValue = indexed;
                }
            }
        }

        private static string[] LocalizedEnumNames(Type enumType)
        {
            return Enum.GetValues(enumType).Cast<Enum>().ToArray()
                .Select(enumValue => CgdLocalization.EnumLocalize(enumValue, enumType))
                .ToArray();
        }
    }
}