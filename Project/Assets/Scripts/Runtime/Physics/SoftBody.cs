﻿using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;


public class SoftBody : MonoBehaviour
{
    public List<SoftBodyParticle> Particles = new List<SoftBodyParticle>();

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
                total += p.Connections.Count;
            return total;
        }
    }


    // Editor code that updates the component according to the changes in the inspector.
#if UNITY_EDITOR
    public bool DEBUG_LiveUpdate = false;
    [MinValue(0)] public int DEBUG_UpdateFrameSkip = 60;


    private Vector3[] _startPositions;
    private Quaternion[] _startRotations;
    private int _updatesLeftForUpdate = 0;


    private void Start()
    {
        _startPositions = new Vector3[Particles.Count];
        _startRotations = new Quaternion[Particles.Count];
        for (int i = 0; i < Particles.Count; i++)
        {
            _startPositions[i] = Particles[i].transform.position;
            _startRotations[i] = Particles[i].transform.rotation;
        }
    }

           
    private void FixedUpdate()
    {
        if (!DEBUG_LiveUpdate)
            return;

        _updatesLeftForUpdate--;
        if (_updatesLeftForUpdate >= 0)
            return;
        _updatesLeftForUpdate = DEBUG_UpdateFrameSkip;

        ResetAllJointValues();
    }
#endif


    public bool AreParticlesJoinedDirectly(SoftBodyParticle a, SoftBodyParticle b)
    {
        if (a.IsConnectedTo(b))
            return true;
        return b.IsConnectedTo(a);
    }


    // Returns false if all the joints between the two particles have broken.
    public bool AreParticlesJoinedRecursive(SoftBodyParticle a, SoftBodyParticle b)
    {
        if (a == b)
            throw new InvalidOperationException("Received the same particle in both arguments!");

        var conn = GetConnectionRecursive(a, b, false);
        return conn != null && !conn.isBroken;
    }


    public SoftBodyConnection GetConnectionRecursive(SoftBodyParticle a, SoftBodyParticle b, bool acceptBrokenJoints)
    {
        if (a == b)
            throw new InvalidOperationException("Received the same particle in both arguments!");

        var alreadyChecked = new HashSet<SoftBodyParticle>();
        var conn = GetConnectionRecursion(a, b, acceptBrokenJoints, ref alreadyChecked);
        if (conn != null)
            return conn;
        return GetConnectionRecursion(b, a, acceptBrokenJoints, ref alreadyChecked);
    }


    //private bool AreParticlesJoinedRecursion(SoftBodyParticle a, SoftBodyParticle b, ref HashSet<SoftBodyParticle> alreadyChecked)
    private SoftBodyConnection GetConnectionRecursion(SoftBodyParticle a, SoftBodyParticle b, bool acceptBrokenJoints, ref HashSet<SoftBodyParticle> alreadyChecked)
    {
        if (!alreadyChecked.Contains(a))
        {
            var conn = a.GetConnection(b);
            if (conn != null && (acceptBrokenJoints || !conn.isBroken))
                return conn;
            else
                alreadyChecked.Add(a);
        }

        foreach (var aConn in a.ConnectedParticles)
        {
            if (!alreadyChecked.Contains(aConn))
            {
                var conn = aConn.GetConnection(b);
                if (conn != null && (acceptBrokenJoints || !conn.isBroken))
                {
                    return conn;
                }
                else
                {
                    alreadyChecked.Add(aConn);
                    conn = GetConnectionRecursion(aConn, b, acceptBrokenJoints, ref alreadyChecked);
                    if (conn != null && (acceptBrokenJoints || !conn.isBroken))
                        return conn;
                }
            }
        }

        return null;
    }


    #region Joints and connections

    [Button("Recreate All Joints")]
    public void RecreateJoints()
    {
        foreach (var p in Particles)
            p.Clear();

        foreach (var p in Particles)
        {
            // Create new joints.
            foreach (var conn in p.Connections)
            {
                var newJoint = p.gameObject.AddComponent<ConfigurableJoint>();
                newJoint.connectedBody = conn.ConnectedParticle.Rigidbody;
                ResetJoint(newJoint);
                conn.Joint = newJoint;
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
            for (int i = 0; i < Particles.Count; i++)
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

    #endregion Joints and connections
}

