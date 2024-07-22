#if UNITY_EDITOR
namespace Plugins.Machination.Notepad
{
    public static class NotepadConstants
    {
        // Menu Directory
        public const string MenuDir = "Tools/Machination/Notepad/";

        // Notepad Directory
        public const string NotepadFolder = "Assets/Plugins/Machination/Notepad";

        // Menu Items
        public const string CustomFont = "Toggle Monospace Font";

        // Dialogs
        public const string UnsavedChanges = "Unsaved Changes";
        public const string UnsavedMessage = "You have unsaved changes. Do you want to save before discarding and changing to another file?";
        public const string UnsavedNewFileMessage = "You have unsaved changes. Do you want to save before creating a new file?";
       public const string UnsavedYes = "Yes";
        public const string UnsavedNo = "No";
        public const string FileExistsMessage = "A file with that name already exists. Please choose a different name.";
        public const string FileExistsTitle = "File Exists";

        // Labels
        public const string NotepadTitle = "Notepad";
        public const string SelectFile = "Select File:";
        public const string CreateNewFileDialog = "Create New File";
        public const string NewNoteDefaultName = "NewNote";
        public const string FontSizeLabel = "Font Size:";

        // Errors
        public const string SaveError = "Failed to save Notepad: ";
        public const string LoadError = "Failed to load Notepad: ";
        public const string FontLoadError = "Failed to load Custom Font";
        public const string FileNotFound = "Notepad file not found: ";
        public const string NotesFolderNotFound = "Notes folder not found: ";
    }
}
#endif