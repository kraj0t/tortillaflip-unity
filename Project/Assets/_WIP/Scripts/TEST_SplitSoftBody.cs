using NaughtyAttributes;
using UnityEngine;


[RequireComponent(typeof(SoftBody))]
public class TEST_SplitSoftBody : MonoBehaviour
{
    public SoftBodyParticle a;
    public SoftBodyParticle b;
    public bool FullTreeSearch = true;


    [Button]
    public void DisplayConnection()
    {
        var softBody = GetComponent<SoftBody>();

        var areJoined = softBody.AreParticlesJoinedAndInSameTree(a, b);

        var color = areJoined ? Color.green : Color.red;

        Debug.DrawLine(a.transform.position, b.transform.position, color);
    }
}
