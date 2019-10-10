using UnityEditor;
using UnityEngine;

namespace XUFindRef
{
    public class XUFindReferences
    {
        [MenuItem("Assets/XU Find References", false, 10)]
        [MenuItem("Window/XU Find References", false, 1000)]
        public static void ShowWindow()
        {
            uint w = 800;
            uint h = 600;
            Rect wr = new Rect((Screen.width - w) / 2, (Screen.height - h) / 2, w, h);
            XUFindReferencesWindows window = (XUFindReferencesWindows)EditorWindow.GetWindowWithRect(typeof(XUFindReferencesWindows), wr, true, "Find References");
            window.Show();
        }
    }
}