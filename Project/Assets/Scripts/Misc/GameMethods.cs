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


    public void ReloadScene()
    {
        var scn = SceneManager.GetActiveScene();
        //SceneManager.LoadScene(scn.buildIndex, new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.Physics3D));
        SceneManager.LoadScene(scn.buildIndex, LoadSceneMode.Single);
    }


    public void ReloadAllScenes()
    {
        // Keep the indices of the loaded scenes.
        var activeLoadedScene = SceneManager.GetActiveScene().buildIndex;
        var loadedScenes = new List<int>(SceneManager.sceneCount);
        for (int i = 0; i < SceneManager.sceneCount; i++)
            loadedScenes.Add(SceneManager.GetSceneAt(i).buildIndex);

        // Unload all scenes before reloading them all.
        foreach (var i in loadedScenes)
            SceneManager.UnloadSceneAsync(i, UnloadSceneOptions.None);

        // First reload the main scene, then additively load the rest.
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
