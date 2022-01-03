using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdSysCompileFallbackAndSplit
    {
        private readonly Components.ComboGestureDynamics _cgd;
        private readonly CgdSys.Activation[] _activations;

        public CgdSysCompileFallbackAndSplit(Components.ComboGestureDynamics cgd, CgdSys.Activation[] activations)
        {
            _cgd = cgd;
            _activations = activations;
            throw new NotImplementedException();
        }

        public void MutateAnimations()
        {
            var allAnimations = FindAllAnimationClips(_activations);
            var allBindings = FindAllCurveBindingsOfActivations(_activations);
            var bindingToFallbackValue = CaptureFallbackValuesFromAvatar(allBindings);
            MutateInsertFallbackValues(allAnimations, allBindings, bindingToFallbackValue);
        }

        private Dictionary<EditorCurveBinding, float> CaptureFallbackValuesFromAvatar(EditorCurveBinding[] allBindings)
        {
            return allBindings.ToDictionary(binding => binding, binding =>
            {
                AnimationUtility.GetFloatValue(_cgd.avatar.gameObject, binding, out var data);
                return data;
            });
        }

        private EditorCurveBinding[] FindAllCurveBindingsOfActivations(CgdSys.Activation[] activations)
        {
            return activations
                .Select(activation => activation.compiledEffect.compiledMotion.universal)
                .SelectMany(FindAllCurveBindingsOf)
                .ToArray();
        }

        private EditorCurveBinding[] FindAllCurveBindingsOf(Motion motion)
        {
            switch (motion)
            {
                case AnimationClip clip:
                    return AnimationUtility.GetCurveBindings(clip);
                case BlendTree tree:
                    return tree.children
                        .Select(childMotion => childMotion.motion)
                        .SelectMany(FindAllCurveBindingsOf)
                        .ToArray();
                default:
                    throw new InvalidOperationException();
            }
        }

        private AnimationClip[] FindAllAnimationClips(CgdSys.Activation[] activations)
        {
            return activations
                .Select(activation => activation.compiledEffect.compiledMotion.universal)
                .SelectMany(AllAnimationsOf)
                .ToArray();
        }

        private static void MutateInsertFallbackValues(AnimationClip[] allAnimations, EditorCurveBinding[] allBindings, Dictionary<EditorCurveBinding, float> bindingToCapturedValue)
        {
            foreach (var clip in allAnimations)
            {
                foreach (var binding in allBindings)
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve == null)
                    {
                        AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0, 1 / 60f, bindingToCapturedValue[binding]));
                    }
                }
            }
        }

        private IEnumerable<AnimationClip> AllAnimationsOf(Motion motion)
        {
            switch (motion)
            {
                case AnimationClip clip:
                    return new[] {clip};
                case BlendTree tree:
                    return tree.children
                        .Select(childMotion => childMotion.motion)
                        .SelectMany(AllAnimationsOf)
                        .ToArray();
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}