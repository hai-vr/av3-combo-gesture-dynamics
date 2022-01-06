using System;
using System.Linq;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI
{
    public class CgdLocalization
    {
        public static string Localize(Phrase phrase, params object[] args)
        {
            var suffix = args.Length == 0 ? "" : $" ({string.Join(", ", args.Select(o => $"{o}").ToArray())})";
            return LocalizeEnum(phrase) + suffix;
        }

        private static string LocalizeEnum(Phrase phrase)
        {
            // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
            switch (phrase)
            {
                case Phrase.When:
                    return "when";
                case Phrase.And:
                    return "and";
                default:
                    return Enum.GetName(typeof(Phrase), phrase);
            }
        }

        public enum Phrase
        {
            Rules,
            Permutations,
            Animations,
            VisibleParts,
            Configuration,
            AvatarDescriptor,
            HandPoseNeutral,
            HandPoseFist,
            HandPoseOpen,
            HandPosePoint,
            HandPoseVictory,
            HandPoseRockNRoll,
            HandPoseGun,
            HandPoseThumbsUp,
            Parts,
            Component,
            Permutation,
            SelectedRule,
            DefaultRule,
            RuleName,
            Conditions,
            When,
            PassingRule,
            ConditionHandGesture,
            ConditionDefaultMoodSelector,
            ConditionSpecificMoodSelector,
            ConditionParameterBoolValue,
            ConditionParameterIntValue,
            ConditionParameterFloatValue,
            ConditionParameterIntBetween,
            ConditionParameterFloatBetween,
            ConditionConditionFromComponent,
            And,
            AlwaysActive,
            BoolIsTrue,
            BoolIsFalse,
            MainPart,
            SecondaryParts
        }
    }
}