using System.Collections.Generic;
using System.Linq;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdExpressionsLayer
    {
        private readonly List<Motion> _motions;
        private readonly AacV0.AacFlBase<CgdController> _aac;
        private readonly CgdParameters _parameters;

        internal CgdExpressionsLayer(AacV0.AacFlBase<CgdController> aac, CgdParameters parameters, List<Motion> motions)
        {
            _aac = aac;
            _parameters = parameters;
            _motions = motions;
        }

        public void Generate()
        {
            var layer = _aac.CreateSupportingFxLayer("Expressions");
            layer.NewState("Expressions", 0, 0).WithAnimation(CreateTweeningTree());
        }

        private BlendTree CreateTweeningTree()
        {
            var expLibForA = CreateExpressionLibraryBlendTree(_parameters.ExpressionLow);
            var expLibForB = CreateExpressionLibraryBlendTree(_parameters.ExpressionHigh);

            var tree = _aac.NewBlendTreeAsRaw();
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = _parameters.Tweening;
            tree.useAutomaticThresholds = false;
            tree.AddChild(expLibForA, 0f);
            tree.AddChild(expLibForB, 1f);

            return tree;
        }

        private BlendTree CreateExpressionLibraryBlendTree(string parameter)
        {
            var tree = _aac.NewBlendTreeAsRaw();
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = parameter;
            tree.useAutomaticThresholds = false;
            for (var index = 0; index < _motions.Count; index++)
            {
                var motion = _motions[index];
                tree.AddChild(motion, index);
            }

            return tree;
        }
    }
}