#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Parking
{
    public static class ParkingTools
    {
        [MenuItem("Park It/Open Main Scene")]
        public static void OpenMainScene()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene("Assets/Scenes/Main.unity");
        }

        [MenuItem("Park It/Reset Progress")]
        public static void ResetProgress()
        {
            Save.Reset();
            Debug.Log("Park It: stars, best times and unlocks cleared.");
        }
    }
}
#endif
