using System.Linq;
using Hai.ComboGestureDynamics.Scripts.Components;
using Hai.ComboGestureDynamics.Scripts.Editor;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ee2DebugPreview))]
public class Ee2DebugPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var total = ((Ee2DebugPreview) target).tex2ds.Length;

        if (GUILayout.Button("Run"))
        {
            Run();
        }
        if (GUILayout.Button("test"))
        {
            var sceneCamera = SceneView.lastActiveSceneView.camera;
            var ngo = new GameObject().AddComponent<Camera>();
            ngo.transform.position = sceneCamera.transform.position;
            ngo.transform.rotation = sceneCamera.transform.rotation;
            ngo.fieldOfView = sceneCamera.fieldOfView;
            ngo.orthographic = sceneCamera.orthographic;
            ngo.orthographicSize = sceneCamera.orthographicSize;
        }
        var mod = Mathf.Max(1, Screen.width / 104 - 1);
        for (var index = 0; index < total; index++)
        {
            var texture2D = ((Ee2DebugPreview) target).tex2ds[index];
            if (index % mod == 0)
            {
                EditorGUILayout.BeginHorizontal();
            }
            GUILayout.Box(texture2D);
            if ((index + 1) % mod == 0 || index == total - 1)
            {
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    private void Run()
    {
        var that = (Ee2DebugPreview) target;

        var module = new Ee2PreviewModule();
        try
        {
            module.Begin(that.avatar);
            Texture2D neutralTexture = null;
            if (that.generateDiff)
            {
                neutralTexture = NewTexture();
                module.Render(EmptyClip(), neutralTexture);
            }

            var results = that.avatar.relevantSmrs
                .SelectMany(relevantSmr =>
                {
                    var sharedMesh = relevantSmr.sharedMesh;

                    return Enumerable.Range(0, sharedMesh.blendShapeCount)
                        .Select(i =>
                        {
                            var blendShapeName = sharedMesh.GetBlendShapeName(i);
                            var tempClip = new AnimationClip();
                            AnimationUtility.SetEditorCurve(
                                tempClip,
                                new EditorCurveBinding
                                {
                                    path = AnimationUtility.CalculateTransformPath(relevantSmr.transform, that.avatar.dummy.transform),
                                    type = typeof(SkinnedMeshRenderer),
                                    propertyName = $"blendShape.{blendShapeName}"
                                },
                                AnimationCurve.Constant(0, 1 / 60f, 100f)
                            );

                            return tempClip;
                        })
                        .ToArray();
                })
                .Concat(that.clips)
                .ToArray();

            ((Ee2DebugPreview) target).tex2ds = results
                .Select((clip, i) =>
                {
                    if (i % 10 == 0)
                    {
                        EditorUtility.DisplayProgressBar(CgdLocalization.Localize(CgdLocalization.Phrase.Rendering), CgdLocalization.Localize(CgdLocalization.Phrase.Rendering), 1f * i / results.Length);
                    }
                    var result = NewTexture();
                    module.Render(clip, result);
                    return result;
                })
                .Select(texture2D =>
                {
                    var result = NewTexture();
                    module.Diff(texture2D, neutralTexture, result);
                    return result;
                })
                .ToArray();
        }
        finally
        {
            module.Terminate();
            EditorUtility.ClearProgressBar();
        }
    }

    private static AnimationClip EmptyClip()
    {
        var emptyClip = new AnimationClip();
        AnimationUtility.SetEditorCurve(
            emptyClip,
            new EditorCurveBinding
            {
                path = "_ignored",
                type = typeof(GameObject),
                propertyName = "m_Active"
            },
            AnimationCurve.Constant(0, 1 / 60f, 100f)
        );
        return emptyClip;
    }

    private static Texture2D NewTexture()
    {
        var newTexture = new Texture2D(100, 100, TextureFormat.RGB24, false);
        newTexture.wrapMode = TextureWrapMode.Clamp;
        return newTexture;
    }
}