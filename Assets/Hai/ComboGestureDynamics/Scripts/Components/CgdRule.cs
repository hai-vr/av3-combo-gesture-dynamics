using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class CgdRule : MonoBehaviour
    {
        public Cgd.Condition[] conditions = new Cgd.Condition[0];
        public Cgd.EffectBehaviour effectBehaviour;
        public Cgd.TweeningType tweeningType;
        public Cgd.Tweening tweening;

        public CgdPart[] parts = new CgdPart[0];
    }
}