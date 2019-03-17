﻿using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SoftBody : MonoBehaviour
{
    public SoftBodyParticle[] Particles;

    [BoxGroup("Shape")] [MinValue(.0001f)] public float ColliderRadius = 0.025f;
    [BoxGroup("Shape")] [MinValue(.0001f)] public float ColliderHeight = 0.075f;

    [BoxGroup("Joints")] public bool EnablePreprocessing = true;
    [BoxGroup("Joints")] [MinValue(0)] public float LinearSpring = 200;
    [BoxGroup("Joints")] [MinValue(0)] public float LinearDamper = 10;

    [Tooltip("The linear limit will equal the distance between each pair of bodies multiplied by this factor.")]
    [BoxGroup("Joints")] [MinValue(0)] public float LinearLimitFactor = .9f;

    [BoxGroup("Joints")] [MinValue(0)] public float AngularSpring = 10;
    [BoxGroup("Joints")] [MinValue(0)] public float AngularDamper = .2f;
    [BoxGroup("Joints")] [MinValue(0)] public float AngularLimit = 45f;

    [Tooltip("The projection limit will equal the distance between each pair of bodies multiplied by this factor.")]
    [BoxGroup("Joints")] [MinValue(0)] public float HardLinearLimitFactor = 1.1f;

    [BoxGroup("Joints")] [MinValue(0)] public float HardAngularLimit = 60f;
    [BoxGroup("Joints")] [MinValue(0.01f)] public float BreakForce = Mathf.Infinity;
    [BoxGroup("Joints")] [MinValue(0.01f)] public float BreakTorque = Mathf.Infinity;


    [ShowNativeProperty]
    public int TotalNumberOfConnections
    {
        get
        {
            if (Particles == null)
                return 0;
            var total = 0;
            foreach (var p in Particles)
                total += p.ConnectedParticles.Count;
            return total;
        }
    }


    // This code updates the component according to the changes in the inspector.
#if UNITY_EDITOR
    private Vector3[] _startPositions;
    private Quaternion[] _startRotations;
    private void Start()
    {
        _startPositions = new Vector3[Particles.Length];
        _startRotations = new Quaternion[Particles.Length];
        for (int i = 0; i < Particles.Length; i++)
        {
            _startPositions[i] = Particles[i].transform.position;
            _startRotations[i] = Particles[i].transform.rotation;
        }
    }

    public bool DEBUG_LiveUpdate = false;
    [MinValue(0)] public int DEBUG_UpdateFrameSkip = 60;
    private int _updatesToSkip = 0;
    private void FixedUpdate()
    {
        if (!DEBUG_LiveUpdate)
            return;

        _updatesToSkip--;
        if (_updatesToSkip >= 0)
            return;
        _updatesToSkip = DEBUG_UpdateFrameSkip;

        ResetAllJointValues();
    }
#endif


    [Button("Recreate All Joints")]
    public void RecreateJoints()
    {
        foreach (var p in Particles)
            p.Clear();

        foreach (var p in Particles)
        {
            // Create new joints.
            foreach (var conn in p.ConnectedParticles)
            {
                var newJoint = p.gameObject.AddComponent<ConfigurableJoint>();
                newJoint.connectedBody = conn.Rigidbody;
                ResetJoint(newJoint);
                p.RegisterJoint(conn, newJoint);
            }
        }
    }


    [Button("Reset All Joint Values")]
    [ContextMenu("Reset All Joint Values")]
    public void ResetAllJointValues()
    {
#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            for (int i = 0; i < Particles.Length; i++)
            {
                Particles[i].transform.position = _startPositions[i];
                Particles[i].transform.rotation = _startRotations[i];
            }
        }
#endif

        foreach (var p in Particles)
        {
            if (!p)
                continue;

            foreach (var col in p.GetComponents<SphereCollider>())
                col.radius = ColliderRadius;
            foreach (var col in p.GetComponents<CapsuleCollider>())
            {
                col.radius = ColliderRadius;
                col.height = ColliderHeight;
            }

            foreach (var j in p.GetComponents<ConfigurableJoint>())
                ResetJoint(j);

            p.Rigidbody.WakeUp();
        }
    }


    private void ResetJoint(ConfigurableJoint j)
    {
        var a = j.GetComponent<Rigidbody>();
        var b = j.connectedBody;
        if (!a || !b)
            throw new InvalidOperationException("Joints must have two bodies defined!");

        j.configuredInWorldSpace = true;

        j.enablePreprocessing = EnablePreprocessing;

        // This lets PhysX configure the anchor.
        j.connectedBody = null;
        j.autoConfigureConnectedAnchor = true;
        j.connectedBody = null;
        j.connectedBody = b;

        var AtoB = b.position - a.position;
        var dir = AtoB.normalized;
        var dist = Vector3.Dot(AtoB, dir);
        j.axis = dir;
        j.secondaryAxis = Quaternion.LookRotation(dir) * Vector3.right;

        j.xMotion = ConfigurableJointMotion.Limited;
        j.yMotion = ConfigurableJointMotion.Locked;
        j.zMotion = ConfigurableJointMotion.Locked;
        j.angularXMotion = ConfigurableJointMotion.Limited;
        j.angularYMotion = ConfigurableJointMotion.Limited;
        j.angularZMotion = ConfigurableJointMotion.Limited;

        var linearLimit = new SoftJointLimit() { limit = dist * LinearLimitFactor };
        j.linearLimit = linearLimit;

        var angularLimitNeg = new SoftJointLimit() { limit = -AngularLimit };
        j.lowAngularXLimit = angularLimitNeg;
        var angularLimit = new SoftJointLimit() { limit = AngularLimit };
        j.highAngularXLimit = angularLimit;
        j.angularYLimit = angularLimit;
        j.angularZLimit = angularLimit;

        var linearDrive = new JointDrive() { positionSpring = LinearSpring, positionDamper = LinearDamper, maximumForce = Mathf.Infinity };
        j.xDrive = linearDrive;

        j.rotationDriveMode = RotationDriveMode.Slerp;
        var angularDrive = new JointDrive() { positionSpring = AngularSpring, positionDamper = AngularDamper, maximumForce = Mathf.Infinity };
        j.slerpDrive = angularDrive;

        j.breakForce = BreakForce;
        j.breakTorque = BreakTorque;

        j.projectionMode = JointProjectionMode.PositionAndRotation;
        j.projectionDistance = dist * HardLinearLimitFactor;
        j.projectionAngle = HardAngularLimit;
    }
}
