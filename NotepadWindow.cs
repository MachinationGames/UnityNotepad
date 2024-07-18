using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Plugins.Machination.Notepad
{
    public class NotepadWindow : EditorWindow
    {
        private const string MenuDir = "Tools/Machination/";
        private const string NotesFolder = "Plugins/Machination/Notepad/Notes";
        private string _text = "";
        private static string _filePath = "Note.txt";
        private bool _hasUnsavedChanges;
        private string[] _files;
        private int _selectedFileIndex;

        [MenuItem(MenuDir + "Notepad")]
        public static void ShowWindow() { GetWindow<NotepadWindow>("Notepad"); }
        
        private void OnEnable()
        {
            LoadFiles();
            LoadTextFromFile();
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
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Select File:");
            int newSelectedFileIndex = EditorGUILayout.Popup(_selectedFileIndex, _files);
            if (newSelectedFileIndex != _selectedFileIndex)
            {
                if (_hasUnsavedChanges && EditorUtility.DisplayDialog("Unsaved Changes", "You have unsaved changes. Do you want to save before switching files?", "Yes", "No"))
                {
                    SaveTextToFile();
                }

                _selectedFileIndex = newSelectedFileIndex;
                _filePath = _files[_selectedFileIndex];
                LoadTextFromFile();
            }
            if (GUILayout.Button("Reload"))
            {
                LoadFiles();
            }
            if (GUILayout.Button("New File"))
            {
                CreateNewFile();
            }
            EditorGUILayout.EndHorizontal();

            var newText = EditorGUILayout.TextArea(_text, GUILayout.ExpandHeight(true));
            if (newText != _text)
            {
                _text = newText;
                _hasUnsavedChanges = true;
                UpdateWindowTitle();
            }
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
                var fullPath = Path.Combine("Assets", NotesFolder, _filePath);
                File.WriteAllText(fullPath, _text);
                AssetDatabase.Refresh();
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
                var fullPath = Path.Combine("Assets", NotesFolder, _filePath);
                if (File.Exists(fullPath))
                {
                    _text = File.ReadAllText(fullPath);
                    _hasUnsavedChanges = false;
                    UpdateWindowTitle();
                }
                else
                {
                    Debug.LogWarning("Notepad file not found: " + fullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load Notepad: " + e.Message);
            }
        }

        private void LoadFiles()
        {
            var notesFolderFullPath = Path.Combine("Assets", NotesFolder);
            if (Directory.Exists(notesFolderFullPath))
            {
                _files = Directory.GetFiles(notesFolderFullPath)
                                  .Where(file => !file.EndsWith(".meta"))
                                  .Select(Path.GetFileName)
                                  .ToArray();
                if (_files.Length > 0)
                {
                    _selectedFileIndex = Array.IndexOf(_files, _filePath);
                    if (_selectedFileIndex == -1)
                    {
                        _selectedFileIndex = 0;
                        _filePath = _files[_selectedFileIndex];
                    }
                }
            }
            else
            {
                _files = new string[0];
                Debug.LogWarning("Notes folder not found: " + notesFolderFullPath);
            }
        }

        private void CreateNewFile()
        {
            string newFileName = EditorUtility.SaveFilePanel("Create New File", "Assets/" + NotesFolder, "NewNote", "txt");
            if (!string.IsNullOrEmpty(newFileName))
            {
                newFileName = Path.GetFileName(newFileName);
                var fullPath = Path.Combine("Assets", NotesFolder, newFileName);
                if (!File.Exists(fullPath))
                {
                    File.WriteAllText(fullPath, "");
                    AssetDatabase.Refresh();
                    LoadFiles();
                    _selectedFileIndex = Array.IndexOf(_files, newFileName);
                    _filePath = newFileName;
                    _text = "";
                    _hasUnsavedChanges = false;
                    UpdateWindowTitle();
                }
                else
                {
                    EditorUtility.DisplayDialog("File Exists", "A file with that name already exists. Please choose a different name.", "OK");
                }
            }
        }
    }
}
