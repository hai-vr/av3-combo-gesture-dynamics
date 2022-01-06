using System.Linq;
using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    internal class CgdEditorRulesLayout
    {
        private readonly CgdEditor _cgdEditor;
        private Rect m_focusAreaRect;

        private CgdRule _selectedRule;
        private CgdPermutationRuleset _selectedPermutationRuleset;

        public CgdEditorRulesLayout(CgdEditor cgdEditor)
        {
            _cgdEditor = cgdEditor;
        }

        public void Layout()
        {
            EditorGUILayout.BeginVertical("GroupBox");
            CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 7, EditorGUIUtility.singleLineHeight * 7), rect => m_focusAreaRect = rect);
            GUILayout.BeginArea(m_focusAreaRect);
            EditorGUILayout.BeginHorizontal();

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
                EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Component)), _cgdEditor.cgd.rootRule, typeof(CgdRootRule), true);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.SelectedRule, CgdLocalization.Localize(CgdLocalization.Phrase.DefaultRule)), EditorStyles.boldLabel);
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("GroupBox");
            ExpandRules(_cgdEditor.cgd.rootRule.transform, 0, null);

            if (GUILayout.Button(CgdLocalization.Localize(CgdLocalization.Phrase.DefaultRule), _selectedRule == null && _selectedPermutationRuleset == null ? EditorStyles.boldLabel : EditorStyles.label))
            {
                SelectDefaultRule();
            }
            GUILayout.EndVertical();
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
    }
}