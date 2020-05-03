using UnityEditor;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements;

namespace RainOfStages.RainOfStagesShared.AutoConfig.Editor
{
    public class TestEditor : EditorWindow
    {
        [MenuItem("Tools/Rain of Stages/Download Bepinex")]
        static void Launch()
        {
            //var window = GetWindow<TestEditor>("Test Editor");
            //window.Show();
            BepInExPackLoader.DownloadBepinex();
        }

    }
}
