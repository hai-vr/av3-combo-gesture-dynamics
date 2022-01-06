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
        private const int PermutationWidth = 100;
        private const int PermutationHeight = 90;
        private static Color LeftSideBg => EditorGUIUtility.isProSkin ? new Color(0.62f, 0.4f, 0.12f) : new Color(1f, 0.81f, 0.59f);
        private static Color RightSideBg => EditorGUIUtility.isProSkin ? new Color(0.24f, 0.48f, 0.62f) : new Color(0.7f, 0.9f, 1f);
        private static Color NeutralSideBg => EditorGUIUtility.isProSkin ? new Color(0.07f, 0.07f, 0.07f) : new Color(0.88f, 0.88f, 0.88f);
        private static Color LeftSymmetricalBg => EditorGUIUtility.isProSkin ? new Color(0.34f, 0.27f, 0.23f) : new Color(0.7f, 0.65f, 0.59f);
        private static Color RightSymmetricalBg => EditorGUIUtility.isProSkin ? new Color(0.23f, 0.26f, 0.34f) : new Color(0.56f, 0.64f, 0.7f);
        private static Color InconsistentBg => EditorGUIUtility.isProSkin ? new Color(0.72f, 0.09f, 0.27f) : new Color(1f, 0.41f, 0.54f);
        private static Color HighlightMain => EditorGUIUtility.isProSkin ? new Color(0.61f, 0.61f, 0.61f) : Color.white;
        private static Color HighlightSecondary => EditorGUIUtility.isProSkin ? new Color(0.43f, 0.43f, 0.43f) : new Color(0.82f, 0.82f, 0.82f);
        private Vector2 _scrollPos;
        private Rect m_permutationArea;
        private static bool _anySelectedPermutation;
        private static CgdEdi.Permutation _selectedPermutation;
        private int _focus;

        private Components.ComboGestureDynamics _cgd;
        private Rect m_focusArea;
        private Rect m_focusPermutationSelectedRect;
        private Rect m_focusPermutationMirrorRect;
        private CgdRule _selectedRule;
        private CgdPermutationRuleset _selectedPermutationRuleset;

        [MenuItem("Window/Haï/ComboGestureDynamics UI")]
        public static void ShowWindow()
        {
            var window = GetWindow<CgdEditor>();
            window.titleContent = new GUIContent("ComboGestureDynamics");
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

            var cgdSerialized = new SerializedObject(_cgd);

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

            GUILayout.BeginHorizontal();
            if (_cgd != null)
            {
                GUILayout.BeginVertical("GroupBox");

                RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 7, EditorGUIUtility.singleLineHeight * 7), rect => m_focusArea = rect);
                GUILayout.BeginArea(m_focusArea);
                switch (_focus)
                {
                    case 0: // Configuration
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Component)), _cgd, typeof(Components.ComboGestureDynamics), true);
                        EditorGUI.EndDisabledGroup();

                        EditorGUILayout.PropertyField(cgdSerialized.FindProperty(nameof(Components.ComboGestureDynamics.avatar)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.AvatarDescriptor)));
                        break;
                    case 2:
                        if (_anySelectedPermutation)
                        {
                            EditorGUILayout.BeginHorizontal();

                            var leftSide = _selectedPermutation.IsLeft() ? _selectedPermutation : _selectedPermutation.Mirror();
                            var rightSide = leftSide.Mirror();

                            RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth, PermutationHeight), rect => m_focusPermutationSelectedRect = rect);
                            GUILayout.BeginArea(m_focusPermutationSelectedRect);
                            DrawPermutationBox(leftSide, true);
                            GUILayout.EndArea();

                            if (!_selectedPermutation.IsSymmetrical())
                            {
                                RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth, PermutationHeight), rect => m_focusPermutationMirrorRect = rect);
                                GUILayout.BeginArea(m_focusPermutationMirrorRect);
                                DrawPermutationBox(rightSide, true);
                                GUILayout.EndArea();
                            }

                            GUILayout.FlexibleSpace();

                            EditorGUILayout.EndHorizontal();
                        }
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
                    PermutationsLayout();
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

            cgdSerialized.ApplyModifiedProperties();
            //
            // GUILayout.EndScrollView();
        }

        private void PermutationsLayout()
        {
            GUILayout.BeginVertical("GroupBox");
            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.Permutations, "dummy"), EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth * 8, PermutationHeight * 8), rect => m_permutationArea = rect);
            GUILayout.BeginArea(m_permutationArea);
            for (var left = 0; left <= (int) Cgd.HandGesture.HandPose.ThumbsUp; left++)
            {
                var leftPose = (Cgd.HandGesture.HandPose) left;
                for (var right = 0; right <= (int) Cgd.HandGesture.HandPose.ThumbsUp; right++)
                {
                    var rightPose = (Cgd.HandGesture.HandPose) right;
                    GUILayout.BeginArea(RectAt(right, left));
                    var permutation = new CgdEdi.Permutation(leftPose, rightPose);
                    DrawPermutationBox(permutation);
                    GUILayout.EndArea();
                }
            }

            GUILayout.EndArea();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.EndVertical();
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

        private static void DrawPermutationBox(CgdEdi.Permutation permutation, bool bypassSelection = false)
        {
            if (!bypassSelection && _anySelectedPermutation && _selectedPermutation.left == permutation.left && _selectedPermutation.right == permutation.right)
            {
                DrawColoredBackground(HighlightMain);
            }
            else if (!bypassSelection && _anySelectedPermutation && _selectedPermutation.left == permutation.right && _selectedPermutation.right == permutation.left)
            {
                DrawColoredBackground(HighlightSecondary);
            }
            else
            {
                DrawColoredBackground(permutation.left == permutation.right ? NeutralSideBg : (int)permutation.left > (int)permutation.right ? LeftSideBg : RightSideBg);
            }

            GUILayout.BeginVertical();

            EditorGUILayout.LabelField(new CgdEdi.Permutation(permutation.left, permutation.right).ToLocalizedName());
            // GUILayout.Box("", GUILayout.Width(PermutationWidth), GUILayout.Height(PermutationHeight - EditorGUIUtility.singleLineHeight * 3));
            if (GUILayout.Button(GUIContent.none, GUIStyle.none, GUILayout.Width(PermutationWidth), GUILayout.Height(PermutationHeight - EditorGUIUtility.singleLineHeight * 2)))
            {
                SelectPermutation(permutation.left, permutation.right);
            }
            GUILayout.EndVertical();
        }

        private static void SelectPermutation(Cgd.HandGesture.HandPose leftPose, Cgd.HandGesture.HandPose rightPose)
        {
            _anySelectedPermutation = true;
            _selectedPermutation = new CgdEdi.Permutation(leftPose, rightPose);
        }

        private static void DrawColoredBackground(Color color)
        {
            var col = GUI.color;
            try
            {
                GUI.color = color;
                GUI.Box(new Rect(0, 0, PermutationWidth, PermutationHeight), "", new GUIStyle("box") {
                    normal = new GUIStyleState
                    {
                        background = Texture2D.whiteTexture
                    }
                });
            }
            finally
            {
                GUI.color = col;
            }
        }

        private static Rect RectAt(int xGrid, int yGrid)
        {
            return new Rect(xGrid * PermutationWidth, yGrid * PermutationHeight, PermutationWidth - 3, PermutationHeight - 3);
        }

        private static void RectOnRepaint(Func<Rect> rectFn, Action<Rect> applyFn)
        {
            var rect = rectFn();
            if (Event.current.type == EventType.Repaint)
            {
                // https://answers.unity.com/questions/515197/how-to-use-guilayoututilitygetrect-properly.html
                applyFn(rect);
            }
        }
    }
}