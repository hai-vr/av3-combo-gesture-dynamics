using Hai.ComboGestureDynamics.Scripts.Components;
using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    internal class CgdEditorPermutationsLayout
    {
        private readonly CgdEditor _cgdEditor;
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
        private Rect m_focusAreaRect;

        private const int PermutationWidth = 100;
        private const int PermutationHeight = 90;

        public CgdEditorPermutationsLayout(CgdEditor cgdEditor)
        {
            _cgdEditor = cgdEditor;
        }

        public void Layout()
        {
            EditorGUILayout.BeginVertical("GroupBox");
            CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 7, EditorGUIUtility.singleLineHeight * 7), rect => m_focusAreaRect = rect);
            GUILayout.BeginArea(m_focusAreaRect);
            EditorGUILayout.BeginHorizontal();
            if (_anySelectedPermutation)
            {
                var leftSide = _selectedPermutation.IsLeft() ? _selectedPermutation : _selectedPermutation.Mirror();
                var rightSide = leftSide.Mirror();

                CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth, PermutationHeight), rect => m_focusPermutationSelectedRect = rect);
                GUILayout.BeginArea(m_focusPermutationSelectedRect);
                DrawPermutationBox(leftSide, true);
                GUILayout.EndArea();

                if (!_selectedPermutation.IsSymmetrical())
                {
                    CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(PermutationWidth, PermutationHeight), rect => m_focusPermutationMirrorRect = rect);
                    GUILayout.BeginArea(m_focusPermutationMirrorRect);
                    DrawPermutationBox(rightSide, true);
                    GUILayout.EndArea();
                }

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
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

        private void SelectPermutation(Cgd.HandGesture.HandPose leftPose, Cgd.HandGesture.HandPose rightPose)
        {
            _anySelectedPermutation = true;
            _selectedPermutation = new CgdEdi.Permutation(leftPose, rightPose);
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