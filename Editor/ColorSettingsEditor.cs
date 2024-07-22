using UnityEditor;
using UnityEngine;

namespace Plugins.Machination.Notepad 
{
    public class ColorSettingsEditor : EditorWindow
    {
        private ColorSettings _colorSettings;

        [MenuItem("Tools/Machination/Notepad/Color Settings", false, 50)]
        public static void ShowWindow()
        {
            GetWindow<ColorSettingsEditor>("Color Settings");
        }

        private void OnEnable()
        {
            LoadColorSettings();
        }

        private void OnGUI()
        {
            GUILayout.Label("Edit Notepad Colors", EditorStyles.boldLabel);

            _colorSettings = (ColorSettings)EditorGUILayout.ObjectField("Color Settings", _colorSettings, typeof(ColorSettings), false);

            if (!_colorSettings) return;
            _colorSettings.textColor = EditorGUILayout.ColorField("Text Color", _colorSettings.textColor);
            _colorSettings.backgroundColor =
                EditorGUILayout.ColorField("Background Color", _colorSettings.backgroundColor);

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(_colorSettings);
                AssetDatabase.SaveAssets();
            }
            else
            {
                GUILayout.Label("No ColorSettings asset found.");
            }
        }

        private void LoadColorSettings()
        {

            _colorSettings = ColorSettingsProvider.Instance.ColorSettings;
        }
    }
}
