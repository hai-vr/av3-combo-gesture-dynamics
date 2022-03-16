using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    internal class CgdEditorPermutationsLayout
    {
        private readonly CgdEditor _cgdEditor;
        private Rect m_focusAreaLeftRect;
        private Rect m_focusAreaRightRect;

        private static Color LeftSideBg => EditorGUIUtility.isProSkin ? new Color(0.62f, 0.4f, 0.12f) : new Color(1f, 0.81f, 0.59f);
        private static Color RightSideBg => EditorGUIUtility.isProSkin ? new Color(0.24f, 0.48f, 0.62f) : new Color(0.7f, 0.9f, 1f);
        private static Color NeutralSideBg => EditorGUIUtility.isProSkin ? new Color(0.07f, 0.07f, 0.07f) : new Color(0.88f, 0.88f, 0.88f);
        private static Color LeftSymmetricalBg => EditorGUIUtility.isProSkin ? new Color(0.34f, 0.27f, 0.23f) : new Color(0.7f, 0.65f, 0.59f);
        private static Color RightSymmetricalBg => EditorGUIUtility.isProSkin ? new Color(0.23f, 0.26f, 0.34f) : new Color(0.56f, 0.64f, 0.7f);
        private static Color InconsistentBg => EditorGUIUtility.isProSkin ? new Color(0.72f, 0.09f, 0.27f) : new Color(1f, 0.41f, 0.54f);
        private static Color HighlightMain => EditorGUIUtility.isProSkin ? new Color(0.61f, 0.61f, 0.61f) : Color.white;
        private static Color HighlightSecondary => EditorGUIUtility.isProSkin ? new Color(0.43f, 0.43f, 0.43f) : new Color(0.82f, 0.82f, 0.82f);
        private bool _anySelectedPermutation;
        private CgdEdi.Permutation _selectedPermutation;
        private Rect m_permutationAreaRect;
        private Rect m_focusPermutationSelectedRect;
        private Rect m_focusPermutationMirrorRect;
        private bool _mirrorHand;
        private CgdPermutationRuleset _selectedRulesetNullable;
        private int _tool;

        private const int PermutationWidth = 100;
        private const int PermutationHeight = 90;

        public CgdEditorPermutationsLayout(CgdEditor cgdEditor)
        {
            _cgdEditor = cgdEditor;
        }

        public void Layout()
        {
            InitIfApplicable();

            EditorGUILayout.BeginVertical("GroupBox");
            CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 10, EditorGUIUtility.singleLineHeight * 10), rect => m_focusAreaLeftRect = rect);
            GUILayout.BeginArea(m_focusAreaLeftRect);
            if (_anySelectedPermutation)
            {
                EditorGUILayout.BeginHorizontal();
                var leftSide = _selectedPermutation.IsLeft() ? _selectedPermutation : _selectedPermutation.Mirror();
                var rightSide = leftSide.Mirror();
                var isBothFists = _selectedPermutation.IsSymmetrical() && _selectedPermutation.left == Cgd.HandGesture.HandPose.Fist;

                var ruleset = new SerializedObject(_selectedRulesetNullable);
                var permutationEffects = ruleset.FindProperty(nameof(CgdPermutationRuleset.permutationEffectBehaviours));
                var elementSelected = permutationEffects.GetArrayElementAtIndex(_selectedPermutation.ToPermutationEffectBehavioursArrayIndex());
                var elementNotSelected = permutationEffects.GetArrayElementAtIndex(_selectedPermutation.Mirror().ToPermutationEffectBehavioursArrayIndex());
                var elementLeftSide = permutationEffects.GetArrayElementAtIndex(leftSide.ToPermutationEffectBehavioursArrayIndex());
                var elementRightSide = permutationEffects.GetArrayElementAtIndex(rightSide.ToPermutationEffectBehavioursArrayIndex());
                var sameAnimation = elementLeftSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expression)).objectReferenceValue == elementRightSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expression)).objectReferenceValue;

                if (!isBothFists)
                {
                    if (sameAnimation && _mirrorHand)
                    {
                        CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth * 2, PermutationHeight), rect => m_focusPermutationSelectedRect = rect);
                        GUILayout.BeginArea(new Rect(m_focusPermutationSelectedRect.x, m_focusPermutationSelectedRect.y, PermutationWidth, m_focusPermutationSelectedRect.height));
                        DrawPermutationBox(leftSide, true);
                        GUILayout.EndArea();

                        if (!_selectedPermutation.IsSymmetrical())
                        {
                            // CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth, PermutationHeight), rect => m_focusPermutationMirrorRect = rect);
                            GUILayout.BeginArea(new Rect(m_focusPermutationSelectedRect.x + PermutationWidth, m_focusPermutationSelectedRect.y, PermutationWidth, m_focusPermutationSelectedRect.height));
                            DrawPermutationBox(rightSide, true);
                            GUILayout.EndArea();
                        }
                    }
                    else
                    {
                        CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth * 2, PermutationHeight), rect => m_focusPermutationSelectedRect = rect);
                        GUILayout.BeginArea(new Rect(m_focusPermutationSelectedRect.x, m_focusPermutationSelectedRect.y, PermutationWidth, m_focusPermutationSelectedRect.height));
                        DrawPermutationBox(_selectedPermutation, true);
                        GUILayout.EndArea();
                    }
                }
                else
                {
                    CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth * 3, PermutationHeight), rect => m_focusPermutationSelectedRect = rect);
                    GUILayout.BeginArea(new Rect(m_focusPermutationSelectedRect.x, m_focusPermutationSelectedRect.y, PermutationWidth, m_focusPermutationSelectedRect.height));
                    DrawPermutationBox(_selectedPermutation, true);
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(m_focusPermutationSelectedRect.x + PermutationWidth, m_focusPermutationSelectedRect.y, PermutationWidth, m_focusPermutationSelectedRect.height));
                    DrawPermutationFist(true);
                    GUILayout.EndArea();
                    GUILayout.BeginArea(new Rect(m_focusPermutationSelectedRect.x + PermutationWidth * 2, m_focusPermutationSelectedRect.y, PermutationWidth, m_focusPermutationSelectedRect.height));
                    DrawPermutationFist(false);
                    GUILayout.EndArea();
                }


                EditorGUILayout.BeginVertical("GroupBox");
                if (_cgdEditor.cgd.uiComplexity != Cgd.UiComplexity.Simple)
                {
                    _tool = GUILayout.Toolbar(_tool, new[]
                    {
                        CgdLocalization.Localize(CgdLocalization.Phrase.Animations),
                        CgdLocalization.Localize(CgdLocalization.Phrase.Tweening)
                    });
                }
                else
                {
                    _tool = 0;
                }
                if (_tool == 0)
                {
                    if (permutationEffects.arraySize > 0)
                    {
                        if (isBothFists)
                        {
                            EditorGUILayout.PropertyField(elementLeftSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expression)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.BothFistsAnimation)));
                            EditorGUILayout.PropertyField(elementLeftSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expressionFistLeft)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.LeftFistAnimation)));
                            EditorGUILayout.PropertyField(elementLeftSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expressionFistRight)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.RightFistAnimation)));
                        }
                        else if (sameAnimation && _mirrorHand)
                        {
                            // EditorGUILayout.PropertyField(elementLeftSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.effect)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Animation)));
                            var leftEffect = elementLeftSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expression));
                            var rightEffect = elementRightSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expression));

                            var previousObject = leftEffect.objectReferenceValue;
                            var newObject = EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Effect)), previousObject, typeof(Motion), true);
                            if (newObject != previousObject)
                            {
                                leftEffect.objectReferenceValue = newObject;
                                rightEffect.objectReferenceValue = newObject;
                            }
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(elementSelected.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expression)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Effect)));
                            var newMotion = EditorGUILayout.ObjectField(new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.Animation)), null, typeof(Motion)) as Motion; // FIXME this no longer needs to be a classic object field
                            if (newMotion != null)
                            {
                                elementSelected.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expression)).objectReferenceValue = newMotion;
                            }
                        }

                        if (!_selectedPermutation.IsSymmetrical())
                        {
                            if (!sameAnimation)
                            {
                                if (_cgdEditor.cgd.uiComplexity != Cgd.UiComplexity.Simple)
                                {
                                    EditorGUI.BeginDisabledGroup(true);
                                    EditorGUILayout.Toggle(CgdLocalization.Localize(CgdLocalization.Phrase.MirrorHand), false);
                                    EditorGUILayout.PropertyField(elementNotSelected.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.expression)), new GUIContent(CgdLocalization.Localize(CgdLocalization.Phrase.MirrorHandAnimation)));
                                    EditorGUI.EndDisabledGroup();
                                }
                            }
                            else
                            {
                                _mirrorHand = EditorGUILayout.Toggle(CgdLocalization.Localize(CgdLocalization.Phrase.MirrorHand), _mirrorHand);
                            }
                        }
                    }
                }
                else
                {
                    if (permutationEffects.arraySize > 0)
                    {
                        var tweeningType = elementLeftSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.tweeningType));
                        var tweening = elementLeftSide.FindPropertyRelative(nameof(Cgd.PermutationEffectBehaviour.tweening));
                        CgdEditorUiExtensions.TweeningBox(tweeningType, tweening);
                    }
                }
                ruleset.ApplyModifiedProperties();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndArea();

            EditorGUILayout.EndVertical();

            GUILayout.BeginVertical("GroupBox");
            EditorGUILayout.LabelField(CgdLocalization.Localize(CgdLocalization.Phrase.Permutations, "dummy"), EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth * 8, PermutationHeight * 8), rect => m_permutationAreaRect = rect);
            GUILayout.BeginArea(m_permutationAreaRect);
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

        private void InitIfApplicable()
        {
            if (_selectedRulesetNullable != null) return;

            var rulesets = CgdEditorUiExtensions.FindPermutationRulesets(_cgdEditor.cgd);
            if (rulesets.Length == 1)
            {
                _selectedRulesetNullable = rulesets[0];
            }
        }

        private void DrawPermutationBox(CgdEdi.Permutation permutation, bool bypassSelection = false)
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
                DrawColoredBackground(permutation.left == permutation.right ? NeutralSideBg : (int)permutation.left > (int)permutation.right ? LeftSymmetricalBg : RightSymmetricalBg);
            }

            GUILayout.BeginVertical();

            EditorGUILayout.LabelField(new CgdEdi.Permutation(permutation.left, permutation.right).ToLocalizedName());

            var effect = _selectedRulesetNullable.permutationEffectBehaviours[permutation.ToPermutationEffectBehavioursArrayIndex()].expression;

            // FIXME RequireRender may be passed a blendtree, which will fail to render
            var renderable = (AnimationClip) effect;
            var button = effect != null
                ? GUILayout.Button(_cgdEditor.renderQueue.RequireRender(renderable), GUIStyle.none, GUILayout.Width(PermutationWidth), GUILayout.Height(PermutationHeight - EditorGUIUtility.singleLineHeight * 2))
                : GUILayout.Button(GUIContent.none, GUIStyle.none, GUILayout.Width(PermutationWidth), GUILayout.Height(PermutationHeight - EditorGUIUtility.singleLineHeight * 2));
            if (button)
            {
                SelectPermutation(permutation.left, permutation.right);
            }
            GUILayout.EndVertical();
        }

        private void DrawPermutationFist(bool isLeftFist)
        {
            DrawColoredBackground(isLeftFist ? LeftSymmetricalBg : RightSymmetricalBg);

            GUILayout.BeginVertical();

            EditorGUILayout.LabelField(CgdLocalization.Localize(isLeftFist ? CgdLocalization.Phrase.LeftFist : CgdLocalization.Phrase.RightFist));
            GUILayout.EndVertical();
        }

        private void SelectPermutation(Cgd.HandGesture.HandPose leftPose, Cgd.HandGesture.HandPose rightPose)
        {
            _anySelectedPermutation = true;
            _selectedPermutation = new CgdEdi.Permutation(leftPose, rightPose);

            _mirrorHand = true;
        }

        private void DrawColoredBackground(Color color)
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
    }
}