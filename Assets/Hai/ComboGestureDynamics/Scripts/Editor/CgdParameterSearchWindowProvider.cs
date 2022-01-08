using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdParameterSearchWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private Components.ComboGestureDynamics cgd;
        private CgdParameterCollector.UserParameterType filter;
        private SerializedProperty parameterNameSerialized;

        public static void Open(Components.ComboGestureDynamics cgd, CgdParameterCollector.UserParameterType userParameterType, SerializedProperty parameterNameSerialized)
        {
            var provider = CreateInstance<CgdParameterSearchWindowProvider>();
            provider.cgd = cgd;
            provider.filter = userParameterType;
            provider.parameterNameSerialized = parameterNameSerialized;

            SearchWindow.Open(new SearchWindowContext(MousePosition()), provider);
        }

        private static Vector2 MousePosition()
        {
            return GUIUtility.GUIToScreenPoint(Event.current != null ? Event.current.mousePosition : Vector2.zero);
        }

        public bool OnSelectEntry(SearchTreeEntry entry, SearchWindowContext context)
        {
            parameterNameSerialized.stringValue = entry.name;
            return true;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var filteredParameters = CgdParameterCollector.Collect(cgd, filter);
            var title = Title();

            var ste = new List<SearchTreeEntry>();
            ste.Add(new SearchTreeGroupEntry(new GUIContent(CgdLocalization.Localize(title)), 0));
            ste.Add(Group(CgdLocalization.Phrase.ExpressionParameters));
            ste.AddRange(Remap(filteredParameters.expressionParameters));
            ste.Add(Group(CgdLocalization.Phrase.VRChatParameters));
            ste.AddRange(Remap(filteredParameters.vrchatParameters));
            ste.Add(Group(CgdLocalization.Phrase.ComponentParameters));
            ste.AddRange(Remap(filteredParameters.componentParameters));
            ste.Add(Group(CgdLocalization.Phrase.AnimatorParameters));
            ste.AddRange(Remap(filteredParameters.animatorParameters));
            return ste;
        }

        private static SearchTreeGroupEntry Group(CgdLocalization.Phrase phrase)
        {
            return new SearchTreeGroupEntry(new GUIContent(CgdLocalization.Localize(phrase)), 1);
        }

        private CgdLocalization.Phrase Title()
        {
            switch (filter)
            {
                case CgdParameterCollector.UserParameterType.IntParam:
                    return CgdLocalization.Phrase.IntParameters;
                case CgdParameterCollector.UserParameterType.FloatParam:
                    return CgdLocalization.Phrase.FloatParameters;
                case CgdParameterCollector.UserParameterType.BoolParam:
                    return CgdLocalization.Phrase.BoolParameters;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return CgdLocalization.Phrase.Parameters;
        }

        private static SearchTreeEntry[] Remap(CgdParameterCollector.UserParameter[] allParametersExpressionParameters)
        {
            return allParametersExpressionParameters
                .Select(parameter => new SearchTreeEntry(new GUIContent(parameter.name)) {level = 2, userData = parameter})
                .ToArray();
        }
    }

    public class CgdParameterCollector
    {
        public static CgdEdiParameters Collect(Components.ComboGestureDynamics cgd, UserParameterType filter)
        {
            var filterPredicate = Filter(filter);
            return new CgdEdiParameters
            {
                expressionParameters = AllExpressionParameters(cgd).Where(filterPredicate).ToArray(),
                animatorParameters = AllAnimatorParametersFromFxAndGesture(cgd).Where(filterPredicate).ToArray(),
                vrchatParameters = AllVrchatParameters().Where(filterPredicate).ToArray(),
                componentParameters = new UserParameter[0]
            };
        }

        private static Func<UserParameter, bool> Filter(UserParameterType userParameterTypeNullableIfNoFilter)
        {
            return parameter => parameter.type == userParameterTypeNullableIfNoFilter;
        }

        private static UserParameter[] AllVrchatParameters()
        {
            return new[]
            {
                new UserParameter {name = "IsLocal", type = UserParameterType.BoolParam},
                new UserParameter {name = "Viseme", type = UserParameterType.IntParam},
                new UserParameter {name = "GestureLeft", type = UserParameterType.IntParam},
                new UserParameter {name = "GestureRight", type = UserParameterType.IntParam},
                new UserParameter {name = "GestureLeftWeight", type = UserParameterType.FloatParam},
                new UserParameter {name = "GestureRightWeight", type = UserParameterType.FloatParam},
                new UserParameter {name = "AngularY", type = UserParameterType.FloatParam},
                new UserParameter {name = "VelocityX", type = UserParameterType.FloatParam},
                new UserParameter {name = "VelocityY", type = UserParameterType.FloatParam},
                new UserParameter {name = "VelocityZ", type = UserParameterType.FloatParam},
                new UserParameter {name = "Upright", type = UserParameterType.FloatParam},
                new UserParameter {name = "Grounded", type = UserParameterType.BoolParam},
                new UserParameter {name = "Seated", type = UserParameterType.BoolParam},
                new UserParameter {name = "AFK", type = UserParameterType.BoolParam},
                new UserParameter {name = "TrackingType", type = UserParameterType.IntParam},
                new UserParameter {name = "VRMode", type = UserParameterType.IntParam},
                new UserParameter {name = "MuteSelf", type = UserParameterType.BoolParam},
                new UserParameter {name = "InStation", type = UserParameterType.BoolParam},
            };
        }

        private static UserParameter[] AllAnimatorParametersFromFxAndGesture(Components.ComboGestureDynamics cgd)
        {
            return cgd.avatar.baseAnimationLayers
                .Where(layer => layer.type == VRCAvatarDescriptor.AnimLayerType.Gesture || layer.type == VRCAvatarDescriptor.AnimLayerType.FX).ToArray()
                .SelectMany(layer => ((AnimatorController)layer.animatorController).parameters)
                .Where(parameter => parameter.type != AnimatorControllerParameterType.Trigger)
                .Select(parameter => new UserParameter
                {
                    name = parameter.name,
                    type = RemapValid(parameter.type)
                })
                .ToArray();
        }

        private static UserParameter[] AllExpressionParameters(Components.ComboGestureDynamics cgd)
        {
            return cgd.avatar.expressionParameters.parameters.Select(parameter => new UserParameter
            {
                name = parameter.name,
                type = Remap(parameter.valueType)
            }).ToArray();
        }

        private static UserParameterType Remap(VRCExpressionParameters.ValueType valueType)
        {
            switch (valueType)
            {
                case VRCExpressionParameters.ValueType.Int:
                    return UserParameterType.IntParam;
                case VRCExpressionParameters.ValueType.Float:
                    return UserParameterType.FloatParam;
                case VRCExpressionParameters.ValueType.Bool:
                    return UserParameterType.BoolParam;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static UserParameterType RemapValid(AnimatorControllerParameterType parameterType)
        {
            switch (parameterType)
            {
                case AnimatorControllerParameterType.Float:
                    return UserParameterType.FloatParam;
                case AnimatorControllerParameterType.Int:
                    return UserParameterType.IntParam;
                case AnimatorControllerParameterType.Bool:
                    return UserParameterType.BoolParam;
                case AnimatorControllerParameterType.Trigger: // Filtered beforehand
                default:
                    throw new ArgumentOutOfRangeException(nameof(parameterType), parameterType, null);
            }
        }

        public struct CgdEdiParameters
        {
            public UserParameter[] expressionParameters;
            public UserParameter[] animatorParameters;
            public UserParameter[] vrchatParameters;
            public UserParameter[] componentParameters;
        }

        public struct UserParameter
        {
            public string name;
            public UserParameterType type;
        }

        public enum UserParameterType
        {
            IntParam, FloatParam, BoolParam
        }
    }

    public class MyClass
    {
    }
}