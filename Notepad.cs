using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Plugins.Machination.Notepad
{
    public class Notepad : EditorWindow
    {
        private const string MenuDir = "Tools/Machination/Notepad/";
        private const string NotesFolder = "Plugins/Machination/Notepad/Notes";
        private string _text = "";
        private static string _filePath = "NewNote";
        private bool _hasUnsavedChanges;
        private string[] _files;
        private int _selectedFileIndex;
        private GUIStyle _textAreaStyle;
        private Font _customFont;
        private Vector2 _scrollPosition;
        private int _fontSize = 14;
        private string _fontSizeInput = "14";

        private static bool UseCustomFont
        {
            get => EditorPrefs.GetBool("UseCustomFont", true);
            set => EditorPrefs.SetBool("UseCustomFont", value);
        }

        #region Text
        private const string CustomFont = "Toggle Monospace Font";
        private const string UnsavedChanges = "Unsaved Changes";
        private const string UnsavedMessage = "You have unsaved changes. Do you want to save before creating a new file?";
        private const string UnsavedYes = "Yes";
        private const string UnsavedNo = "No";
        #endregion

        [MenuItem(MenuDir + "Open Notepad", false, 1)]
        public static void ShowWindow() { GetWindow<Notepad>("Notepad"); }

        [MenuItem(MenuDir + CustomFont, false, 51)]
        private static void ToggleCustomFont()
        {
            UseCustomFont = !UseCustomFont;
        }

        [MenuItem(MenuDir + CustomFont, true)]
        private static bool ToggleCustomFontValidate()
        {
            Menu.SetChecked(MenuDir + CustomFont, UseCustomFont);
            return true;
        }
        
        private void OnEnable()
        {
            LoadFiles();
            LoadTextFromFile();
            EditorApplication.quitting += OnEditorQuitting;
        }
        
        private void OnDisable() { EditorApplication.quitting -= OnEditorQuitting; }
        
        private void OnEditorQuitting() { CheckForUnsavedChanges(); }
        
        private void OnDestroy() { CheckForUnsavedChanges(); }
        
        private void CheckForUnsavedChanges()
        {
            if (_hasUnsavedChanges && EditorUtility.DisplayDialog(UnsavedChanges, UnsavedMessage, UnsavedYes, UnsavedNo))
            {
                SaveTextToFile();
            }
        }
        
        private void UpdateWindowTitle() { titleContent.text = "Notepad" + (_hasUnsavedChanges ? " *" : ""); }

        private void OnGUI()
        {
            HandleShortcuts();
            LoadCustomFont();
            
            EditorGUILayout.BeginHorizontal();
            RenderFileSelection();
            if (GUILayout.Button("Reload")) { LoadFiles(); }
            if (GUILayout.Button("New File")) { CheckForUnsavedChangesBeforeCreatingNewFile(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            RenderFontSizeInput();
            EditorGUILayout.EndHorizontal();
            
            RenderTextArea();
        }

        private void RenderTextArea()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            var newText = EditorGUILayout.TextArea(_text, _textAreaStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            
            if (newText == _text) return;
            _text = newText;
            _hasUnsavedChanges = true;
            UpdateWindowTitle();
        }

        private void RenderFileSelection()
        {
            GUILayout.Label("Select File:");
            var newSelectedFileIndex = EditorGUILayout.Popup(_selectedFileIndex, _files);
            if (newSelectedFileIndex != _selectedFileIndex)
            {
                HandleFileSelectionChange(newSelectedFileIndex);
            }
        }

        private void HandleFileSelectionChange(int newSelectedFileIndex)
        {
            if (_hasUnsavedChanges && EditorUtility.DisplayDialog(UnsavedChanges, UnsavedMessage, UnsavedYes, UnsavedNo))
            {
                SaveTextToFile();
            }

            _selectedFileIndex = newSelectedFileIndex;
            _filePath = _files[_selectedFileIndex];
            LoadTextFromFile();
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
                if (_files.Length <= 0) return;
                _selectedFileIndex = Array.IndexOf(_files, _filePath);
                if (_selectedFileIndex != -1) return;
                _selectedFileIndex = 0;
                _filePath = _files[_selectedFileIndex];
            }
            else
            {
                _files = Array.Empty<string>();
                Debug.LogWarning("Notes folder not found: " + notesFolderFullPath);
            }
        }

        private void CheckForUnsavedChangesBeforeCreatingNewFile()
        {
            if (_hasUnsavedChanges && EditorUtility.DisplayDialog(UnsavedChanges, UnsavedMessage, UnsavedYes, UnsavedNo))
            {
                SaveTextToFile();
            }
            CreateNewFile();
        }

        private void CreateNewFile()
        {
            var newFileName = EditorUtility.SaveFilePanel("Create New File", "Assets/" + NotesFolder, "NewNote", "txt");
            if (string.IsNullOrEmpty(newFileName)) return;
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

        private void LoadCustomFont()
        {
            if (UseCustomFont)
            {
                _customFont = (Font)AssetDatabase.LoadAssetAtPath("Assets/Plugins/Machination/Notepad/Fonts/CourierPrime.ttf", typeof(Font));
                if (_customFont)
                {
                    _textAreaStyle = new GUIStyle(GUI.skin.textArea)
                    {
                        font = _customFont, fontSize = _fontSize
                    };
                }
                else
                {
                    Debug.LogError("Failed to load Custom Font");
                    _textAreaStyle = new GUIStyle(GUI.skin.textArea);
                }
            }
            else
            {
                _textAreaStyle = new GUIStyle(GUI.skin.textArea);
            }
        }

        private void RenderFontSizeInput()
        {
            GUILayout.Label("Font Size:");
            _fontSize = EditorGUILayout.IntSlider(_fontSize, 10, 30);

            // Input Field Option
            //_fontSizeInput = GUILayout.TextField(_fontSizeInput, GUILayout.Width(40));

            // if (!int.TryParse(_fontSizeInput, out var newFontSize)) return;
            // _fontSize = Mathf.Clamp(newFontSize, 10, 30);
            // LoadCustomFont();
        }
    }
}
