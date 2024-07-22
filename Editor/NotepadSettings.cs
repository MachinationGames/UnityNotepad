#if UNITY_EDITOR
using UnityEngine;

namespace Plugins.Machination.Notepad 
{
    [CreateAssetMenu(fileName = "NotepadSettings", menuName = "Notepad/NotepadSettings")]
    public class NotepadSettings : ScriptableObject
    {
        public Color textColor = new Color32(0x26, 0xAB, 0x2E, 0xFF); // Hex: 26AB2E
        public Color backgroundColor = new Color32(0x2A, 0x2A, 0x2A, 0xFF); // Hex: 2A2A2A
        public string selectedFont = "CourierPrime";
        public bool useCustomFont = true;
    }
}
#endif