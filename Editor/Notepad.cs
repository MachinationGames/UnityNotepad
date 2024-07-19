using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Plugins.Machination.Notepad
{
    public class Notepad : EditorWindow
    {
        #region Fields
        private string _text = "";
        private static string _filePath = NotepadConstants.NewNoteDefaultName;
        private bool _hasUnsavedChanges;
        private string[] _files;
        private int _selectedFileIndex;
        private GUIStyle _textAreaStyle;
        private Font _customFont;
        private Vector2 _scrollPosition;
        private int _fontSize = 14;
        //private string _fontSizeInput = "14"; //Used for unused FontSize Input Field
        private Texture2D _reloadButtonTexture;
        private Texture2D _newFileButtonTexture;
        private GUIStyle _buttonStyle;
        #endregion

        #region Properties
        private static bool UseCustomFont
        {
            get => EditorPrefs.GetBool("UseCustomFont", true);
            set => EditorPrefs.SetBool("UseCustomFont", value);
        }
        #endregion

        #region MenuItems
        [MenuItem(NotepadConstants.MenuDir + "Open Notepad", false, 1)]
        public static void ShowWindow() 
        {
            var window = GetWindow<Notepad>(NotepadConstants.NotepadTitle);
            var icon = (Texture2D)AssetDatabase.LoadAssetAtPath(NotepadConstants.NotepadFolder + "/Resources/icon.png", typeof(Texture2D));
            window.titleContent = icon != null ? new GUIContent(NotepadConstants.NotepadTitle, icon) : new GUIContent(NotepadConstants.NotepadTitle);
        }

        [MenuItem(NotepadConstants.MenuDir + NotepadConstants.CustomFont, false, 51)]
        private static void ToggleCustomFont()
        {
            UseCustomFont = !UseCustomFont;
        }

        [MenuItem(NotepadConstants.MenuDir + NotepadConstants.CustomFont, true)]
        private static bool ToggleCustomFontValidate()
        {
            Menu.SetChecked(NotepadConstants.MenuDir + NotepadConstants.CustomFont, UseCustomFont);
            return true;
        }
        #endregion

        #region Unity Methods
        private void OnEnable()
        {
            LoadFiles();
            LoadTextFromFile();
            LoadTextures();
            EditorApplication.quitting += OnEditorQuitting;
        }
        
        private void OnDisable() { EditorApplication.quitting -= OnEditorQuitting; }

        private void OnDestroy() { CheckForUnsavedChanges(); }

        private void OnGUI()
        {
            HandleShortcuts();
            LoadCustomFont();
            SetupStyles();
            
            EditorGUILayout.BeginHorizontal();
            RenderFileSelection();
            if (GUILayout.Button(new GUIContent(_reloadButtonTexture, "Reload files"), _buttonStyle)) { LoadFiles(); }
            if (GUILayout.Button(new GUIContent(_newFileButtonTexture, "Create new file"), _buttonStyle)) { CheckForUnsavedChangesBeforeCreatingNewFile(); }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            RenderFontSizeInput();
            EditorGUILayout.EndHorizontal();
            
            RenderTextArea();
        }
        #endregion

        #region Methods
        private void OnEditorQuitting() 
        {
            CheckForUnsavedChanges();
        }

        private void CheckForUnsavedChanges()
        {
            if (_hasUnsavedChanges && EditorUtility.DisplayDialog(NotepadConstants.UnsavedChanges, NotepadConstants.UnsavedMessage, NotepadConstants.UnsavedYes, NotepadConstants.UnsavedNo))
            {
                SaveTextToFile();
            }
        }

        private void UpdateWindowTitle() 
        {
            titleContent.text = NotepadConstants.NotepadTitle + (_hasUnsavedChanges ? " *" : ""); 
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
            GUILayout.Label(NotepadConstants.SelectFile);
            var newSelectedFileIndex = EditorGUILayout.Popup(_selectedFileIndex, _files);
            if (newSelectedFileIndex != _selectedFileIndex)
            {
                HandleFileSelectionChange(newSelectedFileIndex);
            }
        }

        private void HandleFileSelectionChange(int newSelectedFileIndex)
        {
            if (_hasUnsavedChanges && EditorUtility.DisplayDialog(NotepadConstants.UnsavedChanges, NotepadConstants.UnsavedMessage, NotepadConstants.UnsavedYes, NotepadConstants.UnsavedNo))
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
                var fullPath = Path.Combine(NotepadConstants.NotepadFolder + "/Notes", _filePath);
                File.WriteAllText(fullPath, _text);
                AssetDatabase.Refresh();
                _hasUnsavedChanges = false;
                UpdateWindowTitle();
            }
            catch (Exception e)
            {
                Debug.LogError(NotepadConstants.SaveError + e.Message);
            }
        }

        private void LoadTextFromFile()
        {
            try
            {
                var fullPath = Path.Combine(NotepadConstants.NotepadFolder, "Notes", _filePath);
                if (File.Exists(fullPath))
                {
                    _text = File.ReadAllText(fullPath);
                    _hasUnsavedChanges = false;
                    UpdateWindowTitle();
                }
                else
                {
                    Debug.LogWarning(NotepadConstants.FileNotFound + fullPath);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(NotepadConstants.LoadError + e.Message);
            }
        }

        private void LoadFiles()
        {
            var notesFolderFullPath = Path.Combine(NotepadConstants.NotepadFolder, "Notes");
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
                Debug.LogWarning(NotepadConstants.NotesFolderNotFound + notesFolderFullPath);
            }
        }

        private void CheckForUnsavedChangesBeforeCreatingNewFile()
        {
            if (_hasUnsavedChanges && EditorUtility.DisplayDialog(NotepadConstants.UnsavedChanges, NotepadConstants.UnsavedMessage, NotepadConstants.UnsavedYes, NotepadConstants.UnsavedNo))
            {
                SaveTextToFile();
            }
            CreateNewFile();
        }

        private void CreateNewFile()
        {
            var newFileName = EditorUtility.SaveFilePanel(NotepadConstants.CreateNewFileDialog, NotepadConstants.NotepadFolder + "/Notes", NotepadConstants.NewNoteDefaultName, "txt");
            if (string.IsNullOrEmpty(newFileName)) return;
            newFileName = Path.GetFileName(newFileName);
            var fullPath = Path.Combine(NotepadConstants.NotepadFolder + "/Notes", newFileName);
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
                EditorUtility.DisplayDialog(NotepadConstants.FileExistsTitle, NotepadConstants.FileExistsMessage, NotepadConstants.UnsavedNo);
            }
        }

        private void LoadCustomFont()
        {
            if (UseCustomFont)
            {
                _customFont = (Font)AssetDatabase.LoadAssetAtPath(NotepadConstants.NotepadFolder + "/Fonts/CourierPrime.ttf", typeof(Font));
                if (_customFont)
                {
                    _textAreaStyle = new GUIStyle(GUI.skin.textArea)
                    {
                        font = _customFont, fontSize = _fontSize
                    };
                }
                else
                {
                    Debug.LogError(NotepadConstants.FontLoadError);
                    _textAreaStyle = new GUIStyle(GUI.skin.textArea);
                }
            }
            else
            {
                _textAreaStyle = new GUIStyle(GUI.skin.textArea);
            }
        }
        
        private void LoadTextures()
        {
            _reloadButtonTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(NotepadConstants.NotepadFolder + "/Resources/reload.png", typeof(Texture2D));
            _newFileButtonTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(NotepadConstants.NotepadFolder + "/Resources/newfile.png", typeof(Texture2D));
        }
        
        private void SetupStyles()
        {
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fixedWidth = 24,
                fixedHeight = 24,
                padding = new RectOffset(0, 0, 0, 0)
            };
        }

        private void RenderFontSizeInput()
        {
            GUILayout.Label(NotepadConstants.FontSizeLabel);
            _fontSize = EditorGUILayout.IntSlider(_fontSize, 10, 30);

            // Input Field Option
            //_fontSizeInput = GUILayout.TextField(_fontSizeInput, GUILayout.Width(40));

            // if (!int.TryParse(_fontSizeInput, out var newFontSize)) return;
            // _fontSize = Mathf.Clamp(newFontSize, 10, 30);
            // LoadCustomFont();
        }
        #endregion
    }
}
