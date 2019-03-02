using UnityEngine;

public class TortillaSpawner : MonoBehaviour
{
    public GameObject Spawnable;
    public Transform SpawnPoint;


    private GameObject _currentlySpawned;

    void Update()
    {
        if (!Spawnable || !SpawnPoint)
            return;

        if (!_currentlySpawned)
        {
            // TO DO add some random rotation
            _currentlySpawned = Instantiate<GameObject>(Spawnable, SpawnPoint.position, Quaternion.identity);
        }
    }
}
