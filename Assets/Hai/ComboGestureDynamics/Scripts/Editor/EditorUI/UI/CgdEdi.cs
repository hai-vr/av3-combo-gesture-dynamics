using System;
using Hai.ComboGestureDynamics.Scripts.Components;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    public class CgdEdi
    {
        public struct Permutation
        {
            public Cgd.HandGesture.HandPose left;
            public Cgd.HandGesture.HandPose right;

            public Permutation(Cgd.HandGesture.HandPose leftPose, Cgd.HandGesture.HandPose rightPose)
            {
                left = leftPose;
                right = rightPose;
            }

            public string ToLocalizedName()
            {
                var leftHandName = Localize(left);

                if (left == right)
                {
                    return $"{leftHandName} x2";
                }

                var rightHandName = Localize(right);
                return $"{leftHandName} ▶ {rightHandName}";
            }

            private string Localize(Cgd.HandGesture.HandPose handPose)
            {
                var phrase = Remap(handPose);

                return Enum.GetName(typeof(Cgd.HandGesture.HandPose), handPose);
            }

            private static CgdLocalization.Phrase Remap(Cgd.HandGesture.HandPose handPose)
            {
                switch (handPose)
                {
                    case Cgd.HandGesture.HandPose.Neutral: return CgdLocalization.Phrase.HandPoseNeutral;
                    case Cgd.HandGesture.HandPose.Fist: return CgdLocalization.Phrase.HandPoseFist;
                    case Cgd.HandGesture.HandPose.Open: return CgdLocalization.Phrase.HandPoseOpen;
                    case Cgd.HandGesture.HandPose.Point: return CgdLocalization.Phrase.HandPosePoint;
                    case Cgd.HandGesture.HandPose.Victory: return CgdLocalization.Phrase.HandPoseVictory;
                    case Cgd.HandGesture.HandPose.RockNRoll: return CgdLocalization.Phrase.HandPoseRockNRoll;
                    case Cgd.HandGesture.HandPose.Gun: return CgdLocalization.Phrase.HandPoseGun;
                    case Cgd.HandGesture.HandPose.ThumbsUp: return CgdLocalization.Phrase.HandPoseThumbsUp;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(handPose), handPose, null);
                }
            }

            public bool IsSymmetrical()
            {
                return left == right;
            }

            public Permutation Mirror()
            {
                return new Permutation(right, left);
            }

            public bool IsLeft()
            {
                return (int) left > (int) right;
            }

            public int ToPermutationEffectBehavioursArrayIndex()
            {
                return ((int) left) * 8 + (int) right;
            }
        }
    }
}