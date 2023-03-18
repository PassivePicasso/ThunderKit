using UnityEditor.SceneManagement;
using UnityEngine;

public class AddressablePreviewStage : PreviewSceneStage
{
    public string StageName;
    protected override GUIContent CreateHeaderContent()
    {
        return new GUIContent(StageName);
    }
}
