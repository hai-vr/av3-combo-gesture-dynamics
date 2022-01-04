using System.Linq;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using VRC.SDK3.Avatars.Components;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdGenerator
    {
        private readonly CgdSys.Activation[] _activations;
        private readonly CgdParameters _cgdParameters;
        private readonly AacV0.AacFlBase<CgdController> _aac;

        internal CgdGenerator(VRCAvatarDescriptor avatar, CgdSys.Activation[] activations, CgdParameters cgdParameters)
        {
            _activations = activations;
            _cgdParameters = cgdParameters;
            _aac = AacV0.Using(new CgdController
            {
                avatar = avatar
            });
        }

        internal void Generate()
        {
            var orderedCompiledEffects = _activations.Select(activation => activation.compiledEffect).Distinct().ToArray();
            new CgdExpressionsLayer(_aac, _cgdParameters, orderedCompiledEffects).GenerateFX();
            new CgdTweeningLayer(_aac, _cgdParameters, _activations.Length).GenerateFX();
            new CgdActivationsLayer(_aac, _cgdParameters, orderedCompiledEffects.Length, _activations).GenerateFX();
        }
    }
}