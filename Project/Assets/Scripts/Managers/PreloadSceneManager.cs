using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloadSceneManager : MonoBehaviour
{
#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitPreloadScene()
    {
        // Make sure your _preload scene is the first in scene build list.
        if (!SceneManager.GetSceneAt(0).isLoaded)
            SceneManager.LoadScene(0, LoadSceneMode.Additive);
    }
#endif


#if !UNITY_EDITOR
    private void Awake()
    {
        SceneManager.LoadScene(1);
    }
#endif
}