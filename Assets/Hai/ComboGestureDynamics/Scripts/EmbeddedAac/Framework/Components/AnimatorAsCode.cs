using System;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Components
{
    public abstract class AnimatorAsCode : MonoBehaviour
    {
        public VRCAvatarDescriptor avatar;
        public RuntimeAnimatorController assetHolder;
        public string layerNameSuffix;
        public string parameterName;

        public AacInternal _internal;
    }

    [Serializable]
    public struct AacInternal
    {
        public bool created;
        public string assetKey;
        public int createdWithMajorVersion;
    }
}
