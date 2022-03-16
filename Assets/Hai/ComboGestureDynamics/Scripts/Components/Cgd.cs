using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

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
            public string parameterName;
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
            public bool value;
        }

        [Serializable]
        public struct ParameterIntValue
        {
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
            public int lowerBoundInclusive;
            public int upperBoundInclusive;
        }

        [Serializable]
        public struct ParameterFloatBetween
        {
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

            public static (HandGesture, HandGesture) FromPermutationEffectBehavioursArrayIndex(int arrayIndex)
            {
                return (new HandGesture
                {
                    side = HandSide.Left,
                    pose = (HandPose) (arrayIndex / 8)
                }, new HandGesture
                {
                    side = HandSide.Right,
                    pose = (HandPose) (arrayIndex % 8)
                });
            }
        }

        // [Serializable]
        // public struct Blend
        // {
        //     public BlendType blendType;
        //     public Position[] positions;
        //
        //     public void MutateAnyReferenceNormalize()
        //     {
        //         if (positions == null) positions = new Position[0];
        //     }
        // }

        [Serializable]
        public struct Position
        {
            public Vector2[] position;
            public Expression expression;
        }

        [Serializable]
        public struct Expression
        {
            public AnimationClip clip;
            // public CgdEffect effect;

            public bool IsDefined()
            {
                return clip != null; // || effect != null;
            }

            public void MutateAnyReferenceNormalize()
            {
                // effect.MutateAnyReferenceNormalize();
            }
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

        // [Serializable]
        // public struct Regular
        // {
        //     public InheritedEffect[] inheritedEffects; // = new InheritedEffect[0];
        //     public InsertedClip[] insertedClips; // = new InsertedClip[0];
        //     public PropertyValue[] properties; // = new PropertyValue[0];
        //     public Tag[] tags; // = new Tag[0];
        //
        //     public void MutateAnyReferenceNormalize()
        //     {
        //         if (inheritedEffects == null) inheritedEffects = new InheritedEffect[0];
        //         if (insertedClips == null) insertedClips = new InsertedClip[0];
        //         if (properties == null) properties = new PropertyValue[0];
        //         if (tags == null) tags = new Tag[0];
        //     }
        // }

        [Serializable]
        public struct InsertedClip
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
        public struct Tweening
        {
            [FormerlySerializedAs("durationSeconds")] public float entranceDurationSeconds;
            public Shape shape;
            public Importance importance;
            [FormerlySerializedAs("customExitDuration")] public bool hasCustomExitDuration;
            public float exitDurationSeconds;
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
            None, Important
        }

        // [Serializable]
        // public struct EffectReference
        // {
        //     public EffectReferenceType effectReferenceType;
        //     public Motion motion;
        //     public CgdEffect effect;
        // }
        //
        // public enum EffectReferenceType
        // {
        //     Motion,
        //     Effect
        // }

        [Serializable]
        public struct EffectBehaviour
        {
            public EffectBehaviourType effectBehaviourType;
            public string analogParameterName;
            public float analogMin; // = 0f;
            public float analogMax; // = 1f;
            public Expression expression;
            public Expression restOptional;

            public Expression[] DefensiveActiveExpressions()
            {
                switch (effectBehaviourType)
                {
                    case EffectBehaviourType.Normal: return NonNull(new[] {expression});
                    case EffectBehaviourType.Analog: return NonNull(new[] {expression, restOptional});
                    case EffectBehaviourType.None: return new Expression[0];
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private Expression[] NonNull(Expression[] things)
            {
                return things.Where(expression => expression.IsDefined()).ToArray();
            }

            public void MutateAnyReferenceNormalize()
            {
                if (expression.IsDefined()) expression.MutateAnyReferenceNormalize();
                if (restOptional.IsDefined()) restOptional.MutateAnyReferenceNormalize();
            }
        }

        [Serializable]
        public enum EffectBehaviourType
        {
            Normal, Analog, None
        }

        [Serializable]
        public struct PermutationEffectBehaviour
        {
            public Expression expression;
            public Expression expressionFistLeft;
            public Expression expressionFistRight;
            public TweeningType tweeningType;
            public Tweening tweening;
        }

        public enum PermutationStencil
        {
            None, DefinedAndCombos, Defined
        }

        public enum UiComplexity
        {
            Simple, Advanced
        }
    }
}