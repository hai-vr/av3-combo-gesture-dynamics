using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class CgdRootRule : MonoBehaviour
    {
        public Cgd.EffectBehaviour effectBehaviour;
        public Cgd.Tweening tweening;

        public void MutateAnyReferenceNormalize()
        {
            effectBehaviour.MutateAnyReferenceNormalize();
            MutateAnyReferenceRule(transform);
        }

        private void MutateAnyReferenceRule(Transform whichTransform)
        {
            foreach (Transform child in whichTransform)
            {
                var cgdPermutationRuleset = child.GetComponent<CgdPermutationRuleset>();
                if (cgdPermutationRuleset != null)
                {
                    cgdPermutationRuleset.MutateAnyReferenceNormalize();
                }
                else
                {
                    var rule = child.GetComponent<CgdRule>();
                    if (rule != null)
                    {
                        rule.MutateAnyReferenceNormalize();
                        MutateAnyReferenceRule(child);
                    }
                }
            }
        }
    }
}