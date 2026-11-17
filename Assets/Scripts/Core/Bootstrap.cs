using UnityEngine;

namespace Parking
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (Object.FindFirstObjectByType<GameApp>() != null) return;
            var go = new GameObject("PARK IT!");
            go.AddComponent<GameApp>();
        }
    }
}
