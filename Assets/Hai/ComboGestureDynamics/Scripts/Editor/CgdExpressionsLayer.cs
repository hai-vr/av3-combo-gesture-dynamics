using System.Linq;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using UnityEditor.Animations;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    internal class CgdExpressionsLayer
    {
        private readonly CgdSys.CompiledEffect[] _orderedCompiledEffects;
        private readonly AacV0.AacFlBase<CgdController> _aac;
        private readonly CgdParameters _parameters;

        internal CgdExpressionsLayer(AacV0.AacFlBase<CgdController> aac, CgdParameters parameters, CgdSys.CompiledEffect[] orderedCompiledEffects)
        {
            _aac = aac;
            _parameters = parameters;
            _orderedCompiledEffects = orderedCompiledEffects;
        }

        public void GenerateFX()
        {
            Generate(false);
        }

        public void GenerateGesture()
        {
            Generate(true);
        }

        private void Generate(bool isGesture)
        {
            var layer = isGesture ? _aac.CreateSupportingGestureLayer("Expressions") : _aac.CreateSupportingFxLayer("Expressions");
            layer.NewState("Expressions", 0, 0).WithAnimation(CreateTweeningTree(false));
        }

        private BlendTree CreateTweeningTree(bool isGesture)
        {
            var expLibForA = CreateExpressionLibraryBlendTree(_parameters.ExpressionLow, isGesture);
            var expLibForB = CreateExpressionLibraryBlendTree(_parameters.ExpressionHigh, isGesture);

            var tree = _aac.NewBlendTreeAsRaw();
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = _parameters.Tweening;
            tree.useAutomaticThresholds = false;
            tree.AddChild(expLibForA, 0f);
            tree.AddChild(expLibForB, 1f);

            return tree;
        }

        private BlendTree CreateExpressionLibraryBlendTree(string parameter, bool isGesture)
        {
            var tree = _aac.NewBlendTreeAsRaw();
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = parameter;
            tree.useAutomaticThresholds = false;
            tree.children = _orderedCompiledEffects.Select((effect, index) => new ChildMotion
            {
                threshold = index,
                motion = isGesture ? effect.compiledMotion.gesture : effect.compiledMotion.fx
            }).ToArray();

            return tree;
        }
    }
}