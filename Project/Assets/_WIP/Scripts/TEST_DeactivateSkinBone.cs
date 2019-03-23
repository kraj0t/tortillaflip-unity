using NaughtyAttributes;
using UnityEngine;

public class TEST_DeactivateSkinBone : MonoBehaviour
{
    public SkinnedMeshRenderer Skin;

    public Transform RemovedBone;
    public Transform ParentBone;

    public SoftBody SoftBody;
    public SoftBodyParticle ParticleA;
    public SoftBodyParticle ParticleB;


    void Start()
    {
        if (!Skin)
            Skin = GetComponent<SkinnedMeshRenderer>();

        
    }


    [Button]
    public void DeactivateBoneByReplacingWithDummy()
    {
        TEST_SoftBodyBreakerByDeactivatingBones.DeactivateBoneByReplacingWithDummy(Skin, RemovedBone, ParentBone);
    }


    [Button]
    public void DeactivateBonesIfDisconnected()
    {
        TEST_SoftBodyBreakerByDeactivatingBones.DeactivateBonesIfDisconnected(SoftBody, ParticleA, ParticleB);
    }
}
