using System.Collections.Generic;
using Hai.BlendshapeViewer.Scripts.Editor;
using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    public class CgdRenderQueue
    {
        private readonly Dictionary<CgdEffect, Texture2D> _effectToTexture;
        private Queue<CgdEffect> _queue;

        public CgdRenderQueue(CgdEditor cgdEditor)
        {
            _effectToTexture = new Dictionary<CgdEffect, Texture2D>();
            _queue = new Queue<CgdEffect>();
        }

        public void ForceClearAll()
        {
            _effectToTexture.Clear();
            _queue.Clear();
        }

        public Texture2D RequireRender(CgdEffect effect)
        {
            if (_effectToTexture.ContainsKey(effect)) return _effectToTexture[effect];

            var texture = new Texture2D(100, 100);
            _effectToTexture[effect] = texture; // TODO: Dimensions

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
            var viewer = new ModifiedBlendshapeViewerGenerator();
            try
            {
                viewer.Begin(copy, 0f, true);
                var head = copy.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                viewer.ParentCameraTo(head);

                while (_queue.Count > 0)
                {
                    var cgdEffect = _queue.Dequeue();
                    viewer.Render(cgdEffect.regular.insertedClips[0].clip, _effectToTexture[cgdEffect]);
                }
            }
            finally
            {
                viewer.Terminate();
            }
        }
    }
}