using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdSys
    {
        public class System
        {
            public Activation[] activations;
            public AvatarMask fxMask;
            public AvatarMask gestureMask;
        }

        internal interface ICondition
        {
        }

        internal class BoolCondition : ICondition
        {
            public string key;
            public bool value;
        }

        internal class FloatCondition : ICondition
        {
            public string key;
            public FloatOperation operation;
            public float value;
        }

        internal class IntCondition : ICondition
        {
            public string key;
            public IntOperation operation;
            public int value;
        }

        internal class AnyHandSideWithPoseCondition : ICondition
        {
            public int pose;
        }

        internal class LeftSideWithPoseCondition : ICondition
        {
            public int pose;
        }

        internal class RightSideWithPoseCondition : ICondition
        {
            public int pose;
        }

        internal class BothSidesWithPoseCondition : ICondition
        {
            public int pose;
        }

        public struct Tweening
        {
            public float durationSeconds;
            public Shape shape;
            public Importance importance;
        }

        public enum Shape
        {
            Linear,
            EaseInEaseOut,
            AttackInEaseOut
        }

        public enum Importance
        {
            None
        }

        internal enum FloatOperation
        {
            IsGreaterThan,
            IsLessThan
        }

        internal enum IntOperation
        {
            IsEqualTo,
            IsNotEqualTo,
            IsGreaterThan,
            IsLessThan,
        }

        internal class CompiledEffect
        {
            public CompiledMotion compiledMotion;
        }

        internal class CompiledMotion
        {
            public Motion universal;
            public Motion fx;
            public Motion gesture;
        }

        internal class Activation
        {
            public ICondition[] conditions;
            public Tweening tweening;
            public CompiledEffect compiledEffect;
        }
    }
}