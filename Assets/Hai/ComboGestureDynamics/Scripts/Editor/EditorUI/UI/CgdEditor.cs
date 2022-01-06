using UnityEditor;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor.EditorUI.UI
{
    public class CgdEditor : EditorWindow
    {
        private Vector2 _scrollPos;
        private int _focus;

        public Components.ComboGestureDynamics cgd;

        private CgdEditorConfigurationLayout _cgdEditorConfigurationLayout;
        private CgdEditorPermutationsLayout _cgdEditorPermutationsLayout;
        private CgdEditorRulesLayout _cgdEditorRulesLayout;
        private CgdEditorPartsLayout _cgdEditorPartsLayout;

        [MenuItem("Window/Haï/ComboGestureDynamics UI")]
        public static void ShowWindow()
        {
            var window = GetWindow<CgdEditor>();
            window.titleContent = new GUIContent("ComboGestureDynamics");
        }

        private void OnEnable()
        {
            _cgdEditorConfigurationLayout = new CgdEditorConfigurationLayout(this);
            _cgdEditorPermutationsLayout = new CgdEditorPermutationsLayout(this);
            _cgdEditorRulesLayout = new CgdEditorRulesLayout(this);
            _cgdEditorPartsLayout = new CgdEditorPartsLayout(this);
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
            this.cgd = cgd;
        }

        private void OnGUI()
        {
            // _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height));
            //

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

            if (cgd != null)
            {
                switch (_focus)
                {
                    case 0: _cgdEditorConfigurationLayout.Layout(); break;
                    case 2: _cgdEditorPermutationsLayout.Layout(); break;
                    case 3: _cgdEditorRulesLayout.Layout(); break;
                    case 4: _cgdEditorPartsLayout.Layout(); break;
                }
            }

            //
            // GUILayout.EndScrollView();
        }
    }

    internal class CgdEditorDummyLayout
    {
        private readonly CgdEditor _cgdEditor;
        private Rect m_focusAreaRect;

        public CgdEditorDummyLayout(CgdEditor cgdEditor)
        {
            _cgdEditor = cgdEditor;
        }

        public void Layout()
        {
            EditorGUILayout.BeginVertical("GroupBox");
            CgdEditorUiExtensions.RectOnRepaint(() => GUILayoutUtility.GetRect(100, float.MaxValue, EditorGUIUtility.singleLineHeight * 7, EditorGUIUtility.singleLineHeight * 7), rect => m_focusAreaRect = rect);
            GUILayout.BeginArea(m_focusAreaRect);
            EditorGUILayout.BeginHorizontal();



            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
            EditorGUILayout.EndVertical();
        }
    }
}