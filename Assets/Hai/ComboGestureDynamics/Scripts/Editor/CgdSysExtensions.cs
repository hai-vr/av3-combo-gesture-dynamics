using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdSysExtensions
    {
        public static bool IsSameAsBinding(Cgd.PropertyMask propertyMask, EditorCurveBinding binding)
        {
            return propertyMask.path == binding.path && propertyMask.type == binding.type && propertyMask.propertyName == binding.propertyName;
        }
    }
}