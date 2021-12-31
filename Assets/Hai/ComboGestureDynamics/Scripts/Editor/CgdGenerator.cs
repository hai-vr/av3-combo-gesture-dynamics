using System.Collections.Generic;
using System.Linq;
using Hai.ComboGestureDynamics.Scripts.EmbeddedAac.Framework.Editor.Internal.V0;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdGenerator
    {
        private readonly List<Motion> motions;
        private readonly AacV0.AacFlBase<CgdController> _aac;
        private readonly CgdParameters parameters;

        internal CgdGenerator(List<Motion> motions, CgdParameters parameters)
        {
            this.motions = motions.ToList();
            this.parameters = parameters;
            _aac = AacV0.Using(new CgdController());
        }

        public void Generate()
        {
            new CgdExpressionsLayer(_aac, parameters, motions).Generate();
            new CgdTweeningLayer(_aac, parameters).Generate();
        }
    }
}