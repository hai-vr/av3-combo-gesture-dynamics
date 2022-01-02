using Hai.ComboGestureDynamics.Scripts.Components;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdParameters
    {
        private readonly string _prefix;

        public CgdParameters(string prefix)
        {
            _prefix = prefix;
        }

        public string ExpressionLow => $"{_prefix}_ExpressionLow";
        public string ExpressionHigh => $"{_prefix}_ExpressionHigh";
        public string Tweening => $"{_prefix}_Tweening";
        public string CommandToHigh => $"{_prefix}_CommandToHigh";
        public string CommandToLow => $"{_prefix}_CommandToLow";
        public string ShapeToHigh => $"{_prefix}_ShapeToHigh";
        public string ShapeToLow => $"{_prefix}_ShapeToLow";
        public string DurationToHigh => $"{_prefix}_DurationToHigh";
        public string DurationToLow => $"{_prefix}_DurationToLow";
        public string DefaultMoodSelector => $"{_prefix}_DefaultMoodSelector";

        public string SpecificMoodSelector(Cgd.SpecificMoodSelector specificMoodSelector)
        {
            return specificMoodSelector.moodSelector.selectorName;
        }
    }
}