using Hai.Cge2Aac.Framework.Editor.Internal.V0;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.EditorUI
{
    [CustomEditor(typeof(AnimatorAsCode), true)]
    [CanEditMultipleObjects]
    public class AnimatorAsCodeEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (!serializedObject.isEditingMultipleObjects)
            {
                var current = (AnimatorAsCode)target;
                // EditorGUILayout.HelpBox(current.Describe(), MessageType.None);

                DrawDefaultInspector();

                if (GUILayout.Button("Create"))
                {
                    AacRegistry.Registry.Create(current);
                }
                EditorGUI.BeginDisabledGroup(!current._internal.created);
                if (GUILayout.Button("Remove"))
                {
                    AacRegistry.Registry.Remove(current);
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.TextField("Asset key (read only)", current._internal.assetKey);
            }
            else
            {
                DrawDefaultInspector();

                if (GUILayout.Button("Create all"))
                {
                    foreach (var t in targets)
                    {
                        AacRegistry.Registry.Create((AnimatorAsCode)t);
                    }
                }
            }
        }
    }
}
