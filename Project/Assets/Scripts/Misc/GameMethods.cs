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
