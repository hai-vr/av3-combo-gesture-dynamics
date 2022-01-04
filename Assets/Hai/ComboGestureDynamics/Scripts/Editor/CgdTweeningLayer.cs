using System;
using Hai.ComboGestureDynamics.Scripts.Components;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0.Fluentb;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdTweeningLayer
    {
        private const int ToHigh = 1;
        private const int ToLow = 2;
        private readonly AacV0.AacFlBase<CgdController> _aac;
        private readonly CgdParameters _parameters;
        private readonly int _activationsLength;

        internal CgdTweeningLayer(AacV0.AacFlBase<CgdController> aac, CgdParameters parameters, int activationsLength)
        {
            _aac = aac;
            _parameters = parameters;
            _activationsLength = activationsLength;
        }

        public void GenerateFX()
        {
            var layer = _aac.CreateSupportingFxLayer("Tweening");
            var tweeningParam = layer.FloatParameter(_parameters.Tweening);
            // var commandToHigh = layer.BoolParameter(_parameters.CommandToHigh);
            // var commandToLow = layer.BoolParameter(_parameters.CommandToLow);
            // var shapeToHigh = layer.IntParameter(_parameters.ShapeToHigh);
            // var shapeToLow = layer.IntParameter(_parameters.ShapeToLow);
            var tweenShape = layer.IntParameter(_parameters.TweenShape);
            var duration = layer.FloatParameter(_parameters.TweenDuration);
            // var durationToHigh = layer.FloatParameter(_parameters.DurationToHigh);
            // var durationToLow = layer.FloatParameter(_parameters.DurationToLow);
            var tweenIsReady = layer.BoolParameter(_parameters.TweenIsReady);
            var tweenIsHigh = layer.BoolParameter(_parameters.TweenIsHigh);

            var idleLow = layer.NewState("Idle Low", 0, 0)
                // .WithAnimation(CreateDiffTree(layer, false))
                .WithAnimation(_aac.NewClip().That(clip => clip.AnimatingAnimator(layer.FloatParameter(_parameters.Tweening)).WithOneFrame(0f)))
                .Drives(tweenIsReady, true)
                .Drives(tweenIsHigh, false);
            var idleHigh = layer.NewState("Idle High", 3, 0)
                // .WithAnimation(CreateDiffTree(layer, true))
                .WithAnimation(_aac.NewClip().That(clip => clip.AnimatingAnimator(layer.FloatParameter(_parameters.Tweening)).WithOneFrame(1f)))
                .Drives(tweenIsReady, true)
                .Drives(tweenIsHigh, true);

            void CreateTweeningShape(CgdSys.Shape shape, Action<AacFlSettingKeyframes> toHighBuilder, Action<AacFlSettingKeyframes> toLowBuilder)
            {
                var shapeInt = (int) shape;
                var shapeName = Enum.GetName(typeof(CgdSys.Shape), shape);
                var linearToHigh = layer.NewState($"{shapeName} to High", ToHigh, shapeInt * 2)
                    .Drives(tweenIsReady, false)
                    .WithSpeed(duration)
                    .WithAnimation(_aac.NewClip().That(clip => clip.AnimatingAnimator(tweeningParam)
                        .WithSecondsUnit(toHighBuilder)));

                var linearToLow = layer.NewState($"{shapeName} to Low", ToLow, shapeInt * 2 + 1)
                    .Drives(tweenIsReady, false)
                    .WithSpeed(duration)
                    .WithAnimation(_aac.NewClip().That(clip => clip.AnimatingAnimator(tweeningParam)
                        .WithSecondsUnit(toLowBuilder)));

                idleLow.TransitionsTo(linearToHigh)
                    .When(layer.BoolParameter(_parameters.ActivationIsHigh).IsTrue()).And(tweenShape.IsEqualTo(shapeInt));
                idleHigh.TransitionsTo(linearToLow)
                    .When(layer.BoolParameter(_parameters.ActivationIsHigh).IsFalse()).And(tweenShape.IsEqualTo(shapeInt));

                foreach (var lowToHigh in new [] {linearToHigh})
                {
                    lowToHigh.TransitionsTo(idleHigh).AfterAnimationFinishes();
                }
                foreach (var highToLow in new [] {linearToLow})
                {
                    highToLow.TransitionsTo(idleLow).AfterAnimationFinishes();
                }
            }

            CreateTweeningShape(CgdSys.Shape.Linear, keyframes => keyframes.Linear(0f, 0f).Linear(1f, 1f), keyframes => keyframes.Linear(0f, 1f).Linear(1f, 0f));
            CreateTweeningShape(CgdSys.Shape.EaseInEaseOut, keyframes => keyframes.Easing(0f, 0f).Easing(1f, 1f), keyframes => keyframes.Easing(0f, 1f).Easing(1f, 0f));
            CreateTweeningShape(CgdSys.Shape.AttackInEaseOut, keyframes => keyframes.Tangent(0f, 0f, 1f).Easing(1f, 1f), keyframes => keyframes.Tangent(0f, 1f, -1f).Easing(1f, 0f));
        }

        private BlendTree CreateDiffTree(AacV0.AacFlLayer layer, bool isHigh)
        {
            var diffTreeLow = _aac.NewBlendTreeAsRaw();
            diffTreeLow.blendType = BlendTreeType.FreeformCartesian2D;
            diffTreeLow.blendParameter = _parameters.ExpressionLow;
            diffTreeLow.blendParameterY = _parameters.ExpressionHigh;
            diffTreeLow.children = new[]
            {
                new ChildMotion {motion = NewTweenDiffClip(layer, isHigh, 0f), position = new Vector2(0, 0)},
                new ChildMotion {motion = NewTweenDiffClip(layer, isHigh, 0f), position = new Vector2(_activationsLength, _activationsLength)},
                new ChildMotion {motion = NewTweenDiffClip(layer, isHigh, _activationsLength), position = new Vector2(_activationsLength, 0)},
                new ChildMotion {motion = NewTweenDiffClip(layer, isHigh, _activationsLength), position = new Vector2(0, _activationsLength)},
            };
            return diffTreeLow;
        }

        private AnimationClip NewTweenDiffClip(AacV0.AacFlLayer layer, bool isHigh, float diffValue)
        {
            return _aac.NewClip().That(clip =>
            {
                clip.AnimatingAnimator(layer.FloatParameter(_parameters.Tweening)).WithOneFrame(isHigh ? 1f : 0f);
                // clip.AnimatingAnimator(layer.FloatParameter(_parameters.ExpressionDiff)).WithOneFrame(diffValue);
            }).Clip;
        }
    }
}