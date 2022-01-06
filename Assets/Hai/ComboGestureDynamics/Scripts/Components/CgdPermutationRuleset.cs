using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class CgdPermutationRuleset : MonoBehaviour
    {
        public Cgd.Condition[] conditions = new Cgd.Condition[0];
        public Cgd.PermutationEffectBehaviour[] permutationEffectBehaviours;
        public Cgd.PermutationStencil permutationStencil;

        public CgdPart[] parts = new CgdPart[0];
    }
}