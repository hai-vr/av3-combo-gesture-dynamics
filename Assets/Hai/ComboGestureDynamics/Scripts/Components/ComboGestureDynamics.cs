using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class ComboGestureDynamics : MonoBehaviour
    {
        public VRCAvatarDescriptor avatar;

        public CgdRootRule rootRule;
        public CgdPart mainPart;

        public CgdPart[] secondaryParts;
        public Transform effectsLibrary;

        public CgdMoodSelector moodSelector;
        public bool generateMoodSelector = true;
        public bool generateEyeTracking = true;
        public bool generateFistSmoothing = true;
    }
}