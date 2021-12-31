using System.Collections.Generic;
using System.Linq;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Components;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using UnityEditor.Animations;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdController : AnimatorAsCode
    {
    }

    public class CgdExpressionLibrary
    {
        private readonly List<Motion> motions;
        private readonly AacV0.AacFlBase<CgdController> _aac;
        private readonly CgdParameters parameters;

        public CgdExpressionLibrary(List<Motion> motions, CgdParameters parameters)
        {
            this.motions = motions.ToList();
            this.parameters = parameters;
            _aac = AacV0.Using(new CgdController());
        }

        public void Generate()
        {
            _aac.ResetAssetHolder();

            GenerateExpressionsLayer();
        }

        private void GenerateExpressionsLayer()
        {
            var expressionsLayer = _aac.CreateSupportingFxLayer("Expressions");
            var tweeningTree = CreateTweeningTree();
            expressionsLayer.NewState("Expressions", 0, 0).WithAnimation(tweeningTree);
        }

        private BlendTree CreateTweeningTree()
        {
            var expLibForA = CreateExpressionLibraryBlendTree(parameters.ExpressionA);
            var expLibForB = CreateExpressionLibraryBlendTree(parameters.ExpressionB);

            var tree = _aac.NewBlendTreeAsRaw();
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = parameters.TweeningAB;
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
            for (var index = 0; index < motions.Count; index++)
            {
                var motion = motions[index];
                tree.AddChild(motion, index);
            }

            return tree;
        }
    }
}