using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class Tortilla : MonoBehaviour
{
    public Transform RespawnPoint;
    public LayerMask DeathTriggers;
    public float WaitBeforeRespawn = 1;
    public float WaitAfterRespawn = 1;


    public Rigidbody Rigidbody { get; private set; }


    private bool _respawning;


    private void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }


    public void Respawn()
    {
        if (!RespawnPoint || _respawning)
            return;
        StartCoroutine(RespawnCoroutine());        
    }


    private IEnumerator RespawnCoroutine()
    {
        _respawning = true;
        yield return new WaitForSeconds(WaitBeforeRespawn);
        Rigidbody.position = RespawnPoint.position;
        Rigidbody.rotation = Quaternion.identity;
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        yield return new WaitForSeconds(WaitAfterRespawn);
        _respawning = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        var otherLayer = 1 << collision.gameObject.layer;
        if ((DeathTriggers & otherLayer) != 0)
        {
            Respawn();
        }
    }
}
