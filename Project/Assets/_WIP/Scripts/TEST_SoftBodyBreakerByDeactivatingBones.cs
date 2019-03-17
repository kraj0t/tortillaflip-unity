using UnityEngine;


[RequireComponent(typeof(SoftBody))]
public class TEST_SoftBodyBreakerByDeactivatingBones : MonoBehaviour
{
    private SoftBody _softBody;

    public SkinnedMeshRenderer SkinnedRenderer;


    void Start()
    {
        _softBody = GetComponent<SoftBody>();
    }


    private void OnSoftBodyJointBroken(SoftBodyJointBreakEventData e)
    {
        //DeactivateBoneIfDisconnected(e.);
    }


    private void DeactivateBoneIfDisconnected()
    {

    }






    // TODO: this could go in another component, which should handle the renderer.
    private void TryRemoveConnectedBoneFromSkin(SoftBodyJointBreakEventData e)
    {
        if (e.JointOwner != this && e.JointConnectedBody != this)
            return;

        var possibleBone = e.JointOwner == this ? e.JointConnectedBody.transform : e.JointOwner.transform;
        if (DeactivateBoneByReplacingWithDummy(SkinnedRenderer, possibleBone, this.transform))
            Debug.Log("Removed bone " + possibleBone.name + " from skin " + SkinnedRenderer.name + " and particle " + this.name);
    }


    // Returns false if the bone is not influencing the skin and there is no need to remove it.
    public static bool DeactivateBoneByReplacingWithDummy(SkinnedMeshRenderer skin, Transform bone, Transform parentBone)
    {
        var removed = false;

        // - Create a dummy gameObject.
        // - Replace the actual bone with the dummy.
        // - Make it a child of the particle. 
        // - Position it into the bindPose of the bone relative to the bindPose of the particle's bone.

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
