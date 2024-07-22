using UnityEngine;

namespace Plugins.Machination.Notepad 
{
    [CreateAssetMenu(fileName = "ColorSettings", menuName = "Notepad/ColorSettings")]
    public class ColorSettings : ScriptableObject
    {
        public Color textColor = Color.white;
        public Color backgroundColor = Color.black;
    }
}
