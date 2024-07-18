using System;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Machination.Plugins.Machination.Notepad
{
    public class NotepadWindow : EditorWindow
    {
        private const string MenuDir = "Tools/Machination/";
        private const string DefaultFilePath = "Plugins/Machination/Notepad/NotepadText.txt";
        private string _text = "";
        private static string _filePath = DefaultFilePath;
        private DateTime _lastSaveTime;
        private bool _hasUnsavedChanges;
        
        [MenuItem(MenuDir + "Notepad")]
        public static void ShowWindow() { GetWindow<NotepadWindow>("Notepad"); }
        
        private void OnEnable()
        {
            LoadTextFromFile();
            LoadLastSaveTime();
            EditorApplication.quitting += OnEditorQuitting;
        }
        private void OnDisable()
        {
            EditorApplication.quitting -= OnEditorQuitting;
        }
        
        private void OnEditorQuitting()
        {
            CheckForUnsavedChanges();
        }
        
        private void OnDestroy()
        {
            CheckForUnsavedChanges();
        }
        
        private void CheckForUnsavedChanges()
        {
            if (_hasUnsavedChanges && EditorUtility.DisplayDialog("Unsaved Changes", "You have unsaved changes. Do you want to save before quitting?", "Yes", "No"))
            {
                SaveTextToFile();
            }
        }
        
        private void UpdateWindowTitle()
        {
            titleContent.text = "Notepad" + (_hasUnsavedChanges ? " *" : "");
        }

        private void OnGUI()
        {
            HandleShortcuts();
            
            var newText = EditorGUILayout.TextArea(_text, GUILayout.ExpandHeight(true));
            if (newText != _text)
            {
                _text = newText;
                _hasUnsavedChanges = true;
                UpdateWindowTitle();
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("File Path: Assets/");
            _filePath = EditorGUILayout.TextField(_filePath);
            GUILayout.EndHorizontal();
            
            GUILayout.Label("Last saved: " + (_lastSaveTime == default ? "Never" : _lastSaveTime.ToString(CultureInfo.InvariantCulture)));
        }

        private void HandleShortcuts()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown || (!e.control && !e.command) || e.keyCode != KeyCode.S) return;
            e.Use();
            SaveTextToFile();
        }

        private void SaveTextToFile()
        {
            try
            {
                File.WriteAllText(Path.Combine("Assets", _filePath), _text);
                AssetDatabase.Refresh();
                _lastSaveTime = DateTime.Now;
                EditorPrefs.SetString("NotepadLastSaveTime", _lastSaveTime.ToString(CultureInfo.InvariantCulture));
                _hasUnsavedChanges = false;
                UpdateWindowTitle();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to save Notepad: " + e.Message);
            }
        }

        private void LoadTextFromFile()
        {
            try
            {
                var fullPath = Path.Combine("Assets", _filePath);
                if (File.Exists(fullPath))
                {
                    _text = File.ReadAllText(fullPath);
                    _hasUnsavedChanges = false;
                    UpdateWindowTitle();
                }
                else
                {
                    Debug.LogWarning("Notepad file not found: Assets/" + _filePath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load Notepad: " + e.Message);
            }
        }

        private void LoadLastSaveTime()
        {
            var lastSaveTimeStr = EditorPrefs.GetString("NotepadLastSaveTime", "");
            if (!string.IsNullOrEmpty(lastSaveTimeStr))
            {
                _lastSaveTime = DateTime.Parse(lastSaveTimeStr);
            }
        }
    }
}