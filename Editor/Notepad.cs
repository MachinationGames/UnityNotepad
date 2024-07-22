using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Plugins.Machination.Notepad
{
    /// <summary>
    /// Simplest of Observer pattern Interfaces for the simplest of use cases.
    /// 
    /// Utilized by NotepadModel to notify Notepad that state has changed.
    /// 
    /// Todo: Replace this with events, proper Rx, or remove it.
    /// </summary>
    public interface IModelObserver
    {
        /**
         */
        void ModelUpdated() { }
    }
    /// <summary>
    /// Notepad Model handles are business logic for Notepad such as managing
    /// Text, current file, file list, Loading files, saving etc.
    /// </summary>
    public class NotepadModel 
    {
        public string Text { get; private set; } = "";

        private string FilePath { get; set; } = NotepadConstants.NewNoteDefaultName;

        public bool HasUnsavedChanges { get; private set; }

        public List<string> Files { get; private set; } = new();

        public int SelectedFileIndex { get; private set; }

        private readonly IModelObserver _view;
        public NotepadModel(IModelObserver view)
        {
            _view = view;
        }

        public void Init()
        {
            LoadFiles();
            LoadTextFromFile();
        }

        public void SelectFileFromList(int index)
        {
            // do nothing if it was already selected
            if(SelectedFileIndex == index) return;

            if (HasUnsavedChanges && EditorUtility.DisplayDialog(NotepadConstants.UnsavedChanges, NotepadConstants.UnsavedMessage, NotepadConstants.UnsavedYes, NotepadConstants.UnsavedNo))
            {
                SaveTextToFile();
            }

            SelectedFileIndex = index;
            FilePath = Files[SelectedFileIndex];
            LoadTextFromFile();
        }

        public void UpdateTextIfChanged(string text)
        {
            var different = Text != text;
            if(!different) return;
            Text = text;
            HasUnsavedChanges = true;
            _view.ModelUpdated();
        }
        public void LoadFiles()
        {
            var notesFolderFullPath = Path.Combine(NotepadConstants.NotepadFolder, "Notes");
            if (Directory.Exists(notesFolderFullPath))
            {
                Files = Directory.GetFiles(notesFolderFullPath)
                                  .Where(file => !file.EndsWith(".meta"))
                                  .Select(Path.GetFileName)
                                  .ToList();
                if (!Files.Any()) return;
                SelectedFileIndex = Files.FindIndex(x => x == FilePath);
                if (SelectedFileIndex != -1) return;
                SelectedFileIndex = 0;
                FilePath = Files[SelectedFileIndex];
                _view.ModelUpdated();
            }
            else
            {
                Files = new List<string>();
                Debug.LogWarning(NotepadConstants.NotesFolderNotFound + notesFolderFullPath);
            }
        }
        public void SaveTextToFile()
        {
            try
            {
                var fullPath = Path.Combine(NotepadConstants.NotepadFolder + "/Notes", FilePath);
                File.WriteAllText(fullPath, Text);
                AssetDatabase.Refresh();
                HasUnsavedChanges = false;
                _view.ModelUpdated();
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
                var fullPath = Path.Combine(NotepadConstants.NotepadFolder, "Notes", FilePath);
                if (File.Exists(fullPath))
                {
                    Text = File.ReadAllText(fullPath);
                    HasUnsavedChanges = false;
                    _view.ModelUpdated();
                    return;
                }
                Debug.LogWarning(NotepadConstants.FileNotFound + fullPath);
            }
            catch (Exception e)
            {
                Debug.LogError(NotepadConstants.LoadError + e.Message);
            }
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
                SelectedFileIndex = Files.FindIndex(name => name == newFileName);
                FilePath = newFileName;
                Text = "";
                HasUnsavedChanges = false;
                _view.ModelUpdated();
            }
            else
            {
                EditorUtility.DisplayDialog(NotepadConstants.FileExistsTitle, NotepadConstants.FileExistsMessage, NotepadConstants.UnsavedNo);
            }
        }
        
        public void CheckForUnsavedChangesBeforeCreatingNewFile()
        {
            if (HasUnsavedChanges && EditorUtility.DisplayDialog(NotepadConstants.UnsavedChanges, NotepadConstants.UnsavedNewFileMessage, NotepadConstants.UnsavedYes, NotepadConstants.UnsavedNo))
            {
                SaveTextToFile();
            }
            CreateNewFile();
        }
        
        public void CheckForUnsavedChanges()
        {
            if (HasUnsavedChanges && EditorUtility.DisplayDialog(NotepadConstants.UnsavedChanges, NotepadConstants.UnsavedMessage, NotepadConstants.UnsavedYes, NotepadConstants.UnsavedNo))
            {
                SaveTextToFile();
            }
        }
        
        public void OnDestroy()
        {
            CheckForUnsavedChanges();           
        }
    }

    /// <summary>
    /// Notepad. 
    ///  
    /// Directly Interfaces in Unity. Tries to only handle UI logic while pushing all logic to NotepadModel
    /// </summary>
    public class Notepad : EditorWindow, IModelObserver
    {
        #region Fields

        private NotepadModel _model;
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
            if(_model == null) {
                _model = new NotepadModel(this);
                _model.Init();
            }

            LoadTextures();
            EditorApplication.quitting += OnEditorQuitting;
        }
        
        private void OnDisable() { EditorApplication.quitting -= OnEditorQuitting; }


        private void OnDestroy() {  _model.OnDestroy(); }

        private void OnGUI()
        {
            _model ??= new NotepadModel(this);

            HandleShortcuts();
            LoadCustomFont();
            SetupStyles();
            
            EditorGUILayout.BeginHorizontal();
            RenderFileSelection();
            if (GUILayout.Button(new GUIContent(_reloadButtonTexture, "Reload files"), _buttonStyle)) { _model.LoadFiles(); }
            if (GUILayout.Button(new GUIContent(_newFileButtonTexture, "Create new file"), _buttonStyle)) { _model.CheckForUnsavedChangesBeforeCreatingNewFile(); }
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
            _model.CheckForUnsavedChanges();
        }


        public void ModelUpdated()
        {
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle() 
        {
            titleContent.text = NotepadConstants.NotepadTitle + (_model.HasUnsavedChanges ? " *" : ""); 
        }

        private void RenderTextArea()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            var newText = EditorGUILayout.TextArea(_model.Text, _textAreaStyle, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            _model.UpdateTextIfChanged(newText);
        }

        private void RenderFileSelection()
        {
            GUILayout.Label(NotepadConstants.SelectFile);
            var newSelectedFileIndex = EditorGUILayout.Popup(_model.SelectedFileIndex, _model.Files.AsEnumerable().ToArray());
            _model.SelectFileFromList(newSelectedFileIndex);
        }

        private void HandleShortcuts()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown || (!e.control && !e.command) || e.keyCode != KeyCode.S) return;
            e.Use();
            _model.SaveTextToFile();
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
            _newFileButtonTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(NotepadConstants.NotepadFolder + "/Resources/newFile.png", typeof(Texture2D));
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
        }
        #endregion
    }
}
