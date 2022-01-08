using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class CgdEffect : MonoBehaviour
    {
        public Cgd.EffectType effectType;

        public Cgd.Regular regular;
        public Cgd.Blend blend;
        public Motion customBlendTree;

        public void MutateAnyReferenceNormalize()
        {
            regular.MutateAnyReferenceNormalize();
            blend.MutateAnyReferenceNormalize();
        }
    }
}