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
        public string DefaultMoodSelector => $"{_prefix}_{nameof(DefaultMoodSelector)}";
        public string IsLeftEyeClosed => $"{_prefix}_{nameof(IsLeftEyeClosed)}";
        public string TweenIsReady => $"{_prefix}_{nameof(TweenIsReady)}";
        public string TweenIsHigh => $"{_prefix}_{nameof(TweenIsHigh)}";
        public string TweenShape => $"{_prefix}_{nameof(TweenShape)}";
        public string TweenDuration => $"{_prefix}_{nameof(TweenDuration)}";
        public string ActivationIsHigh => $"{_prefix}_{nameof(ActivationIsHigh)}";
        public string VariationOfGestureLeftWeight => "GestureLeftWeight_Smoothing"; // TODO: Make this vary depending on user settings
        public string VariationOfGestureRightWeight => "GestureLeftWeight_Smoothing";

        public string SpecificMoodSelector(Cgd.SpecificMoodSelector specificMoodSelector)
        {
            return specificMoodSelector.moodSelector.selectorName;
        }
    }
}