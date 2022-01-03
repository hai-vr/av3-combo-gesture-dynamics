using System;
using Hai.Cge2Aac.Framework.Editor.Internal.V0.Fluentb;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using UnityEditor.Animations;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdActivationsLayer
    {
        private readonly AacV0.AacFlBase<CgdController> _aac;
        private readonly CgdParameters _parameters;
        private readonly CgdSys.Activation[] _activations;

        internal CgdActivationsLayer(AacV0.AacFlBase<CgdController> aac, CgdParameters parameters, CgdSys.Activation[] activations)
        {
            _aac = aac;
            _parameters = parameters;
            _activations = activations;
        }

        public void GenerateFX()
        {
            var layer = _aac.CreateSupportingFxLayer("Activations");
            var activationsLength = _activations.Length;
            for (var index = 0; index < activationsLength; index++)
            {
                var activation = _activations[index];
                var state = layer.NewState($"Activation {index}", 0, activationsLength - index);
                layer.AnyTransitionsTo(state).When(new AacFlConditionSimple(condition =>
                {
                    foreach (var activationCondition in activation.conditions)
                    {
                        switch (activationCondition)
                        {
                            case CgdSys.BoolCondition specific:
                                condition.Add(specific.key, specific.value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f);
                                break;
                            case CgdSys.FloatCondition specific:
                                condition.Add(specific.key, Remap(specific.operation), specific.value);
                                break;
                            case CgdSys.IntCondition specific:
                                condition.Add(specific.key, Remap(specific.operation), specific.value);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }));
            }
        }

        private static AnimatorConditionMode Remap(CgdSys.FloatOperation floatOperation)
        {
            switch (floatOperation)
            {
                case CgdSys.FloatOperation.IsGreaterThan:
                    return AnimatorConditionMode.Greater;
                case CgdSys.FloatOperation.IsLessThan:
                    return AnimatorConditionMode.Less;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static AnimatorConditionMode Remap(CgdSys.IntOperation intOperation)
        {
            switch (intOperation)
            {
                case CgdSys.IntOperation.IsEqualTo:
                    return AnimatorConditionMode.Equals;
                case CgdSys.IntOperation.IsNotEqualTo:
                    return AnimatorConditionMode.NotEqual;
                case CgdSys.IntOperation.IsGreaterThan:
                    return AnimatorConditionMode.Greater;
                case CgdSys.IntOperation.IsLessThan:
                    return AnimatorConditionMode.Less;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intOperation), intOperation, null);
            }
        }
    }
}