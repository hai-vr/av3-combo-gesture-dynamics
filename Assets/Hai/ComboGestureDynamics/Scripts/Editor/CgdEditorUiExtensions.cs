using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Hai.ComboGestureDynamics.Scripts.Components;
using Hai.ComboGestureDynamics.Scripts.Editor.EditorUI;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdEditorUiExtensions
    {
        public static EditorCurveBinding[] FindAllProperties(Components.ComboGestureDynamics dynamics)
        {
            return FindAllEffects(dynamics)
                .SelectMany(FindAllPropertiesOfEffect)
                .Distinct()
                .OrderBy(binding => binding.path)
                .ThenBy(binding => binding.type)
                .ThenBy(binding => binding.propertyName)
                .ToArray();
        }

        private static EditorCurveBinding[] FindAllPropertiesOfEffect(CgdEffect effect)
        {
            switch (effect.effectType)
            {
                case Cgd.EffectType.Regular:
                    var inheritedProperties = FindAllInheritedEffectsProperties(effect);
                    var insertedProperties = FindAllInsertedClipsProperties(effect);
                    var propertyMasks = FindAllDirectProperties(effect);
                    return new[] {inheritedProperties, insertedProperties, propertyMasks}.SelectMany(bindings => bindings).ToArray();
                case Cgd.EffectType.Blend:
                    return effect.blend.positions
                        .SelectMany(position => FindAllPropertiesOfEffect(position.effect))
                        .ToArray();
                case Cgd.EffectType.Custom:
                    return AllAnimationsOf((BlendTree) effect.customBlendTree)
                        .SelectMany(AnimationUtility.GetCurveBindings)
                        .ToArray();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static EditorCurveBinding[] FindAllDirectProperties(CgdEffect effect)
        {
            return effect.regular.properties.Select(value => value.property.ToEditorCurveBinding()).ToArray();
        }

        private static EditorCurveBinding[] FindAllInsertedClipsProperties(CgdEffect effect)
        {
            return effect.regular.insertedClips
                .SelectMany(insertedClip => AnimationUtility.GetCurveBindings(insertedClip.clip)
                    .Where(binding => !insertedClip.rejectedProperties.Any(mask => mask.IsSameAsBinding(binding)))
                    .ToArray())
                .ToArray();
        }

        private static EditorCurveBinding[] FindAllInheritedEffectsProperties(CgdEffect effect)
        {
            return effect.regular.inheritedEffects
                .SelectMany(inheritedEffect => FindAllPropertiesOfEffect(inheritedEffect.effect)
                    .Where(binding => !inheritedEffect.rejectedProperties.Any(mask => mask.IsSameAsBinding(binding)))
                    .ToArray())
                .ToArray();
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

        private static CgdEffect[] FindAllEffects(Components.ComboGestureDynamics dynamics)
        {
            var mutableEffects = new List<CgdEffect>();
            Iterate(mutableEffects, dynamics.rootRule.transform);
            mutableEffects.AddRange(dynamics.rootRule.effectBehaviour.DefensiveActiveEffects());
            return mutableEffects.ToArray();
        }

        private static void Iterate(List<CgdEffect> mutableEffectsResult, Transform transformToIterate)
        {
            foreach (Transform transforms in transformToIterate)
            {
                var rule = transforms.GetComponent<CgdRule>();
                if (rule == null) continue;

                Iterate(mutableEffectsResult, transforms);
                mutableEffectsResult.AddRange(rule.effectBehaviour.DefensiveActiveEffects());
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
                    return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionSpecificMoodSelector, condition.specificMoodSelector.moodSelector.name, condition.specificMoodSelector.selection);
                case Cgd.ConditionType.ParameterBoolValue:
                    return condition.parameterBoolValue.value
                        ? CgdLocalization.Localize(CgdLocalization.Phrase.BoolIsTrue, condition.parameterBoolValue.parameterName)
                        : CgdLocalization.Localize(CgdLocalization.Phrase.BoolIsFalse, condition.parameterBoolValue.parameterName);
                case Cgd.ConditionType.ParameterIntValue:
                    var intOperation = condition.parameterIntValue.operation;
                    var intValue = NoCulture(condition.parameterIntValue.value);
                    switch (intOperation)
                    {
                        case Cgd.ParameterIntValue.IntOperation.Equal:
                            return condition.parameterIntValue.parameterName + " = " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.NotEqual:
                            return condition.parameterIntValue.parameterName + " ≠ " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.StrictlyGreaterThan:
                            return condition.parameterIntValue.parameterName + " > " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.StrictlyLessThan:
                            return condition.parameterIntValue.parameterName + " < " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.GreaterOrEqualTo:
                            return condition.parameterIntValue.parameterName + " ≥ " + intValue;
                        case Cgd.ParameterIntValue.IntOperation.LessOrEqualTo:
                            return condition.parameterIntValue.parameterName + " ≤ " + intValue;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case Cgd.ConditionType.ParameterFloatValue:
                    var floatOperation = condition.parameterFloatValue.operation;
                    var floatValue = NoCulture(condition.parameterFloatValue.value);
                    switch (floatOperation)
                    {
                        case Cgd.ParameterFloatValue.FloatOperation.StrictlyGreaterThan:
                            return condition.parameterFloatValue.parameterName + " > " + floatValue;
                            break;
                        case Cgd.ParameterFloatValue.FloatOperation.StrictlyLessThan:
                            return condition.parameterFloatValue.parameterName + " < " + floatValue;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                case Cgd.ConditionType.ParameterIntBetween:
                    return NoCulture(condition.parameterIntBetween.lowerBoundInclusive) + " ≤ " + condition.parameterIntBetween.parameterName + " ≤ " + NoCulture(condition.parameterIntBetween.upperBoundInclusive);
                case Cgd.ConditionType.ParameterFloatBetween:
                    return NoCulture(condition.parameterFloatBetween.lowerBoundExclusive) + " < " + condition.parameterFloatBetween.parameterName + " < " + NoCulture(condition.parameterFloatBetween.upperBoundExclusive);
                case Cgd.ConditionType.ConditionFromComponent:
                    return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionConditionFromComponent, condition.conditionFromComponent.conditionComponent.name); // TODO: Describe this
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

        // public static string LocalizeCondition(Cgd.Condition condition)
        // {
        //     switch (condition.conditionType)
        //     {
        //         case Cgd.ConditionType.HandGesture:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionHandGesture, condition.handGesture.side, condition.handGesture.pose); // TODO: Hand gestures need to be remapped to localized
        //         case Cgd.ConditionType.DefaultMoodSelector:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionDefaultMoodSelector);
        //         case Cgd.ConditionType.SpecificMoodSelector:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionSpecificMoodSelector, condition.specificMoodSelector.moodSelector.name);
        //         case Cgd.ConditionType.ParameterBoolValue:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionParameterBoolValue, condition.parameterBoolValue.parameterName, condition.parameterBoolValue.value);
        //         case Cgd.ConditionType.ParameterIntValue:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionParameterIntValue, condition.parameterIntValue.parameterName, condition.parameterIntValue.operation, condition.parameterIntValue.value);
        //         case Cgd.ConditionType.ParameterFloatValue:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionParameterFloatValue, condition.parameterFloatValue.parameterName, condition.parameterFloatValue.operation, condition.parameterFloatValue.value);
        //         case Cgd.ConditionType.ParameterIntBetween:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionParameterIntBetween, condition.parameterIntBetween.parameterName, condition.parameterIntBetween.lowerBoundInclusive, condition.parameterIntBetween.upperBoundInclusive);
        //         case Cgd.ConditionType.ParameterFloatBetween:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionParameterFloatBetween, condition.parameterFloatBetween.parameterName, condition.parameterFloatBetween.lowerBoundExclusive, condition.parameterFloatBetween.upperBoundExclusive);
        //         case Cgd.ConditionType.ConditionFromComponent:
        //             return CgdLocalization.Localize(CgdLocalization.Phrase.ConditionConditionFromComponent, condition.conditionFromComponent.conditionComponent.name); // TODO: Describe this
        //         default:
        //             throw new ArgumentOutOfRangeException();
        //     }
        // }
        public static void RectOnRepaint(Func<Rect> rectFn, Action<Rect> applyFn)
        {
            var rect = rectFn();
            if (Event.current.type == EventType.Repaint)
            {
                // https://answers.unity.com/questions/515197/how-to-use-guilayoututilitygetrect-properly.html
                applyFn(rect);
            }
        }
    }
}