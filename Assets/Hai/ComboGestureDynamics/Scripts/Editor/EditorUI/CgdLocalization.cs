using System;
using System.Linq;

public class CgdLocalization
{
    public static string Localize(Phrase phrase, params object[] args)
    {
        if (phrase == Phrase.Search) return "+";
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
        ConditionConditionFromComponent,
        And,
        AlwaysActive,
        BoolIsTrue,
        BoolIsFalse,
        MainPart,
        SecondaryParts,
        IgnoreHandSide,
        TweeningType,
        TweeningEntranceDurationSeconds,
        TweeningImportance,
        TweeningHasCustomExitDuration,
        TweeningExitDurationSeconds,
        PleaseSelectCgd,
        TweeningShape,
        Parameters,
        ExpressionParameters,
        VRChatParameters,
        ComponentParameters,
        AnimatorParameters,
        IntParameters,
        FloatParameters,
        BoolParameters,
        Search,
        HandGestures
    }

    public static string EnumLocalize(Enum enumValue, Type enumType)
    {
        var enumName = Enum.GetName(enumType, enumValue);
        var enumTypeName = enumType.Name;

        var key = $"Enum_{enumTypeName}_{enumName}";

        return $"{enumName}_{enumTypeName}";
    }
}