using System.Linq;
using Hai.ComboGestureDynamics.Scripts.Components;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdCompiler
    {
        private readonly Components.ComboGestureDynamics _cgd;
        private readonly CgdParameters _cgdParameters;

        public CgdCompiler(Components.ComboGestureDynamics cgd)
        {
            _cgd = cgd;
            _cgdParameters = new CgdParameters("CGD");
        }

        public void Compile()
        {
            var systems = DynamicsToSystems();
            foreach (var system in systems)
            {
                new CgdGenerator(system.activations, _cgdParameters).Generate();
            }
        }

        private CgdSys.System[] DynamicsToSystems()
        {
            var parts = new[] {_cgd.mainPart}.Concat(_cgd.secondaryParts).ToArray();
            return parts.Select((part, index) => ConvertPartToSystem(part, index == 0)).ToArray();
        }

        private CgdSys.System ConvertPartToSystem(CgdPart part, bool isFirstPart)
        {
            var activations = new CgdSysCompileActivation(_cgd, _cgdParameters).FlattenRulesToActivations(_cgd.rootRule, part, isFirstPart);
            new CgdSysCompileFallbackAndSplit(_cgd, activations).MutateAnimations();

            return new CgdSys.System
            {
                activations = activations,
                fxMask = null, // TODO
                gestureMask = null // TODO
            };
        }
    }
}