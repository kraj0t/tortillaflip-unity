using System.Collections;
using UnityEngine;
using NaughtyAttributes;

[DisallowMultipleComponent]
public class Respawner : MonoBehaviour
{
    [InfoBox("If the object has a Rigidbody it will be reset as well.", InfoBoxType.Normal)]
    [InfoBox("The rotation of the RespawnPoint is also applied, but not the scale.", InfoBoxType.Normal)]
    public Transform RespawnPoint;

    [InfoBox("If the object has no collider, you still can respawn it manually (via code or the button below).", InfoBoxType.Normal)]
    public LayerMask DeathTriggers;

    public float WaitBeforeRespawn = 1;
    public float WaitAfterRespawn = 1;


    private bool _respawning;


    [Button]
    public void Respawn()
    {
        if (!RespawnPoint || _respawning)
            return;
        _respawning = true;
        StartCoroutine(RespawnCoroutine());        
    }


    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(WaitBeforeRespawn);

        transform.position = RespawnPoint.position;
        transform.rotation = Quaternion.identity;

        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.position = RespawnPoint.position;
            rb.rotation = Quaternion.identity;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

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
