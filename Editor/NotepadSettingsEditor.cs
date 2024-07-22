#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Plugins.Machination.Notepad 
{
    public class NotepadSettingsEditor : EditorWindow
    {
        private NotepadSettings _notepadSettings;
        private string[] _fontOptions;
        private int _selectedFontIndex;

        [MenuItem("Tools/Machination/Notepad/Notepad Settings", false, 50)]
        public static void ShowWindow()
        {
            GetWindow<NotepadSettingsEditor>("Notepad Settings");
        }

        private void OnEnable()
        {
            LoadColorSettings();
            LoadFontSettings();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            GUILayout.Label("NotepadSettings Asset", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            _notepadSettings = (NotepadSettings)EditorGUILayout.ObjectField("Notepad Asset", _notepadSettings, typeof(NotepadSettings), false);

            if (!_notepadSettings)
            {
                GUILayout.Label("No NotepadSettings Asset Found");
                return;
            }
            
            EditorGUILayout.Space();
            GUILayout.Label("Colors", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            _notepadSettings.textColor = EditorGUILayout.ColorField("Text Color", _notepadSettings.textColor);
            _notepadSettings.backgroundColor = EditorGUILayout.ColorField("Background Color", _notepadSettings.backgroundColor);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            GUILayout.Label("Notepad Font", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            _notepadSettings.selectedFont = EditorGUILayout.TextField("Font Name", "CourierPrime");
            _notepadSettings.useCustomFont = EditorGUILayout.Toggle("Use Custom Font", _notepadSettings.useCustomFont);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            if (GUILayout.Button("Save", GUILayout.Height(40)))
            {
                EditorUtility.SetDirty(_notepadSettings);
                AssetDatabase.SaveAssets();
            }
        }

        private void LoadColorSettings()
        {
            if (_notepadSettings == null)
            {
                _notepadSettings = CreateInstance<NotepadSettings>();
                AssetDatabase.CreateAsset(_notepadSettings, NotepadConstants.ColorSettingsDir);
                AssetDatabase.SaveAssets();
            }
            _notepadSettings = AssetDatabase.LoadAssetAtPath<NotepadSettings>(NotepadConstants.ColorSettingsDir);
        }

        private void LoadFontSettings()
        {
            var fontPath = AssetDatabase.FindAssets("t:Font", new[] { "Assets/Plugins/Machination/Notepad/Fonts" })
                .Select(AssetDatabase.GUIDToAssetPath).ToArray();

            _fontOptions = fontPath.Select(System.IO.Path.GetFileNameWithoutExtension).ToArray();
            _selectedFontIndex = System.Array.IndexOf(_fontOptions, _notepadSettings.selectedFont);
            if (_selectedFontIndex == -1) _selectedFontIndex = 0;
        }
    }
}
#endif