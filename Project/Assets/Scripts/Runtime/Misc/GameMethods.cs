using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


[System.Serializable]
public class GameObjectEvent : UnityEvent<GameObject>
{
}


public class GameMethods : MonoBehaviour
{
    public Transform SpawnPoint;

    public GameObjectEvent OnSpawn;


    void Start()
    {
        if (OnSpawn == null)
            OnSpawn = new GameObjectEvent();
    }


    public void ReloadActiveScene()
    {
        var scn = SceneManager.GetActiveScene();
        //SceneManager.LoadScene(scn.buildIndex, new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.Physics3D));
        SceneManager.LoadScene(scn.buildIndex, LoadSceneMode.Single);
    }


    public IEnumerator ReloadAdditiveScenes()
    {
        // Early out if only one scene (the active one) is loaded.
        if (SceneManager.sceneCount == 1)
            yield break;

        // Find the non-active scenes.
        var activeLoadedScene = SceneManager.GetActiveScene().buildIndex;
        var additiveScenes = new int[SceneManager.sceneCount];
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scnIndex = SceneManager.GetSceneAt(i).buildIndex;
            if (activeLoadedScene != scnIndex)
                additiveScenes[i] = scnIndex;
        }

        // Unload the scenes before reloading them.
        var asyncOps = new AsyncOperation[additiveScenes.Length];
        for (int i = 0; i < asyncOps.Length; i++)
            asyncOps[i] = SceneManager.UnloadSceneAsync(additiveScenes[i], UnloadSceneOptions.None);

        bool unloadDone = false;
        while (!unloadDone)
        {
            yield return null;
            unloadDone = true;
            foreach (var op in asyncOps)
                unloadDone = unloadDone && op.isDone;
        }

        // Reload the additive scenes.
        foreach (var buildIndex in additiveScenes)
            SceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
    }


    public void ReloadAllScenes()
    {
        // Keep the indices of the loaded scenes.
        var activeLoadedScene = SceneManager.GetActiveScene().buildIndex;
        var loadedScenes = new List<int>(SceneManager.sceneCount);
        for (int i = 0; i < SceneManager.sceneCount; i++)
            loadedScenes.Add(SceneManager.GetSceneAt(i).buildIndex);

        // First reload the main scene, then additively load the rest.
        // No need to unload the scenes first thanks to LoadSceneMode.Single.
        SceneManager.LoadScene(activeLoadedScene, LoadSceneMode.Single);
        foreach (var i in loadedScenes)
            if (i != activeLoadedScene)
                SceneManager.LoadScene(i, LoadSceneMode.Additive);
    }


    public void LoadScene(string sceneName)
    {
        var scn = SceneManager.GetSceneByName(sceneName);
        //SceneManager.LoadScene(scn.buildIndex, new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.Physics3D));
        SceneManager.LoadScene(scn.buildIndex, LoadSceneMode.Single);
    }


    public void Spawn(GameObject obj)
    {
        var newObj = Instantiate<GameObject>(obj, SpawnPoint.position, SpawnPoint.rotation);
        OnSpawn.Invoke(newObj);
    }
}
