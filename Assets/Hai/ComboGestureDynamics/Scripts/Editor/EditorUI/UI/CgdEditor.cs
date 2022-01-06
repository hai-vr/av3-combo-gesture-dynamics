using System;
using System.Linq;
using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    public class CgdEditor : EditorWindow
    {
        private Vector2 _scrollPos;
        private int _focus;

        private Components.ComboGestureDynamics _cgd;
        private Rect m_focusArea;
        private CgdRule _selectedRule;
        private CgdPermutationRuleset _selectedPermutationRuleset;

        private CgdEditorPermutationsLayout _cgdEditorPermutationsLayout;

        [MenuItem("Window/Haï/ComboGestureDynamics UI")]
        public static void ShowWindow()
        {
            var window = GetWindow<CgdEditor>();
            window.titleContent = new GUIContent("ComboGestureDynamics");
        }

        private void OnEnable()
        {
            _cgdEditorPermutationsLayout = new CgdEditorPermutationsLayout(this);
        }

        private void OnInspectorUpdate()
        {
            var active = Selection.activeGameObject;
            if (active == null) return;

            var cgd = active.GetComponent<Components.ComboGestureDynamics>();
            if (cgd == null) return;

            SelectCgd(cgd);
            Repaint();
        }

        private void SelectCgd(Components.ComboGestureDynamics cgd)
        {
            _cgd = cgd;
        }

        private void OnGUI()
        {
            // _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height));
            //

            // GUILayout.BeginVertical("GroupBox");
            // EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.Animations), EditorStyles.boldLabel);
            // GUILayout.EndVertical();
            GUILayout.BeginHorizontal("GroupBox");
            _focus = GUILayout.Toolbar(_focus, new[]
            {
                CgdLocalization.Localize(CgdLocalization.Phrase.Configuration),
                CgdLocalization.Localize(CgdLocalization.Phrase.Animations),
                CgdLocalization.Localize(CgdLocalization.Phrase.Permutations),
                CgdLocalization.Localize(CgdLocalization.Phrase.Rules),
                CgdLocalization.Localize(CgdLocalization.Phrase.Parts),
            });
            GUILayout.EndHorizontal();

            if (_cgd != null)
            {
                switch (_focus)
                {
                    case 2:
                        _cgdEditorPermutationsLayout.Layout();
                        return;
                    default:
                        break;
                }
            }

            if (_cgd != null)
            {
                GUILayout.BeginVertical("GroupBox");

                CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 7, EditorGUIUtility.singleLineHeight * 7), rect => m_focusArea = rect);
                GUILayout.BeginArea(m_focusArea);
                switch (_focus)
                {
                    case 0: // Configuration
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Component)), _cgd, typeof(Components.ComboGestureDynamics), true);
                        EditorGUI.EndDisabledGroup();

                        var cgdSerialized = new SerializedObject(_cgd);
                        EditorGUILayout.PropertyField(cgdSerialized.FindProperty(nameof(Components.ComboGestureDynamics.avatar)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.AvatarDescriptor)));
                        cgdSerialized.ApplyModifiedProperties();
                        break;
                    case 2:
                        break;
                    case 3:
                        if (_selectedRule != null)
                        {
                            // EditorGUI.BeginDisabledGroup(true);
                            // EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Component)), _selectedRule, typeof(CgdRule), true);
                            // EditorGUI.EndDisabledGroup();

                            var serializedRule = new SerializedObject(_selectedRule);
                            var serializedRuleObjectName = new SerializedObject(_selectedRule.gameObject);
                            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.SelectedRule, _selectedRule.name), EditorStyles.boldLabel);
                            EditorGUILayout.PropertyField(serializedRuleObjectName.FindProperty("m_Name"), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.RuleName)));
                            serializedRuleObjectName.ApplyModifiedProperties();
                            EditorGUILayout.PropertyField(serializedRule.FindProperty(nameof(CgdRule.conditions)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Conditions)));

                        }
                        else if (_selectedPermutationRuleset != null)
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Component)), _selectedPermutationRuleset, typeof(CgdRule), true);
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.SelectedRule, _selectedPermutationRuleset.name), EditorStyles.boldLabel);
                        }
                        else
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Component)), _cgd.rootRule, typeof(CgdRootRule), true);
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.SelectedRule, CgdLocalization.Localize(CgdLocalization.Phrase.DefaultRule)), EditorStyles.boldLabel);
                        }
                        break;
                    default:
                        break;
                }
                GUILayout.EndArea();

                GUILayout.EndVertical();

                GUILayout.BeginVertical("GroupBox");
                EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.VisibleParts), EditorStyles.boldLabel);
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            switch (_focus)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    RulesLayout();
                    break;
                case 4:
                    PartsLayout();
                    break;
                default:
                    break;
            }

            //
            // GUILayout.EndScrollView();
        }

        private void RulesLayout()
        {
            GUILayout.BeginVertical("GroupBox");
            ExpandRules(_cgd.rootRule.transform, 0, null);

            if (GUILayout.Button(CgdLocalization.Localize(CgdLocalization.Phrase.DefaultRule), _selectedRule == null && _selectedPermutationRuleset == null ? EditorStyles.boldLabel : EditorStyles.label))
            {
                SelectDefaultRule();
            }
            GUILayout.EndVertical();
        }

        private void PartsLayout()
        {
            GUILayout.BeginVertical("GroupBox");
            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.MainPart), EditorStyles.boldLabel);

            var editorCurveBindings = CgdEditorUiExtensions.FindAllProperties(_cgd);
            foreach (var editorCurveBinding in editorCurveBindings)
            {
                DisplayBinding(editorCurveBinding);
            }

            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.SecondaryParts), EditorStyles.boldLabel);
            GUILayout.EndVertical();
        }

        private void DisplayBinding(EditorCurveBinding editorCurveBinding)
        {
            var type = editorCurveBinding.type == typeof(SkinnedMeshRenderer) && editorCurveBinding.propertyName.StartsWith("blendShape") ? "::" : $"({editorCurveBinding.type})";
            var existsInAvatar = AnimationUtility.GetFloatValue(_cgd.avatar.gameObject, editorCurveBinding, out var valueInAvatarOrZero);
            var value = existsInAvatar ? $"= {CgdEditorUiExtensions.NoCulture(valueInAvatarOrZero)}" : "???";

            EditorGUILayout.BeginHorizontal();
            var animatedObject = AnimationUtility.GetAnimatedObject(_cgd.avatar.gameObject, editorCurveBinding);
            EditorGUILayout.LabelField($"{editorCurveBinding.path} {type}");
            EditorGUILayout.LabelField(editorCurveBinding.propertyName);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField(animatedObject, typeof(Object));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField(value);
            EditorGUILayout.EndHorizontal();
            // EditorGUILayout.LabelField(content);
        }

        private void ExpandRules(Transform ruleTransform, int level, CgdRule parentRuleNullableWhenRoot)
        {
            var indent = string.Join("", Enumerable.Repeat("    ", level));
            foreach (Transform subRuleTransform in ruleTransform)
            {
                var rule = subRuleTransform.GetComponent<CgdRule>();
                if (rule == null)
                {
                    var permutationRuleset = subRuleTransform.GetComponent<CgdPermutationRuleset>();
                    if (permutationRuleset != null)
                    {
                        var title = GenerateTitle(parentRuleNullableWhenRoot, permutationRuleset.conditions, $"{permutationRuleset.name} ({CgdLocalization.Localize(CgdLocalization.Phrase.Permutation)})");
                        var permutationTitle = $"{title}";
                        if (GUILayout.Button(indent + permutationTitle, permutationRuleset == _selectedPermutationRuleset ? EditorStyles.boldLabel : EditorStyles.label))
                        {
                            SelectRule(permutationRuleset);
                        }
                    }
                }
                else
                {
                    var title = GenerateTitle(parentRuleNullableWhenRoot, rule.conditions, rule.name);
                    if (GUILayout.Button(indent + title, rule == _selectedRule ? EditorStyles.boldLabel : EditorStyles.label))
                    {
                        SelectRule(rule);
                    }
                    ExpandRules(rule.transform, level + 1, rule);
                }
            }
        }

        private static string GenerateTitle(CgdRule parentRuleNullableWhenRoot, Cgd.Condition[] ruleConditions, string ruleName)
        {
            var baseCondition = parentRuleNullableWhenRoot == null ? new string[] { } : new[] {$"{CgdLocalization.Localize(CgdLocalization.Phrase.PassingRule, parentRuleNullableWhenRoot.name)}"};
            var allConditions = baseCondition.Concat(ruleConditions.Select(CgdEditorUiExtensions.LocalizeCondition).ToArray()).ToArray();
            if (allConditions.Length == 0)
            {
                return $"\"{ruleName}\" {CgdLocalization.Localize(CgdLocalization.Phrase.AlwaysActive)}";
            }

            var ruleCondition = string.Join($" {CgdLocalization.Localize(CgdLocalization.Phrase.And)} ", allConditions);
            return $"\"{ruleName}\" {CgdLocalization.Localize(CgdLocalization.Phrase.When)} {ruleCondition}";
        }

        private void SelectRule(CgdPermutationRuleset permutationRuleset)
        {
            _selectedRule = null;
            _selectedPermutationRuleset = permutationRuleset;
        }

        private void SelectRule(CgdRule rule)
        {
            _selectedRule = rule;
            _selectedPermutationRuleset = null;
        }

        private void SelectDefaultRule()
        {
            _selectedRule = null;
            _selectedPermutationRuleset = null;
        }
    }
}