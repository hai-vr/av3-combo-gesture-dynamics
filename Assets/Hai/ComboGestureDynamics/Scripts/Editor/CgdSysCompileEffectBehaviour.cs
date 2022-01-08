using System;
using System.Linq;
using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdSysCompileEffectBehaviour
    {
        private readonly Cgd.EffectBehaviour _effectBehaviour;
        private readonly CgdPart _part;
        private readonly CgdSys.CompiledEffect _previousEffectOptional;
        private readonly Cgd.PropertyMask[] _denyListOnlyFirstPartNullable;
        private readonly CgdParameters _cgdParameters;

        public CgdSysCompileEffectBehaviour(Cgd.EffectBehaviour effectBehaviour, CgdPart part, CgdSys.CompiledEffect previousEffectOptional, Cgd.PropertyMask[] denyListOnlyFirstPartNullable, CgdParameters cgdParameters)
        {
            _effectBehaviour = effectBehaviour;
            _part = part;
            _previousEffectOptional = previousEffectOptional;
            _denyListOnlyFirstPartNullable = denyListOnlyFirstPartNullable;
            _cgdParameters = cgdParameters;
        }

        public CgdSys.CompiledEffect Compile()
        {
            switch (_effectBehaviour.effectBehaviourType)
            {
                case Cgd.EffectBehaviourType.Normal:
                    return new CgdSysCompileSingleEffect(_part, _denyListOnlyFirstPartNullable, _cgdParameters, _effectBehaviour.effect).Compile();
                case Cgd.EffectBehaviourType.Analog:
                    var active = new CgdSysCompileSingleEffect(_part, _denyListOnlyFirstPartNullable, _cgdParameters, _effectBehaviour.effect).Compile();

                    if (_effectBehaviour.restOptional == null && _previousEffectOptional == null)
                    {
                        return active; // FIXME: This is defensive
                    }

                    var rest = _effectBehaviour.restOptional != null ? new CgdSysCompileSingleEffect(_part, _denyListOnlyFirstPartNullable, _cgdParameters, _effectBehaviour.restOptional).Compile() : _previousEffectOptional;

                    return new CgdSys.CompiledEffect
                    {
                        compiledMotion = new CgdSys.CompiledMotion
                        {
                            universal = AnalogBlendTree(
                                active.compiledMotion.universal,
                                rest.compiledMotion.universal,
                                _effectBehaviour.analogMin,
                                _effectBehaviour.analogMax,
                                _effectBehaviour.analogParameterName
                            ),
                        }
                    };
                case Cgd.EffectBehaviourType.None:
                    // This might only be applicable for the Root Rule.
                    // Normally, None does not result in a compiled motion.
                    return new CgdSysCompileSingleEffect(_part, _denyListOnlyFirstPartNullable, _cgdParameters, new CgdEffect()).Compile();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private BlendTree AnalogBlendTree(Motion active, Motion rest, float min, float max, string parameterName)
        {
            return new BlendTree {
                blendType = BlendTreeType.Simple1D,
                useAutomaticThresholds = false,
                minThreshold = min,
                maxThreshold = max,
                blendParameter = parameterName,
                children = new []
                {
                    new ChildMotion
                    {
                        threshold = min,
                        motion = rest,
                    },
                    new ChildMotion
                    {
                        threshold = max,
                        motion = active,
                    }
                }
            };
        }
    }

    internal class CgdSysCompileSingleEffect
    {
        private readonly CgdPart _part;
        private readonly Cgd.PropertyMask[] _denyListOnlyFirstPartNullable;
        private readonly CgdParameters _cgdParameters;
        private readonly CgdEffect _effect;

        public CgdSysCompileSingleEffect(CgdPart part, Cgd.PropertyMask[] denyListOnlyFirstPartNullable, CgdParameters cgdParameters, CgdEffect effect)
        {
            _part = part;
            _denyListOnlyFirstPartNullable = denyListOnlyFirstPartNullable;
            _cgdParameters = cgdParameters;
            _effect = effect;
        }

        public CgdSys.CompiledEffect Compile()
        {
            switch (_effect.effectType)
            {
                case Cgd.EffectType.Regular:
                    return new CgdSys.CompiledEffect
                    {
                        compiledMotion = CompileRegular(_effect.regular, new Cgd.PropertyMask[0], new[] {_effect.regular})
                    };
                case Cgd.EffectType.Blend:
                    // TODO!!!!!!!!!!!!!!!!!!!!
                    if (true) return null;
                    break;
                case Cgd.EffectType.Custom:
                    // TODO!!!!!!!!!!!!!!!!!!!!
                    if (true) return null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private CgdSys.CompiledMotion CompileRegular(Cgd.Regular regular, Cgd.PropertyMask[] rejectedProperties, Cgd.Regular[] visitedRegular)
        {
            var compiledMotion = regular.inheritedEffects
                .Where(effect => effect.effect.effectType == Cgd.EffectType.Regular) // Regular can only inherit Regular effects
                .Where(effect => !visitedRegular.Contains(effect.effect.regular)) // Prevent circular references
                .Select(effect => CompileRegular(effect.effect.regular, effect.rejectedProperties, visitedRegular.Concat(new[] {regular}).ToArray()))
                .DefaultIfEmpty(new CgdSys.CompiledMotion
                {
                    universal = new AnimationClip()
                })
                .Aggregate((left, right) => new CgdSys.CompiledMotion
                {
                    universal = NewClipMerging(left.universal, right.universal, rejectedProperties),
                });

            foreach (var regularProperty in regular.properties)
            {
                var toModify = (AnimationClip) compiledMotion.universal;
                AnimationUtility.SetEditorCurve(toModify, Remap(regularProperty.property), AnimationCurve.Constant(0, 1/60f, regularProperty.value));
            }

            foreach (var insertedClip in regular.insertedClips)
            {
                var bindingsToAdd = AnimationUtility.GetCurveBindings(insertedClip.clip)
                    .Where(binding => insertedClip.rejectedProperties.All(rejected => !CgdSysExtensions.IsSameAsBinding(rejected, binding)))
                    .ToArray();
                foreach (var editorCurveBinding in bindingsToAdd)
                {
                    var toModify = SelectFxOrGestureClipToModify(compiledMotion);
                    AnimationUtility.SetEditorCurve(toModify, editorCurveBinding, AnimationUtility.GetEditorCurve(insertedClip.clip, editorCurveBinding));
                }
            }

            // Lots of inefficiency in this function (elements are added eagerly then deleted immediately) but it should be good enough
            if (_denyListOnlyFirstPartNullable != null)
            {
                StripRejected((AnimationClip)compiledMotion.universal, _denyListOnlyFirstPartNullable);
            }
            else
            {
                KeepOnlyAccepted((AnimationClip)compiledMotion.universal, _part.acceptedProperties);
            }

            // TODO: Sanitize _part so that an acceptedTag can only be exclusively owned by one part
            foreach (var partAcceptedTag in _part.acceptedTags)
            {
                switch (partAcceptedTag)
                {
                    case Cgd.Tag.IsLeftEye:
                        AapIsLeftEye((AnimationClip)compiledMotion.universal, regular.tags.Contains(Cgd.Tag.IsLeftEye));
                        break;
                    case Cgd.Tag.IsRightEye:
                        AapIsLeftEye((AnimationClip)compiledMotion.universal, regular.tags.Contains(Cgd.Tag.IsRightEye));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return compiledMotion;
        }

        private void AapIsLeftEye(AnimationClip fxClip, bool isTrue)
        {
            AnimationUtility.SetEditorCurve(fxClip, new EditorCurveBinding
            {
                path = "",
                type = typeof(Animator),
                propertyName = _cgdParameters.IsLeftEyeClosed
            }, AnimationCurve.Constant(0, 1/60f, isTrue ? 1f : 0f));
        }

        private static AnimationClip SelectFxOrGestureClipToModify(CgdSys.CompiledMotion compiledMotion)
        {
            return (AnimationClip) compiledMotion.universal;
        }

        private Motion NewClipMerging(Motion left, Motion right, Cgd.PropertyMask[] rejectedProperties)
        {
            var copy = (AnimationClip)Object.Instantiate(left);
            MergeClip(copy, right);
            StripRejected(copy, rejectedProperties);

            return copy;
        }

        private void StripRejected(AnimationClip mutableClip, Cgd.PropertyMask[] rejectedProperties)
        {
            foreach (var mask in rejectedProperties)
            {
                AnimationUtility.SetEditorCurve(mutableClip, Remap(mask), null);
            }
        }

        private void KeepOnlyAccepted(AnimationClip mutableClip, Cgd.PropertyMask[] acceptedProperties)
        {
            var editorCurveBindings = AnimationUtility.GetCurveBindings(mutableClip);
            foreach (var editorCurveBinding in editorCurveBindings)
            {
                if (!acceptedProperties.Any(accepted => CgdSysExtensions.IsSameAsBinding(accepted, editorCurveBinding)))
                {
                    AnimationUtility.SetEditorCurve(mutableClip, editorCurveBinding, null);
                }
            }
        }

        private static EditorCurveBinding Remap(Cgd.PropertyMask toReject)
        {
            return new EditorCurveBinding
            {
                path = toReject.path,
                type = toReject.type,
                propertyName = toReject.propertyName
            };
        }

        private static void MergeClip(AnimationClip mutableClip, Motion right)
        {
            var rightClip = (AnimationClip) right;
            foreach (var toCopy in AnimationUtility.GetCurveBindings(rightClip))
            {
                AnimationUtility.SetEditorCurve(mutableClip, toCopy, AnimationUtility.GetEditorCurve(rightClip, toCopy));
            }
        }
    }
}