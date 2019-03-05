using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMethods : MonoBehaviour
{
    public void ReloadScene()
    {
        var scn = SceneManager.GetActiveScene();
        //scn.GetPhysicsScene().
        //SceneManager.LoadScene(scn.buildIndex, new LoadSceneParameters(LoadSceneMode.Single, LocalPhysicsMode.Physics3D));
        SceneManager.LoadScene(scn.buildIndex);
    }
}
