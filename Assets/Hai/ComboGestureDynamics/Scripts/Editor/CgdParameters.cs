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

        public string ExpressionLow => $"{_prefix}_{nameof(ExpressionLow)}";
        public string ExpressionHigh => $"{_prefix}_{nameof(ExpressionHigh)}";
        public string Tweening => $"{_prefix}_{nameof(Tweening)}";
        // public string CommandToHigh => $"{_prefix}_{nameof(CommandToHigh)}";
        // public string CommandToLow => $"{_prefix}_{nameof(CommandToLow)}";
        // public string ShapeToHigh => $"{_prefix}_{nameof(ShapeToHigh)}";
        // public string ShapeToLow => $"{_prefix}_{nameof(ShapeToLow)}";
        // public string DurationToHigh => $"{_prefix}_{nameof(DurationToHigh)}";
        // public string DurationToLow => $"{_prefix}_{nameof(DurationToLow)}";
        public string DefaultMoodSelector => $"{_prefix}_{nameof(DefaultMoodSelector)}";
        public string IsLeftEyeClosed => $"{_prefix}_{nameof(IsLeftEyeClosed)}";
        public string TweenIsReady => $"{_prefix}_{nameof(TweenIsReady)}";
        public string TweenIsHigh => $"{_prefix}_{nameof(TweenIsHigh)}";
        // public string ExpressionDiff => $"{_prefix}_{nameof(ExpressionDiff)}";
        public string TweenShape => $"{_prefix}_{nameof(TweenShape)}";
        public string TweenDuration => $"{_prefix}_{nameof(TweenDuration)}";
        public string TweenRequested => $"{_prefix}_{nameof(TweenRequested)}";
        public string ActivationIsHigh => $"{_prefix}_{nameof(ActivationIsHigh)}";

        public string SpecificMoodSelector(Cgd.SpecificMoodSelector specificMoodSelector)
        {
            return specificMoodSelector.moodSelector.selectorName;
        }
    }
}