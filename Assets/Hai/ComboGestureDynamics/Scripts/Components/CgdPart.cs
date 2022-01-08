using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class CgdPart : MonoBehaviour
    {
        public Cgd.PropertyMask[] acceptedProperties = new Cgd.PropertyMask[0];
        public Cgd.Tag[] acceptedTags = new Cgd.Tag[0];

        public void MutateAnyReferenceNormalize()
        {
            if (acceptedProperties == null) acceptedProperties = new Cgd.PropertyMask[0];
            if (acceptedTags == null) acceptedTags = new Cgd.Tag[0];
        }
    }
}