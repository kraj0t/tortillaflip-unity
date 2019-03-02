using UnityEngine;

public class Tortilla : MonoBehaviour
{
    public LayerMask DeathTriggers;

    private void OnTriggerEnter(Collider other)
    {
        var otherLayer = 1 << other.gameObject.layer;
        if ((DeathTriggers & otherLayer) != 0)
        {
            Destroy(gameObject);
        }
    }

}
