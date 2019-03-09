using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class EditorUtils
{
    public const float kLimitFactor = 0.9f;


    //[MenuItem("GameObject/TORTILLA/Set Config. Joint Linear Limit To Distance", false, -100)]
    [MenuItem("CONTEXT/ConfigurableJoint/Set Linear Limit To Distance", false, -100)]
    [MenuItem("GameObject/TORTILLA/ConfigurableJoint/Set Linear Limit To Distance", false, -100)]
    public static void ResetJointsLinearLimits()
    {
        var l = new List<ConfigurableJoint>();

        foreach (var go in Selection.gameObjects)
        {
            go.GetComponentsInChildren<ConfigurableJoint>(true, l);
            foreach (var j in l)
            {
                SetJointLinearLimitToDistance(j, kLimitFactor);
            }
        }
    }


    public static void SetJointLinearLimitToDistance(ConfigurableJoint j, float factor)
    {
        var a = j.GetComponent<Rigidbody>();
        var b = j.connectedBody;
        if (!a || !b)
            throw new InvalidOperationException("Joints must have two bodies defined!");

        var AtoB = b.position - a.position;
        var dist = AtoB.magnitude;

        var linearLimit = j.linearLimit;
        linearLimit.limit = dist * factor;
        j.linearLimit = linearLimit;
    }



    #region Copy-Paste skin poses
    private static Transform[] _copiedBones;
    private static Matrix4x4[] _copiedBindPoses;

    [MenuItem("CONTEXT/SkinnedMeshRenderer/Copy skin rig", false, -100)]
    [MenuItem("GameObject/TORTILLA/SkinnedMeshRenderer/Copy skin rig", false, -100)]
    public static void CopySkinnedMeshRig()
    {
        if (Selection.gameObjects.Length > 1)
            throw new InvalidOperationException("Select only one gameObject with one SkinnedMeshRenderer");

        var skin = Selection.gameObjects[0].GetComponent<SkinnedMeshRenderer>();
        _copiedBones = skin.bones;
        _copiedBindPoses = skin.sharedMesh.bindposes;
    }


    [MenuItem("CONTEXT/SkinnedMeshRenderer/Paste skin rig", false, -100)]
    [MenuItem("GameObject/TORTILLA/SkinnedMeshRenderer/Paste skin rig", false, -100)]
    public static void PasteSkinnedMeshRig()
    {
        if (Selection.gameObjects.Length > 1)
            throw new InvalidOperationException("Select only one gameObject with one SkinnedMeshRenderer");

        var skin = Selection.gameObjects[0].GetComponent<SkinnedMeshRenderer>();
        var skinParent = skin.transform.parent;

        skin.sharedMesh.bindposes = _copiedBindPoses;

        // We look for same-named children in this SkinnedMeshRenderer.
        var newBones = new Transform[_copiedBones.Length];
        for (int i = 0; i < _copiedBones.Length; i++)
        {
            var srcBone = _copiedBones[i];
            var newBone = FindDeepChild(skinParent, srcBone.name);
            if (!newBone)
                Debug.LogError("No child bone found with the name \"" + srcBone.name + "\"", skin);
            else
                newBones[i] = newBone;
        }
        skin.bones = newBones;
    }


    public static Transform FindDeepChild(Transform parent, string name)
    {
        var queue = new Queue<Transform>();
        queue.Enqueue(parent);
        while (queue.Count > 0)
        {
            var c = queue.Dequeue();
            if (string.Equals(c.name, name))
                return c;
            foreach (Transform t in c)
                queue.Enqueue(t);
        }
        return null;
    }


    [MenuItem("CONTEXT/SkinnedMeshRenderer/TORTILLA/Paste skin rig", true, -100)]
    [MenuItem("GameObject/TORTILLA/SkinnedMeshRenderer/Paste skin rig", true, -100)]
    public static bool PasteSkinnedMeshRigValidate()
    {
        return _copiedBones != null && _copiedBindPoses != null;
    }
    #endregion Copy-Paste skin poses

    /*
    private static void ReflectionRestoreToBindPose()
    {
        if (_target == null)
            return;
        Type type = Type.GetType("UnityEditor.AvatarSetupTool, UnityEditor");
        if (type != null)
        {
            MethodInfo info = type.GetMethod("SampleBindPose", BindingFlags.Static | BindingFlags.Public);
            if (info != null)
            {
                info.Invoke(null, new object[] { _target });
            }
        }
    }
    */
}
