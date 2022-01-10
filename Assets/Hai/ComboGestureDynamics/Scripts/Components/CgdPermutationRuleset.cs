using System.Linq;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class CgdPermutationRuleset : MonoBehaviour
    {
        public Cgd.Condition[] conditions = new Cgd.Condition[0];
        public Cgd.PermutationEffectBehaviour[] permutationEffectBehaviours;
        public Cgd.PermutationStencil permutationStencil;
        public Cgd.TweeningType tweeningType;
        public Cgd.Tweening tweening;

        public CgdPart[] parts = new CgdPart[0];

        public void MutateAnyReferenceNormalize()
        {
            if (conditions == null) conditions = new Cgd.Condition[0];
            if (permutationEffectBehaviours == null) permutationEffectBehaviours = new Cgd.PermutationEffectBehaviour[8 * 8];
            if (permutationEffectBehaviours.Length < 8 * 8)
            {
                permutationEffectBehaviours = permutationEffectBehaviours
                    .Concat(new Cgd.PermutationEffectBehaviour[8 * 8 - permutationEffectBehaviours.Length])
                    .ToArray();
            }
        }

        public Cgd.PermutationEffectBehaviour Behaviour(Cgd.HandGesture.HandPose left, Cgd.HandGesture.HandPose right)
        {
            return permutationEffectBehaviours[(int) left * 8 + (int) right];
        }
    }
}