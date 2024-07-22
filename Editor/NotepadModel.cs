using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Plugins.Machination.Notepad
{
    public interface IModelObserver
    {
        void ModelUpdated();
    }

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

                if (Files.Any())
                {
                    SelectedFileIndex = Files.FindIndex(x => x == FilePath);
                    if (SelectedFileIndex == -1)
                    {
                        SelectedFileIndex = 0;
                        FilePath = Files[SelectedFileIndex];
                    }
                }
                else
                {
                    Files = new List<string>();
                    SelectedFileIndex = -1;
                    FilePath = NotepadConstants.NewNoteDefaultName;
                }
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
                    GUI.FocusControl(null);
                    _view.ModelUpdated();
                    return;
                }
                Text = "";
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
                GUI.FocusControl(null);
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
            _view.ModelUpdated();
            EditorWindow.GetWindow<Notepad>().Repaint();
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
}
