using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Hai.ComboGestureDynamics.Scripts.Editor
{
    public class CgdParameterSearchWindow : SearchWindow
    {
        private static readonly MethodInfo OnGUIMethod;
        private static readonly MethodInfo InitMethod;
        private static readonly FieldInfo FilterWindowField;
        private static readonly FieldInfo LastClosedTimeField;

        static CgdParameterSearchWindow()
        {
            OnGUIMethod = typeof(SearchWindow).GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.NonPublic);
            InitMethod = typeof(SearchWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.NonPublic);
            FilterWindowField = typeof(SearchWindow).GetField("s_FilterWindow", BindingFlags.Static | BindingFlags.NonPublic);
            LastClosedTimeField = typeof(SearchWindow).GetField("s_LastClosedTime", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public new static bool Open<T>(SearchWindowContext context, T provider) where T : ScriptableObject, ISearchWindowProvider
        {
            UnityEngine.Object[] objectsOfTypeAll = Resources.FindObjectsOfTypeAll(typeof(SearchWindow));
            if (objectsOfTypeAll.Length > 0)
            {
                try
                {
                    ((EditorWindow) objectsOfTypeAll[0]).Close();
                    return false;
                }
                catch (Exception)
                {
                    FilterWindowField.SetValue(null, null);
                }
            }

            if (DateTime.Now.Ticks / 10000L < (long) LastClosedTimeField.GetValue(null) + 50L)
            {
                return false;
            }

            SearchWindow window = (SearchWindow) FilterWindowField.GetValue(null);
            if (window == null)
            {
                window = CreateInstance<CgdParameterSearchWindow>();
                window.hideFlags = HideFlags.HideAndDontSave;
                FilterWindowField.SetValue(null, window);
            }

            InitMethod.Invoke(window, new object[] {context, provider});
            return true;
        }

        void OnGUI()
        {
            OnGUIMethod.Invoke(this, null);
        }
    }
}