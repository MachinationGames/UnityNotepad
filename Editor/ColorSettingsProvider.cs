using System.Threading;
using UnityEditor;
using UnityEngine;
using System;

namespace Plugins.Machination.Notepad 
{
    public sealed class ColorSettingsProvider : ScriptableObject
    {
        private static ColorSettingsProvider instance = null;
        private static readonly object padlock = new object();
        ColorSettingsProvider()
        {
        }
        public ColorSettings ColorSettings { get; set; } = null;
        public static ColorSettingsProvider Instance
        {
            get
            {
                if (instance == null) 
                {
                    lock (padlock)
                    {
                        if(instance == null)
                        {
                            instance = CreateInstance<ColorSettingsProvider>();
                            instance.LoadColorSettings();
                        }
                    }
                }
                return instance;
            }
        }

        private void LoadColorSettings()
        {
            ColorSettings = AssetDatabase.LoadAssetAtPath<ColorSettings>(NotepadConstants.ColorSettingsDir);
            if (ColorSettings == null)
            {
                ColorSettings = CreateInstance<ColorSettings>();
                AssetDatabase.CreateAsset(ColorSettings, NotepadConstants.ColorSettingsDir);
                AssetDatabase.SaveAssets();
            }
        }
    }
}
