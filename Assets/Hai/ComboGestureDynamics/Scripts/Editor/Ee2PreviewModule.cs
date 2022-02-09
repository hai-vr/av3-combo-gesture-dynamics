using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class Ee2PreviewModule
    {
        private GameObject _originalGo;
        private Ee2Avatar _avatarCopy;
        private bool _wasActive;
        private Material _materialCopy;

        public void Begin(Ee2Avatar originalAvatar)
        {
            _originalGo = originalAvatar.gameObject;
            _wasActive = _originalGo.activeSelf;
            _originalGo.SetActive(false);
            _avatarCopy = Object.Instantiate(originalAvatar);
            _avatarCopy.gameObject.SetActive(true);
            _materialCopy = Object.Instantiate(_avatarCopy.diffMaterial);
        }

        public void Terminate()
        {
            Object.DestroyImmediate(_avatarCopy.gameObject);
            Object.DestroyImmediate(_materialCopy);
            _originalGo.SetActive(_wasActive);
        }

        public void Render(AnimationClip clip, Texture2D element)
        {
            try
            {
                var sceneCamera = SceneView.lastActiveSceneView.camera;
                _avatarCopy.cameras[0].transform.position = sceneCamera.transform.position;
                _avatarCopy.cameras[0].transform.rotation = sceneCamera.transform.rotation;
                var whRatio = (1f * sceneCamera.pixelWidth / sceneCamera.pixelHeight);
                _avatarCopy.cameras[0].fieldOfView = whRatio < 1 ? sceneCamera.fieldOfView * whRatio : sceneCamera.fieldOfView;
                _avatarCopy.cameras[0].orthographic = sceneCamera.orthographic;
                _avatarCopy.cameras[0].nearClipPlane = sceneCamera.nearClipPlane;
                _avatarCopy.cameras[0].farClipPlane = sceneCamera.farClipPlane;
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_avatarCopy.dummy.gameObject, clip, 1 / 60f);
                AnimationMode.EndSampling();

                var renderTexture = RenderTexture.GetTemporary(element.width, element.height, 24);
                renderTexture.wrapMode = TextureWrapMode.Clamp;

                RenderCamera(renderTexture, _avatarCopy.cameras[0]);
                RenderTextureTo(renderTexture, element);
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            finally
            {
                AnimationMode.StopAnimationMode();
            }
        }

        private static void RenderCamera(RenderTexture renderTexture, Camera camera)
        {
            var originalRenderTexture = camera.targetTexture;
            var originalAspect = camera.aspect;
            try
            {
                camera.targetTexture = renderTexture;
                camera.aspect = (float) renderTexture.width / renderTexture.height;
                camera.Render();
            }
            finally
            {
                camera.targetTexture = originalRenderTexture;
                camera.aspect = originalAspect;
            }
        }

        private static void RenderTextureTo(RenderTexture renderTexture, Texture2D texture2D)
        {
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
        }

        public void Diff(Texture2D source, Texture2D neutralTexture, Texture2D newTexture)
        {
            var diff = RenderTexture.GetTemporary(newTexture.width, newTexture.height, 24);
            _materialCopy.SetTexture("_NeutralTex", neutralTexture);
            Graphics.Blit(source, diff, _materialCopy);
            RenderTextureTo(diff, newTexture);
            RenderTexture.ReleaseTemporary(diff);
        }
    }
}