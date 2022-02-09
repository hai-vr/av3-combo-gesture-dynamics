using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class ComboGestureDynamics : MonoBehaviour
    {
        public VRCAvatarDescriptor avatar;
        public Ee2Avatar previewAvatar;

        public CgdRootRule rootRule;
        public CgdPart mainPart;

        public CgdPart[] secondaryParts;
        public Transform effectsLibrary;

        public CgdMoodSelector moodSelector;
        public bool generateMoodSelector = true;
        public bool generateEyeTracking = true;
        public bool generateFistSmoothing = true;
        public Cgd.UiComplexity uiComplexity;

        public void MutateAnyReferenceNormalize()
        {
            rootRule.MutateAnyReferenceNormalize();
            mainPart.MutateAnyReferenceNormalize();
            if (secondaryParts == null) secondaryParts = new CgdPart[0];
            foreach (var secondaryPart in secondaryParts)
            {
                secondaryPart.MutateAnyReferenceNormalize();
            }
            if (moodSelector != null) moodSelector.MutateAnyReferenceNormalize();
        }
    }
}