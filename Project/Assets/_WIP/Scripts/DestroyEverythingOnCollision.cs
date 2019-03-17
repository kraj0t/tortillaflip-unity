using UnityEngine;


public class DestroyEverythingOnCollision : MonoBehaviour
{
    public float DestructionDelay = 0;


    private void OnCollisionEnter(Collision collision)
    {
        Destroy(collision.gameObject, DestructionDelay);
    }
}
