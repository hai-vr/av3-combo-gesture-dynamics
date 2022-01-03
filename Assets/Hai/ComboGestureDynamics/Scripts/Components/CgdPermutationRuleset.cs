using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class CgdPermutationRuleset : MonoBehaviour
    {
        public Cgd.Condition[] conditions = new Cgd.Condition[0];
        public Cgd.Permutation[] permutations;

        public CgdPart[] parts = new CgdPart[0];
    }
}