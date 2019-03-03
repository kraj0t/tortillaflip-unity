using UnityEngine;
using UnityEngine.SceneManagement;

public static class PreloadSceneIntegration
{
#if UNITY_EDITOR 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitLoadingScene()
    {
        // Make sure your _preload scene is the first in scene build list.
        if (!SceneManager.GetSceneAt(0).isLoaded)
            SceneManager.LoadScene(0, LoadSceneMode.Additive);        
    }
#endif
}