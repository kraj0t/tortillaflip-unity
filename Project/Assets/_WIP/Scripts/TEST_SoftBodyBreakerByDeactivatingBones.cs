using UnityEngine;


[RequireComponent(typeof(SoftBody))]
public class TEST_SoftBodyBreakerByDeactivatingBones : MonoBehaviour
{
    private SoftBody _softBody;


    void Start()
    {
        _softBody = GetComponent<SoftBody>();

        foreach (var p in _softBody.Particles)
            p.OnJointBroken.AddListener(OnSoftBodyJointBroken);
    }


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
                DeactivateBoneIfDisconnected(p, owner);
                DeactivateBoneIfDisconnected(p, other);
            }
        }
    }


    private void DeactivateBoneIfDisconnected(SoftBodyParticle a, SoftBodyParticle b)
    {
        if (_softBody.AreParticlesJoinedRecursive(a, b))
        {
            DeactivateBoneByReplacingWithDummy(a.SkinnedRenderer, b.transform, a.transform);
            DeactivateBoneByReplacingWithDummy(b.SkinnedRenderer, a.transform, b.transform);
        }
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


    #region OLD & POSSIBLY DEPRECATED
    // Returns false if the bone is not influencing the skin.
    public static bool RemoveSkinBone_OLD_APPROACH__REMOVING_MESH_BONEWEIGHTS(SkinnedMeshRenderer skin, Transform bone)
    {
        var removed = false;

        for (int i = 0; !removed && i < skin.bones.Length; i++)
        {
            if (skin.bones[i] == bone)
            {
                skin.bones[i] = null;
                RemoveMeshBoneWeights(skin.sharedMesh, i);
                removed = true;
            }
        }

        return removed;
    }

    public static void RemoveMeshBoneWeights(Mesh mesh, int boneIndex)
    {
        var newBoneWeights = mesh.boneWeights;
        for (int vertexIndex = 0; vertexIndex < newBoneWeights.Length; vertexIndex++)
        {
            if (newBoneWeights[vertexIndex].boneIndex0 == boneIndex)
            {
                newBoneWeights[vertexIndex].weight0 = 0;
                var norm = new Vector3(newBoneWeights[vertexIndex].weight1, newBoneWeights[vertexIndex].weight2, newBoneWeights[vertexIndex].weight3).normalized;
                newBoneWeights[vertexIndex].weight1 = norm.x;
                newBoneWeights[vertexIndex].weight2 = norm.y;
                newBoneWeights[vertexIndex].weight3 = norm.z;
            }
            else if (newBoneWeights[vertexIndex].boneIndex1 == boneIndex)
            {
                newBoneWeights[vertexIndex].weight1 = 0;
                var norm = new Vector3(newBoneWeights[vertexIndex].weight0, newBoneWeights[vertexIndex].weight2, newBoneWeights[vertexIndex].weight3).normalized;
                newBoneWeights[vertexIndex].weight0 = norm.x;
                newBoneWeights[vertexIndex].weight2 = norm.y;
                newBoneWeights[vertexIndex].weight3 = norm.z;
            }
            else if (newBoneWeights[vertexIndex].boneIndex2 == boneIndex)
            {
                newBoneWeights[vertexIndex].weight2 = 0;
                var norm = new Vector3(newBoneWeights[vertexIndex].weight0, newBoneWeights[vertexIndex].weight1, newBoneWeights[vertexIndex].weight3).normalized;
                newBoneWeights[vertexIndex].weight0 = norm.x;
                newBoneWeights[vertexIndex].weight1 = norm.y;
                newBoneWeights[vertexIndex].weight3 = norm.z;
            }
            else
            {
                newBoneWeights[vertexIndex].weight3 = 0;
                var norm = new Vector3(newBoneWeights[vertexIndex].weight0, newBoneWeights[vertexIndex].weight1, newBoneWeights[vertexIndex].weight2).normalized;
                newBoneWeights[vertexIndex].weight0 = norm.x;
                newBoneWeights[vertexIndex].weight1 = norm.y;
                newBoneWeights[vertexIndex].weight2 = norm.z;
            }
        }

        mesh.boneWeights = newBoneWeights;
    }
    #endregion OLD & POSSIBLY DEPRECATED
}

