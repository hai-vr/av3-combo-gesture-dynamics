using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdSys
    {
        public class System
        {
            public Activation[] activations;
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
        }

        internal class Activation
        {
            public ICondition[] conditions;
            public Tweening tweening;
            public CompiledEffect compiledEffect;
        }
    }
}