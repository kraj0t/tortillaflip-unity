using NaughtyAttributes;
using System;
using UnityEngine;


/// <summary>
/// Helper class for configuring and modifying a soft body.
/// </summary>
[AddComponentMenu("Tortilla Flip/Physics/Soft Body Creator", -1000)]
[RequireComponent(typeof(SoftBody))]
[DisallowMultipleComponent]
public class SoftBodyCreator : MonoBehaviour
{
    [BoxGroup("Shape")] [MinValue(.0001f)] public float ColliderRadius = 0.025f;
    [BoxGroup("Shape")] [MinValue(.0001f)] public float ColliderHeight = 0.075f;

    [BoxGroup("Joints")] public bool EnablePreprocessing = false;
    [BoxGroup("Joints")] [MinValue(0)] public float LinearSpring = 200;
    [BoxGroup("Joints")] [MinValue(0)] public float LinearDamper = 1;

    [Tooltip("The linear limit will equal the distance between each pair of bodies multiplied by this factor.")]
    [BoxGroup("Joints")] [MinValue(0)] public float LinearLimitFactor = .9f;

    [BoxGroup("Joints")] [MinValue(0)] public float AngularSpring = 0.1f;
    [BoxGroup("Joints")] [MinValue(0)] public float AngularDamper = .001f;
    [BoxGroup("Joints")] [MinValue(0)] public float AngularLimit = 90f;

    [Tooltip("The projection limit will equal the distance between each pair of bodies multiplied by this factor.")]
    [BoxGroup("Joints")] [MinValue(0)] public float HardLinearLimitFactor = 1.1f;

    [BoxGroup("Joints")] [MinValue(0)] public float HardAngularLimit = 120f;
    [BoxGroup("Joints")] [MinValue(0.01f)] public float BreakForce = Mathf.Infinity;
    [BoxGroup("Joints")] [MinValue(0.01f)] public float BreakTorque = Mathf.Infinity;


    // Editor code that updates the component according to the changes in the inspector.
#if UNITY_EDITOR
    [Space(20)]
    public bool DEBUG_LiveUpdate = false;
    [MinValue(0)] public int DEBUG_UpdateFrameSkip = 120;


    private Vector3[] _startPositions;
    private Quaternion[] _startRotations;
    private int _updatesLeftForUpdate = 0;


    private void Start()
    {
        var sb = GetComponent<SoftBody>();

        _startPositions = new Vector3[sb.Particles.Count];
        _startRotations = new Quaternion[sb.Particles.Count];
        for (int i = 0; i < sb.Particles.Count; i++)
        {
            _startPositions[i] = sb.Particles[i].transform.position;
            _startRotations[i] = sb.Particles[i].transform.rotation;
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


    [Button("Recreate All Joints")]
    public void RecreateJoints()
    {
        var sb = GetComponent<SoftBody>();

        foreach (var p in sb.Particles)
            p.Clear();

        foreach (var p in sb.Particles)
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
        var sb = GetComponent<SoftBody>();

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            for (int i = 0; i < sb.Particles.Count; i++)
            {
                sb.Particles[i].transform.position = _startPositions[i];
                sb.Particles[i].transform.rotation = _startRotations[i];
            }
        }
#endif

        foreach (var p in sb.Particles)
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
