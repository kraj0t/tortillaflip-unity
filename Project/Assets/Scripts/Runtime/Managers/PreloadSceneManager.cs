using UnityEngine;
using UnityEngine.SceneManagement;

public class PreloadSceneManager : MonoBehaviour
{
    public string FirstScene;
    public string[] AdditiveScenes;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitPreloadScene()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

#if UNITY_EDITOR
        // Make sure your _preload scene is the first in scene build list.
        if (!SceneManager.GetSceneAt(0).isLoaded)
            SceneManager.LoadScene(0, LoadSceneMode.Additive);
#endif
    }


    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    private static void OnSceneLoaded(Scene scn, LoadSceneMode loadMode)
    {
        if (scn.buildIndex == 0)
            SceneManager.UnloadSceneAsync(0, UnloadSceneOptions.None);
    }


#if !UNITY_EDITOR
    private void Awake()
    {
        SceneManager.LoadScene(FirstScene);
        foreach (var s in AdditiveScenes)
            SceneManager.LoadScene(s, LoadSceneMode.Additive);
    }
#endif
}