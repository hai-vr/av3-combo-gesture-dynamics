using System;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class Cgd
    {
        [Serializable]
        public struct PropertyMask
        {
            public string path;
            public Type type;
            public string propertyName;
        }

        [Serializable]
        public enum Tag
        {
            IsLeftEye, IsRightEye
        }

        [Serializable]
        public struct InheritedEffect
        {
            public CgdEffect effect;
            public PropertyMask[] rejectedProperties;
        }

        [Serializable]
        public enum ConditionType
        {
            HandGesture,
            DefaultMoodSelector,
            SpecificMoodSelector,
            ParameterBoolValue,
            ParameterIntValue,
            ParameterFloatValue,
            ParameterIntBetween,
            ParameterFloatBetween,
            ConditionFromComponent
        }

        [Serializable]
        public struct Condition
        {
            public ConditionType conditionType;
            public HandGesture handGesture;
            public DefaultMoodSelector defaultMoodSelector;
            public SpecificMoodSelector specificMoodSelector;
            public ParameterBoolValue parameterBoolValue;
            public ParameterIntValue parameterIntValue;
            public ParameterFloatValue parameterFloatValue;
            public ParameterIntBetween parameterIntBetween;
            public ParameterFloatBetween parameterFloatBetween;
            public ConditionFromComponent conditionFromComponent;
        }

        [Serializable]
        public struct DefaultMoodSelector
        {
            public string selection;
        }

        [Serializable]
        public struct SpecificMoodSelector
        {
            public CgdMoodSelector moodSelector;
            public string selection;
        }

        [Serializable]
        public struct ParameterBoolValue
        {
            public string parameterName;
            public bool value;
        }

        [Serializable]
        public struct ParameterIntValue
        {
            public string parameterName;
            public IntOperation operation;
            public int value;

            [Serializable]
            public enum IntOperation
            {
                Equal, NotEqual, StrictlyGreaterThan, StrictlyLessThan, GreaterOrEqualTo, LessOrEqualTo
            }
        }

        [Serializable]
        public struct ParameterFloatValue
        {
            public string parameterName;
            public FloatOperation operation;
            public float value;

            [Serializable]
            public enum FloatOperation
            {
                StrictlyGreaterThan, StrictlyLessThan
            }
        }

        [Serializable]
        public struct ParameterIntBetween
        {
            public string parameterName;
            public int lowerBoundInclusive;
            public int upperBoundInclusive;
        }

        [Serializable]
        public struct ParameterFloatBetween
        {
            public string parameterName;
            public float lowerBoundExclusive;
            public float upperBoundExclusive;
        }

        [Serializable]
        public struct ConditionFromComponent
        {
            public CgdCondition conditionComponent;
        }

        [Serializable]
        public struct HandGesture
        {
            public HandSide side;
            public HandPose pose;

            [Serializable]
            public enum HandSide
            {
                Left, Right, Both
            }

            [Serializable]
            public enum HandPose
            {
                Neutral,
                Fist,
                Open,
                Point,
                Victory,
                RockNRoll,
                Gun,
                ThumbsUp
            }
        }

        [Serializable]
        public struct Blend
        {
            public BlendType blendType;
            public Position[] positions;
        }

        [Serializable]
        public struct Position
        {
            public Vector2[] position;
            public CgdEffect effect;
        }

        [Serializable]
        public enum EffectType
        {
            Regular, Blend, Custom
        }

        [Serializable]
        public enum BlendType
        {
            Directional, Cartesian, Simple1D
        }

        [Serializable]
        public class Regular
        {
            public InheritedEffect[] inheritedEffects = new InheritedEffect[0];
            public InsertedClip[] insertedClips = new InsertedClip[0];
            public PropertyValue[] properties = new PropertyValue[0];
            public Tag[] tags = new Tag[0];
        }

        [Serializable]
        public class InsertedClip
        {
            public AnimationClip clip;
            public PropertyMask[] rejectedProperties;
        }

        [Serializable]
        public struct PropertyValue
        {
            public PropertyMask property;
            public float value;
        }

        [Serializable]
        public struct RootTweening
        {
            public float durationSeconds;
            public Shape shape;
            public Importance importance;
        }

        [Serializable]
        public struct Tweening
        {
            public float durationSeconds;
            public Shape shape;
            public Importance importance;
        }

        [Serializable]
        public enum TweeningType
        {
            Inherited, Custom
        }

        [Serializable]
        public enum Shape
        {
            Linear,
            EaseInEaseOut,
            AttackInEaseOut
        }

        [Serializable]
        public enum Importance
        {
            None
        }

        [Serializable]
        public struct EffectBehaviour
        {
            public EffectBehaviourType effectBehaviourType;
            public string analogParameterName;
            public float analogMin; // = 0f;
            public float analogMax; // = 1f;
            public CgdEffect effect;
            public CgdEffect restOptional;
        }

        [Serializable]
        public enum EffectBehaviourType
        {
            Normal, Analog
        }

        public class Permutation
        {
            public EffectBehaviour[] animation;
            public TweeningType tweeningType;
            public Tweening tweening;
        }
    }
}