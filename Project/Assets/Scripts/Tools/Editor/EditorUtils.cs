using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class EditorUtils
{
    public const float kLimitFactor = 0.9f;


    //[MenuItem("GameObject/TORTILLA/Set Config. Joint Linear Limit To Distance", false, -100)]
    [MenuItem("CONTEXT/TORTILLA/ConfigurableJoint/Set Linear Limit To Distance", false, -100)]
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
}
