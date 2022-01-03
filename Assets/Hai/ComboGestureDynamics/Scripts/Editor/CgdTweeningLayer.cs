using System;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0.Fluentb;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdTweeningLayer
    {
        private const int ToHigh = 1;
        private const int ToLow = 2;
        private readonly AacV0.AacFlBase<CgdController> _aac;
        private readonly CgdParameters _parameters;

        internal CgdTweeningLayer(AacV0.AacFlBase<CgdController> aac, CgdParameters parameters)
        {
            _aac = aac;
            _parameters = parameters;
        }

        public void GenerateFX()
        {
            var layer = _aac.CreateSupportingFxLayer("Tweening");
            var tweeningParam = layer.FloatParameter(_parameters.Tweening);
            var commandToHigh = layer.BoolParameter(_parameters.CommandToHigh);
            var commandToLow = layer.BoolParameter(_parameters.CommandToLow);
            var shapeToHigh = layer.IntParameter(_parameters.ShapeToHigh);
            var shapeToLow = layer.IntParameter(_parameters.ShapeToLow);
            var durationToHigh = layer.FloatParameter(_parameters.DurationToHigh);
            var durationToLow = layer.FloatParameter(_parameters.DurationToLow);

            var idleLow = layer.NewState("Idle Low", 0, 0)
                .WithAnimation(_aac.NewClip().That(clip => clip.AnimatingAnimator(tweeningParam).WithOneFrame(0f)));
            var idleHigh = layer.NewState("Idle High", 3, 0)
                .WithAnimation(_aac.NewClip().That(clip => clip.AnimatingAnimator(tweeningParam).WithOneFrame(1f)));

            void CreateTweeningShape(CgdTweeningShape shape, Action<AacFlSettingKeyframes> toHighBuilder, Action<AacFlSettingKeyframes> toLowBuilder)
            {
                var shapeInt = (int) shape;
                var linearToHigh = layer.NewState("Linear to High", ToHigh, shapeInt * 2)
                    .WithSpeed(durationToHigh)
                    .WithAnimation(_aac.NewClip().That(clip => clip.AnimatingAnimator(tweeningParam)
                        .WithSecondsUnit(toHighBuilder)));

                var linearToLow = layer.NewState("Linear to High", ToLow, shapeInt * 2 + 1)
                    .WithSpeed(durationToLow)
                    .WithAnimation(_aac.NewClip().That(clip => clip.AnimatingAnimator(tweeningParam)
                        .WithSecondsUnit(toLowBuilder)));

                idleLow.TransitionsTo(linearToHigh).When(commandToHigh.IsTrue()).And(shapeToHigh.IsEqualTo(shapeInt));
                idleHigh.TransitionsTo(linearToLow).When(commandToLow.IsTrue()).And(shapeToLow.IsEqualTo(shapeInt));
            }

            CreateTweeningShape(CgdTweeningShape.Linear, keyframes => keyframes.Linear(0f, 0f).Linear(1f, 1f), keyframes => keyframes.Linear(0f, 1f).Linear(1f, 0f));
            CreateTweeningShape(CgdTweeningShape.EaseInOut, keyframes => keyframes.Easing(0f, 0f).Easing(1f, 1f), keyframes => keyframes.Easing(0f, 1f).Easing(1f, 0f));
        }
    }

    internal enum CgdTweeningShape
    {
        Linear,
        EaseInOut
    }
}