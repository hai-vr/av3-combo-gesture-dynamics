namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdParameters
    {
        private readonly string _prefix;

        public CgdParameters(string prefix)
        {
            _prefix = prefix;
        }

        public string ExpressionA => $"{_prefix}_ExpressionA";
        public string ExpressionB => $"{_prefix}_ExpressionB";
        public string TweeningAB => $"{_prefix}_TweeningAB";
    }
}