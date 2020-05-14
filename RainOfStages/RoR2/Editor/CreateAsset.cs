using RainOfStages.Proxy;
using RainOfStages.Utilities;
using UnityEditor;

namespace RainOfStages.Editor
{
    public class CreateAsset
    {
        [MenuItem("Assets/Rain of Stages/" + nameof(SurfaceDef))]
        public static void CreateSurfaceDef() => ScriptableHelper.CreateAsset<SurfaceDef>();

        [MenuItem("Assets/Rain of Stages/" + nameof(DirectorCardCategorySelection))]
        public static void CreateDirectorCardCategorySelection() => ScriptableHelper.CreateAsset<DirectorCardCategorySelection>();


        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(SpawnCard))]
        public static void CreateSpawnCard() => ScriptableHelper.CreateAsset<SpawnCard>();
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(InteractableSpawnCard))]
        public static void CreateInteractableSpawnCard() => ScriptableHelper.CreateAsset<InteractableSpawnCard>();
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(CharacterSpawnCard))]
        public static void CreateCharacterSpawnCard() => ScriptableHelper.CreateAsset<CharacterSpawnCard>();
        [MenuItem("Assets/Rain of Stages/SpawnCards/" + nameof(BodySpawnCard))]
        public static void CreateBodySpawnCard() => ScriptableHelper.CreateAsset<BodySpawnCard>();


        [MenuItem("Assets/Rain of Stages/Stages/" + nameof(SceneDefReference))]
        public static void CreateSceneDefReference() => ScriptableHelper.CreateAsset<SceneDefReference>();

        [MenuItem("Assets/Rain of Stages/Stages/" + nameof(SceneDefinition))]
        public static void CreateCustomSceneProxy() => ScriptableHelper.CreateAsset<SceneDefinition>();

        [MenuItem("Assets/Rain of Stages/Modding Assets/" + nameof(BakeSettings))]
        public static void Create() => ScriptableHelper.CreateAsset<BakeSettings>();
    }
}