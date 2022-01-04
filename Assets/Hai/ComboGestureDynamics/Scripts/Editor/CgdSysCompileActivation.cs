﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdSysCompileActivation
    {
        private readonly Components.ComboGestureDynamics _cgd;
        private readonly CgdParameters _cgdParameters;

        public CgdSysCompileActivation(Components.ComboGestureDynamics cgd, CgdParameters cgdParameters)
        {
            _cgd = cgd;
            _cgdParameters = cgdParameters;
        }

        public CgdSys.Activation[] FlattenRulesToActivations(CgdRootRule rootRule, CgdPart part, bool isFirstPart)
        {
            var mutableActivations = new List<CgdSys.Activation>();
            var currentTweening = Remap(rootRule.tweening);

            // FIXME: Maybe parts should be removed at the last step...?
            var whenFirstPartCreateDenyList = isFirstPart ? _cgd.secondaryParts.SelectMany(cgdPart => cgdPart.acceptedProperties).Distinct().ToArray() : null;
            var compiledEffect = new CgdSysCompileEffect(rootRule.effectBehaviour, part, null, whenFirstPartCreateDenyList, _cgdParameters).Compile();

            foreach (Transform child in rootRule.transform)
            {
                var rule = child.GetComponent<CgdRule>();
                if (rule == null) continue;

                var activations = FlattenRulesBelongingToPart(rule, part, currentTweening, new CgdSys.ICondition[0], compiledEffect, whenFirstPartCreateDenyList);
                mutableActivations.AddRange(activations);
            }

            mutableActivations.Add(new CgdSys.Activation
            {
                conditions = new CgdSys.ICondition[0],
                tweening = currentTweening,
                compiledEffect = compiledEffect
            });

            return mutableActivations.ToArray();
        }

        private CgdSys.Activation[] FlattenRulesBelongingToPart(CgdRule currentRule, CgdPart part, CgdSys.Tweening inheritedTweening, CgdSys.ICondition[] inheritedConditions, CgdSys.CompiledEffect previousEffect, Cgd.PropertyMask[] whenFirstPartCreateDenyList)
        {
            var mutableActivations = new List<CgdSys.Activation>();

            var compiledEffect = new CgdSysCompileEffect(currentRule.effectBehaviour, part, previousEffect, whenFirstPartCreateDenyList, _cgdParameters).Compile();

            var currentTweening = currentRule.tweeningType == Cgd.TweeningType.Inherited ? inheritedTweening : Remap(currentRule.tweening);
            var currentConditions = BuildConditions(currentRule, inheritedConditions);

            foreach (Transform child in currentRule.transform)
            {
                var childRule = child.GetComponent<CgdRule>();
                if (childRule == null) continue;

                mutableActivations.AddRange(FlattenRulesBelongingToPart(childRule, part, currentTweening, currentConditions, compiledEffect, whenFirstPartCreateDenyList));
            }

            if (currentRule.parts.Length == 0 || currentRule.parts.Contains(part))
            {
                mutableActivations.Add(new CgdSys.Activation
                {
                    conditions = currentConditions,
                    tweening = currentTweening,
                    compiledEffect = compiledEffect
                });
            }

            if (currentConditions.Any(condition => condition is CgdSys.AnyHandSideWithPoseCondition))
            {
                // TODO: Make big split of Any into Left, Right, and Both
            }

            return mutableActivations.ToArray();
        }

        private CgdSys.ICondition[] BuildConditions(CgdRule rule, CgdSys.ICondition[] inheritedConditions)
        {
            var solvedConditions = SolveConditionsPertainingTo(rule);
            return inheritedConditions.Concat(solvedConditions).ToArray();
        }

        private CgdSys.ICondition[] SolveConditionsPertainingTo(CgdRule rule)
        {
            var mutableTraversal = new List<CgdCondition>();
            return rule.conditions.SelectMany(condition => BuildIntermediaryCondition(condition, mutableTraversal)).ToArray();
        }

        private IEnumerable<CgdSys.ICondition> BuildIntermediaryCondition(Cgd.Condition condition, List<CgdCondition> mutableVisitedConditions)
        {
            switch (condition.conditionType)
            {
                case Cgd.ConditionType.HandGesture:
                    return HandGestureCondition(condition.handGesture);
                case Cgd.ConditionType.DefaultMoodSelector:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.IntCondition
                        {
                            key = _cgdParameters.DefaultMoodSelector,
                            value = _cgd.moodSelector.choices
                                .Select((choice, i) => choice.parameterName == condition.defaultMoodSelector.selection ? i : -1)
                                .Max()
                        }
                    };
                case Cgd.ConditionType.SpecificMoodSelector:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.IntCondition
                        {
                            key = condition.specificMoodSelector.moodSelector == _cgd.moodSelector
                                ? _cgdParameters.DefaultMoodSelector
                                : _cgdParameters.SpecificMoodSelector(condition.specificMoodSelector),
                            value = _cgd.moodSelector.choices
                                .Select((choice, i) => choice.parameterName == condition.defaultMoodSelector.selection ? i : -1)
                                .Max()
                        }
                    };
                case Cgd.ConditionType.ParameterBoolValue:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.BoolCondition
                        {
                            key = condition.parameterBoolValue.parameterName,
                            value = condition.parameterBoolValue.value
                        }
                    };
                case Cgd.ConditionType.ParameterIntValue:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.IntCondition
                        {
                            key = condition.parameterIntValue.parameterName,
                            operation = Remap(condition.parameterIntValue.operation),
                            value = RemapIntValue(condition.parameterIntValue.operation, condition.parameterIntValue.value)
                        }
                    };
                case Cgd.ConditionType.ParameterFloatValue:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.FloatCondition
                        {
                            key = condition.parameterFloatValue.parameterName,
                            operation = Remap(condition.parameterFloatValue.operation),
                            value = condition.parameterFloatValue.value
                        }
                    };
                case Cgd.ConditionType.ParameterIntBetween:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.IntCondition
                        {
                            key = condition.parameterIntBetween.parameterName,
                            operation = CgdSys.IntOperation.IsGreaterThan,
                            value = condition.parameterIntBetween.lowerBoundInclusive - 1
                        },
                        new CgdSys.IntCondition
                        {
                            key = condition.parameterIntBetween.parameterName,
                            operation = CgdSys.IntOperation.IsLessThan,
                            value = condition.parameterIntBetween.upperBoundInclusive + 1
                        }
                    };
                case Cgd.ConditionType.ParameterFloatBetween:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.FloatCondition
                        {
                            key = condition.parameterFloatBetween.parameterName,
                            operation = CgdSys.FloatOperation.IsGreaterThan,
                            value = condition.parameterFloatBetween.lowerBoundExclusive
                        },
                        new CgdSys.FloatCondition
                        {
                            key = condition.parameterFloatBetween.parameterName,
                            operation = CgdSys.FloatOperation.IsLessThan,
                            value = condition.parameterFloatBetween.upperBoundExclusive
                        }
                    };
                case Cgd.ConditionType.ConditionFromComponent:
                    var conditionComponent = condition.conditionFromComponent.conditionComponent;
                    if (mutableVisitedConditions.Contains(conditionComponent))
                    {
                        // Prevent circular references
                        return new CgdSys.ICondition[0];
                    }
                    else
                    {
                        mutableVisitedConditions.Add(conditionComponent);
                        return conditionComponent.conditions
                            .SelectMany(insideCondition => BuildIntermediaryCondition(insideCondition, mutableVisitedConditions))
                            .ToArray();
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CgdSys.IntOperation Remap(Cgd.ParameterIntValue.IntOperation intOperation)
        {
            switch (intOperation)
            {
                case Cgd.ParameterIntValue.IntOperation.Equal:
                    return CgdSys.IntOperation.IsEqualTo;
                case Cgd.ParameterIntValue.IntOperation.NotEqual:
                    return CgdSys.IntOperation.IsNotEqualTo;
                case Cgd.ParameterIntValue.IntOperation.StrictlyGreaterThan:
                case Cgd.ParameterIntValue.IntOperation.GreaterOrEqualTo:
                    return CgdSys.IntOperation.IsGreaterThan;
                case Cgd.ParameterIntValue.IntOperation.StrictlyLessThan:
                case Cgd.ParameterIntValue.IntOperation.LessOrEqualTo:
                    return CgdSys.IntOperation.IsLessThan;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intOperation), intOperation, null);
            }
        }

        private CgdSys.FloatOperation Remap(Cgd.ParameterFloatValue.FloatOperation floatOperation)
        {
            switch (floatOperation)
            {
                case Cgd.ParameterFloatValue.FloatOperation.StrictlyGreaterThan:
                    return CgdSys.FloatOperation.IsGreaterThan;
                case Cgd.ParameterFloatValue.FloatOperation.StrictlyLessThan:
                    return CgdSys.FloatOperation.IsLessThan;
                default:
                    throw new ArgumentOutOfRangeException(nameof(floatOperation), floatOperation, null);
            }
        }

        private int RemapIntValue(Cgd.ParameterIntValue.IntOperation operation, int value)
        {
            switch (operation)
            {
                case Cgd.ParameterIntValue.IntOperation.Equal:
                case Cgd.ParameterIntValue.IntOperation.NotEqual:
                case Cgd.ParameterIntValue.IntOperation.StrictlyGreaterThan:
                case Cgd.ParameterIntValue.IntOperation.StrictlyLessThan:
                    return value;
                case Cgd.ParameterIntValue.IntOperation.GreaterOrEqualTo:
                    return value - 1;
                case Cgd.ParameterIntValue.IntOperation.LessOrEqualTo:
                    return value + 1;
                default:
                    throw new ArgumentOutOfRangeException(nameof(operation), operation, null);
            }
        }

        private CgdSys.ICondition[] HandGestureCondition(Cgd.HandGesture handGesture)
        {
            // Hand gesture conditions need to behave differently based on:
            // - Any: This is an OR condition, which requires an internal Rule tree duplication three times (Left, Right, Both), and this will affect the EffectBehavior fist parameter name.
            // - Left/Right: This will affect the EffectBehavior Any Fist parameter name
            // - Both: Depending on how things work, this will affect Any Fist parameter name,
            //   but also, if supported, Vive Advanced Controls don't need to just work only on Fist pose but on any pose.
            switch (handGesture.side)
            {
                case Cgd.HandGesture.HandSide.Any:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.AnyHandSideWithPoseCondition
                        {
                            pose = (int)handGesture.pose
                        },
                    };
                case Cgd.HandGesture.HandSide.Left:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.LeftSideWithPoseCondition
                        {
                            pose = (int)handGesture.pose
                        },
                    };
                case Cgd.HandGesture.HandSide.Right:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.RightSideWithPoseCondition
                        {
                            pose = (int)handGesture.pose
                        },
                    };
                case Cgd.HandGesture.HandSide.Both:
                    return new CgdSys.ICondition[]
                    {
                        new CgdSys.BothSidesWithPoseCondition
                        {
                            pose = (int)handGesture.pose
                        },
                    };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CgdSys.Tweening Remap(Cgd.Tweening ruleTweening)
        {
            return new CgdSys.Tweening
            {
                importance = Remap(ruleTweening.importance),
                shape = Remap(ruleTweening.shape),
                durationSeconds = ruleTweening.durationSeconds
            };
        }

        private CgdSys.Importance Remap(Cgd.Importance importance)
        {
            switch (importance)
            {
                case Cgd.Importance.None: return CgdSys.Importance.None;
                default:
                    throw new ArgumentOutOfRangeException(nameof(importance), importance, null);
            }
        }

        private CgdSys.Shape Remap(Cgd.Shape shape)
        {
            switch (shape)
            {
                case Cgd.Shape.Linear: return CgdSys.Shape.Linear;
                case Cgd.Shape.EaseInEaseOut: return CgdSys.Shape.EaseInEaseOut;
                case Cgd.Shape.AttackInEaseOut: return CgdSys.Shape.AttackInEaseOut;
                default:
                    throw new ArgumentOutOfRangeException(nameof(shape), shape, null);
            }
        }
    }
}