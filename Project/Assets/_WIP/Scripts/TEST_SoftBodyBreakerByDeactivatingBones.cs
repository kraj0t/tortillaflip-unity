using UnityEngine;


[RequireComponent(typeof(SoftBody))]
public class TEST_SoftBodyBreakerByDeactivatingBones : MonoBehaviour
{
    [Min(.01f)] public float DisconnectedBonesCheckFrequency = .1f;

    private SoftBody _softBody;

    private float _nextDisconnectionCheckTime;


    // TODO: refactor this class, renderer should go in another component.


    void Start()
    {
        Debug.Log("TODO: You need to check that there are no duplicate connections!");

        _softBody = GetComponent<SoftBody>();

        foreach (var p in _softBody.Particles)
            p.OnJointBroken.AddListener(OnSoftBodyJointBroken);

        _nextDisconnectionCheckTime = Time.time + DisconnectedBonesCheckFrequency;
    }


    /*
    private void Update()
    {
        // Every few frames, remove all disconnected particles' bones.
        if (Time.time < _nextDisconnectionCheckTime)
            return;

        _nextDisconnectionCheckTime = Time.time + DisconnectedBonesCheckFrequency;

        // Loop the bones in the particles' skinnedMeshRenderers, and filter out the bones that are not particles of the soft body.
        // This will early-out all the dummy bones that we may have already created.
        foreach (var particle in _softBody.Particles)
        {
            var particleTransform = particle.transform;
            foreach (var bone in particle.SkinnedRenderer.bones)
            {
                if (bone == particleTransform)
                    continue;
                var otherParticle = bone.GetComponent<SoftBodyParticle>();
                if (otherParticle)
                    if (_softBody.Particles.Contains(otherParticle))
                        if (DeactivateBonesIfDisconnected(_softBody, particle, otherParticle))
                            Debug.Log("Automatically disconnected " + particle.name + " from " + otherParticle.name + " in Update!");
            }
        }
    }
    */

    private void OnSoftBodyJointBroken(SoftBodyJointBreakEventData e)
    {
        var owner = e.JointOwner;
        var other = e.JointConnectedBody;

        DeactivateBoneByReplacingWithDummy(owner.SkinnedRenderer, other.transform, owner.transform);
        DeactivateBoneByReplacingWithDummy(other.SkinnedRenderer, owner.transform, other.transform);

        foreach (var p in _softBody.Particles)
        {
            if (p != owner && p != other)
            {
                DeactivateBonesIfDisconnected(_softBody, p, owner);
                DeactivateBonesIfDisconnected(_softBody, p, other);
            }
        }
    }


    // Returns true if any bone was deactivated.
    public static bool DeactivateBonesIfDisconnected(SoftBody softBody, SoftBodyParticle a, SoftBodyParticle b)
    {
        var anyBoneDeactivated = false;

        if (!softBody.AreParticlesJoinedAndInSameTree(a, b))
        {
            anyBoneDeactivated = anyBoneDeactivated || DeactivateBoneByReplacingWithDummy(a.SkinnedRenderer, b.transform, a.transform);
            anyBoneDeactivated = anyBoneDeactivated || DeactivateBoneByReplacingWithDummy(b.SkinnedRenderer, a.transform, b.transform);
        }

        return anyBoneDeactivated;
    }


    // "Deactivates" a SkinnedMeshRenderer's bone by doing the following:
    // - Create a dummy gameObject.
    // - Assign the dummy as bone instead of the original one.
    // - Make the dummy a child of the particle. 
    // - Position it into the bindPose of the parent bone relative to the bindPose of the original bone.
    //
    // Returns false if the bone is not influencing the skin and there is no need to remove it.
    public static bool DeactivateBoneByReplacingWithDummy(SkinnedMeshRenderer skin, Transform bone, Transform parentBone)
    {
        var removed = false;

        for (int boneIndex = 0; !removed && boneIndex < skin.bones.Length; boneIndex++)
        {
            if (skin.bones[boneIndex] == bone)
            {
                removed = true;

                var newBones = skin.bones;
                var originalBone = newBones[boneIndex];
                var dummy = new GameObject("Deactivated bone [" + originalBone.name + "]").transform;
                newBones[boneIndex] = dummy;
                skin.bones = newBones;

                Matrix4x4 parentBindPose = Matrix4x4.identity;
                if (parentBone)
                {
                    dummy.SetParent(parentBone, false);
                    for (int parentBoneIndex = 0; parentBoneIndex < skin.bones.Length; parentBoneIndex++)
                    {
                        if (skin.bones[parentBoneIndex] == parentBone)
                        {
                            parentBindPose = skin.sharedMesh.bindposes[parentBoneIndex];
                            break;
                        }
                    }
                }

                var boneBindPose = skin.sharedMesh.bindposes[boneIndex];
                var relativeBindPose = parentBindPose * boneBindPose.inverse;
                dummy.localPosition = relativeBindPose.DecodePosition();
                dummy.localRotation = relativeBindPose.DecodeRotation();
                dummy.localScale = relativeBindPose.DecodeScale();
            }
        }

        //if (removed)
        //    Debug.Log("Removed <color=green>SUCCESSFULY</color> bone <b>" + bone.name + "</b> from skin <b>" + skin.name + "</b> with parent bone <b>" + parentBone.name + "</b>", bone);
        //else
        //    Debug.LogWarning("Could <color=red>NOT</color> remove bone <b>" + bone.name + "</b> from skin <b>" + skin.name + "</b> with parent bone <b>" + parentBone.name + "</b>", bone);

        return removed;
    }
}

