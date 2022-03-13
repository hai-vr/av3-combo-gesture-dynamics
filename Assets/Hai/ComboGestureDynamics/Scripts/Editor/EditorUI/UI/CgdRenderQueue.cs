using System.Collections.Generic;
// using Hai.BlendshapeViewer.Scripts.Editor;
using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    public class CgdRenderQueue
    {
        private readonly Dictionary<AnimationClip, Texture2D> _clipToTexture;
        private Queue<AnimationClip> _queue;

        public CgdRenderQueue(CgdEditor cgdEditor)
        {
            _clipToTexture = new Dictionary<AnimationClip, Texture2D>();
            _queue = new Queue<AnimationClip>();
        }

        public void ForceClearAll()
        {
            _clipToTexture.Clear();
            _queue.Clear();
        }

        public Texture2D RequireRender(AnimationClip effect)
        {
            if (_clipToTexture.ContainsKey(effect)) return _clipToTexture[effect];

            var texture = new Texture2D(100, 100);
            _clipToTexture[effect] = texture; // TODO: Dimensions

            _queue.Enqueue(effect);

            return texture;
        }

        public bool TryRender(Components.ComboGestureDynamics cgd)
        {
            if (_queue.Count == 0) return false;

            var originalAvatarGo = cgd.avatar.gameObject;
            GameObject copy = null;
            var wasActive = originalAvatarGo.activeSelf;
            try
            {
                copy = Object.Instantiate(originalAvatarGo);
                copy.SetActive(true);
                originalAvatarGo.SetActive(false);
                Render(copy);
            }
            finally
            {
                if (wasActive) originalAvatarGo.SetActive(true);
                if (copy != null) Object.DestroyImmediate(copy);
            }

            return true;
        }

        private void Render(GameObject copy)
        {
            // var viewer = new ModifiedBlendshapeViewerGenerator();
            // try
            // {
            //     viewer.Begin(copy, 0f, true);
            //     var head = copy.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            //     viewer.ParentCameraTo(head);
            //
            //     while (_queue.Count > 0)
            //     {
            //         var cgdEffect = _queue.Dequeue();
            //         viewer.Render(cgdEffect.regular.insertedClips[0].clip, _effectToTexture[cgdEffect]);
            //     }
            // }
            // finally
            // {
            //     viewer.Terminate();
            // }
        }
    }
}