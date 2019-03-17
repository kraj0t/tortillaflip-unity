using NaughtyAttributes;
using UnityEngine;

public class TEST_DeactivateSkinBone : MonoBehaviour
{
    public SkinnedMeshRenderer Skin;

    public Transform RemovedBone;
    public Transform ParentBone;


    void Start()
    {
        if (!Skin)
            Skin = GetComponent<SkinnedMeshRenderer>();

        
    }


    [Button]
    public void DoIt()
    {
        SoftBodyParticle.RemoveSkinBone_DUMMY_BINDPOSE(Skin, RemovedBone, ParentBone);
    }
}
