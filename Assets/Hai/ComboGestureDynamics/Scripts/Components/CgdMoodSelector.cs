using System;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Components
{
    public class CgdMoodSelector : MonoBehaviour
    {
        public Choice[] choices;
        public BehaviourOnDeselect behaviourOnDeselect;
        public bool generateParameterForDefault = true;

        [Serializable]
        public enum BehaviourOnDeselect
        {
            ReturnToDefault,
            KeepSelected
        }

        [Serializable]
        public struct Choice
        {
            public string parameterName;
        }
    }
}