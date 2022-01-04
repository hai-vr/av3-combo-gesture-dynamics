using System;
using System.Collections.Generic;
using Hai.Cge2Aac.Framework.Editor.Internal.V0.Fluentb;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdActivationsLayer
    {
        private readonly AacV0.AacFlBase<CgdController> _aac;
        private readonly CgdParameters _parameters;
        private readonly int _orderedCompiledEffectsLength;
        private readonly CgdSys.Activation[] _activations;

        internal CgdActivationsLayer(AacV0.AacFlBase<CgdController> aac, CgdParameters parameters, int orderedCompiledEffectsLength, CgdSys.Activation[] activations)
        {
            _aac = aac;
            _parameters = parameters;
            _orderedCompiledEffectsLength = orderedCompiledEffectsLength;
            _activations = activations;
        }

        public void GenerateFX()
        {
            var layer = _aac.CreateSupportingFxLayer("Activations");
            var waitingState = layer.NewState("Waiting", 0, 0);
            var activationsLength = _activations.Length;
            var lowStates = new List<AacFlState>();
            var highStates = new List<AacFlState>();
            for (var index = 0; index < activationsLength; index++)
            {
                var angleRad = index / (activationsLength * 1f) * Mathf.PI;
                var radius = Mathf.Log10(activationsLength) * 10 * 50;
                var xx = Mathf.Cos(angleRad * 2) * radius;
                var yy = Mathf.Sin(angleRad * 2) * radius;
                var xxH = Mathf.Cos(angleRad * 2) * (radius + 200);
                var yyH = Mathf.Sin(angleRad * 2) * (radius + 200);
                var lowState = layer.NewStateAt($"Lo {index}", xx, yy);
                var highState = layer.NewStateAt($"Hi {index}", xxH, yyH);
                lowStates.Add(lowState);
                highStates.Add(highState);
                lowState.Drives(layer.FloatParameter(_parameters.ExpressionLow), index / (_orderedCompiledEffectsLength - 1f));
                highState.Drives(layer.FloatParameter(_parameters.ExpressionHigh), index / (_orderedCompiledEffectsLength - 1f));
                lowState.Drives(layer.IntParameter(_parameters.TweenShape), (int) _activations[index].tweening.shape);
                highState.Drives(layer.IntParameter(_parameters.TweenShape), (int) _activations[index].tweening.shape);
                lowState.Drives(layer.BoolParameter(_parameters.ActivationIsHigh), false);
                highState.Drives(layer.BoolParameter(_parameters.ActivationIsHigh), true);
                var durationSeconds = _activations[index].tweening.durationSeconds;
                var isDurationInstant = durationSeconds <= 0.0001f;
                lowState.Drives(layer.FloatParameter(_parameters.TweenDuration), isDurationInstant ? float.MaxValue : 1f / durationSeconds);
                highState.Drives(layer.FloatParameter(_parameters.TweenDuration), isDurationInstant ? float.MaxValue : 1f / durationSeconds);
            }

            for (var lowIndex = 0; lowIndex < lowStates.Count; lowIndex++)
            {
                var low = lowStates[lowIndex];
                for (var highIndex = 0; highIndex < highStates.Count; highIndex++)
                {
                    var high = highStates[highIndex];
                    if (lowIndex != highIndex)
                    {
                        low.TransitionsTo(high)
                            .When(layer.BoolParameter(_parameters.TweenIsReady).IsTrue())
                            .And(layer.BoolParameter(_parameters.TweenIsHigh).IsFalse())
                            .And(ConditionsPass(layer, _activations[highIndex].conditions));
                        high.TransitionsTo(low)
                            .When(layer.BoolParameter(_parameters.TweenIsReady).IsTrue())
                            .And(layer.BoolParameter(_parameters.TweenIsHigh).IsTrue())
                            .And(ConditionsPass(layer, _activations[lowIndex].conditions));
                    }
                    else
                    {
                        low.TransitionsTo(low)
                            .When(layer.BoolParameter(_parameters.TweenIsReady).IsTrue())
                            .And(layer.BoolParameter(_parameters.TweenIsHigh).IsFalse())
                            .And(ConditionsPass(layer, _activations[lowIndex].conditions));
                        high.TransitionsTo(high)
                            .When(layer.BoolParameter(_parameters.TweenIsReady).IsTrue())
                            .And(layer.BoolParameter(_parameters.TweenIsHigh).IsTrue())
                            .And(ConditionsPass(layer, _activations[highIndex].conditions));
                    }
                }
            }

            // FIXME: When there are only two activations, it generate a graph with unreachable parts

            waitingState.AutomaticallyMovesTo(lowStates[lowStates.Count - 1]);
        }

        private Action<AacFlTransitionContinuationWithoutOr> ConditionsPass(AacV0.AacFlLayer layer, CgdSys.ICondition[] conditions)
        {
            return continuation =>
            {
                foreach (var condition in conditions)
                {
                    continuation.And(ResolveCondition(layer, condition));
                }
            };
        }

        private static IAacFlCondition ResolveCondition(AacV0.AacFlLayer layer, CgdSys.ICondition activationCondition)
        {
            switch (activationCondition)
            {
                case CgdSys.BoolCondition specific:
                    return layer.BoolParameter(specific.key).IsEqualTo(specific.value);
                case CgdSys.FloatCondition specific:
                    return ResolveCondition(layer, specific);
                case CgdSys.IntCondition specific:
                    return ResolveCondition(layer, specific);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IAacFlCondition ResolveCondition(AacV0.AacFlLayer layer, CgdSys.FloatCondition specific)
        {
            var floatParam = layer.FloatParameter(specific.key);
            switch (specific.operation)
            {
                case CgdSys.FloatOperation.IsGreaterThan:
                    return floatParam.IsGreaterThan(specific.value);
                case CgdSys.FloatOperation.IsLessThan:
                    return floatParam.IsLessThan(specific.value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static IAacFlCondition ResolveCondition(AacV0.AacFlLayer layer, CgdSys.IntCondition specific)
        {
            var intParam = layer.IntParameter(specific.key);
            switch (specific.operation)
            {
                case CgdSys.IntOperation.IsEqualTo:
                    return intParam.IsEqualTo(specific.value);
                case CgdSys.IntOperation.IsNotEqualTo:
                    return intParam.IsNotEqualTo(specific.value);
                case CgdSys.IntOperation.IsGreaterThan:
                    return intParam.IsGreaterThan(specific.value);
                case CgdSys.IntOperation.IsLessThan:
                    return intParam.IsLessThan(specific.value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}