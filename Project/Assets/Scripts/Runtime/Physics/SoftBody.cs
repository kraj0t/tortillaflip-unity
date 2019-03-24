using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[Serializable]
public class SoftBodySplitEventData
{
    public readonly SoftBody OriginalSoftBody;
    public readonly SoftBody NewSoftBody;
    public SoftBodySplitEventData(SoftBody originalSoftBody, SoftBody newSoftBody)
    {
        OriginalSoftBody = originalSoftBody;
        NewSoftBody = newSoftBody;
    }
}


[Serializable]
public class SoftBodySplitEvent : UnityEvent<SoftBodySplitEventData>
{
}



[AddComponentMenu("Tortilla Flip/Physics/Soft Body", -1000)]
[DisallowMultipleComponent]
public class SoftBody : MonoBehaviour
{
    [BoxGroup("Particles")]
    [Label("(Expand to see the particle array)")]
    public List<SoftBodyParticle> Particles = new List<SoftBodyParticle>();

    public SoftBodySplitEvent OnSplit;


    #region Connections

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


    // TODO: documentation
    public bool AreParticlesJoinedAndInSameTree(SoftBodyParticle a, SoftBodyParticle b)
    {
        if (a == b)
            throw new InvalidOperationException("Received the same particle in both arguments!");

        // Check one side of the tree.
        var treeA = new HashSet<SoftBodyParticle>();
        if (FindParticleWhileCollectingTree(a, b, ref treeA))
            return true;

        // Check the other side of the tree.
        var treeB = new HashSet<SoftBodyParticle>();
        if (FindParticleWhileCollectingTree(b, a, ref treeB))
            return true;

        // If the intersection of both trees is not empty, it means they live in the same tree.
        treeA.IntersectWith(treeB);
        return treeA.Count != 0;
    }


    // Returns true if b was found in a valid connection.
    private bool FindParticleWhileCollectingTree(SoftBodyParticle a, SoftBodyParticle b, ref HashSet<SoftBodyParticle> alreadyCollectedTreeNodes)
    {
        if (!alreadyCollectedTreeNodes.Contains(a))
        {
            var conn = a.GetConnection(b);
            if (conn != null && !conn.IsBroken)
                return true;
            alreadyCollectedTreeNodes.Add(a);
        }

        foreach (var connectedParticle in a.ConnectedParticles)
        {
            if (!alreadyCollectedTreeNodes.Contains(connectedParticle))
            {
                if (FindParticleWhileCollectingTree(connectedParticle, b, ref alreadyCollectedTreeNodes))
                    return true;
                alreadyCollectedTreeNodes.Add(connectedParticle);
            }
        }

        return false;
    }


    /*
    // Convenience method that simply calls GetConnection() and queries IsBroken.
    // Returns true if the particles are connected AND their joint is alive.
    public bool AreParticlesJoined(SoftBodyParticle a, SoftBodyParticle b, bool fullTreeSearch)
    {
        var conn = GetConnection(a, b, fullTreeSearch);
        if (conn == null || conn.isBroken)
            return false;
        return true;
    }
    */


    /*
    // Returns null if the bodies are not connected to one another.
    // The order in which the arguments are passed does not matter.
    // If fullTreeSearch is true, only the first connection found will be returned. See GetConnections().
    // But take into account that there may be more than one connection between both particles, even though it should be disallowed.
    public SoftBodyConnection GetConnection(SoftBodyParticle a, SoftBodyParticle b, bool fullTreeSearch)
    {
        if (!fullTreeSearch)
            return GetDirectConnection(a, b);

        var connectionsFound = GetConnectionsRecursively(a, b, false, true);
        if (connectionsFound.Count > 0)
            return connectionsFound[0];

        return null;
    }
    */


    /*
    // Performs a tree search and returns all the connections found that link 
    // The order in which the arguments are passed does not matter.
    // If fullTreeSearch is true, only the first connection found will be returned. See GetConnections().
    // But take into account that there may be more than one connection between both particles, even though it should be disallowed.
    public SoftBodyConnection[] GetConnections(SoftBodyParticle a, SoftBodyParticle b)
    {
        if (!fullTreeSearch)
            return GetDirectConnection(a, b);

        var connectionsFound = GetConnectionsRecursively(a, b, false, true);
        if (connectionsFound.Count > 0)
            return connectionsFound[0];

        return null;
        TO BE IMPLEMENTED
    }
    */


    /*
    // Returns null if the bodies are not directly connected to one another (without searching recursively).
    // The order of the arguments does not matter.
    private SoftBodyConnection GetDirectConnection(SoftBodyParticle a, SoftBodyParticle b)
    {
        var conn = a.GetConnection(b);
        if (conn != null)
            return conn;
        return b.GetConnection(a);
    }
    */


    /*
    // The order of the arguments does not matter.
    private IList<SoftBodyConnection> GetConnectionsRecursively(SoftBodyParticle a, SoftBodyParticle b, bool acceptBrokenJoints, bool stopWhenFirstIsFound)
    {
        if (a == b)
            throw new InvalidOperationException("Received the same particle in both arguments!");

        var connectionsFound = new List<SoftBodyConnection>();

        // Check one side of the tree.
        var treeA = new HashSet<SoftBodyParticle>();
        TreeSearchConnections(a, b, ref connectionsFound, ref treeA, acceptBrokenJoints, stopWhenFirstIsFound);
        if (stopWhenFirstIsFound && connectionsFound.Count != 0)
            return connectionsFound;

        // Check the other side of the tree.
        var treeB = new HashSet<SoftBodyParticle>();
        TreeSearchConnections(b, a, ref connectionsFound, ref treeB, acceptBrokenJoints, stopWhenFirstIsFound);

        return connectionsFound;
    }
    */


    /*
    private void TreeSearchConnections(SoftBodyParticle a, SoftBodyParticle b, ref List<SoftBodyConnection> connectionsFound, ref HashSet<SoftBodyParticle> nodesAlreadyChecked, bool acceptBrokenJoints, bool stopWhenFirstIsFound)
    {
        if (!nodesAlreadyChecked.Contains(a))
        {
            var conn = a.GetConnection(b);
            if (conn != null && (acceptBrokenJoints || !conn.isBroken))
            {
                connectionsFound.Add(conn);
                if (stopWhenFirstIsFound)
                    return;
            }
            else
                nodesAlreadyChecked.Add(a);
        }

        foreach (var aConn in a.ConnectedParticles)
        {
            if (!nodesAlreadyChecked.Contains(aConn))
            {
                var conn = aConn.GetConnection(b);
                if (conn != null && (acceptBrokenJoints || !conn.isBroken))
                {
                    connectionsFound.Add(conn);
                    if (stopWhenFirstIsFound)
                        return;
                }
                else
                {
                    nodesAlreadyChecked.Add(aConn);
                    TreeSearchConnections(aConn, b, ref connectionsFound, ref nodesAlreadyChecked, acceptBrokenJoints, stopWhenFirstIsFound);
                    if (stopWhenFirstIsFound && connectionsFound.Count != 0)
                        return;
                }
            }
        }
    }
    */

    #endregion Connections


    /// <summary>
    /// Creates a new softbody with the given subset of the softbody's particles. The original SoftBody will still exist and will contain the rest of particles.
    /// - Check that the subset is not empty or that it does not contain all the particles in the softbody.
    /// - Check that all the particles in the subset belong to the softbody. 
    /// - Check that all the particles in the subset are connected among themselves. InvalidOperationException is thrown if not. _(how many particles do we have to query to build the hashset? if a particle has no connections, then skip it, right?)
    /// - Parent the new softbody to the original one's parent.
    /// - Move the subset particles to their new softbody's transform.
    /// - Destroy Joints as needed.
    /// </summary>
    /// <param name="subset">The particles that the new SoftBody will contain. They must all be connected among themselves, otherwise an InvalidOperationException will be thrown.</param>
    /// <returns>A new SoftBody that owns the particles in the passed subset.</returns>
    public SoftBody Split(IEnumerable<SoftBodyParticle> subset)
    {
        ///////////////OnSplit.Invoke(new SoftBodySplitEventData(this, newSoftBody);
        throw new NotImplementedException();
    }


    // Helper method that finds the particles to each side of a plane and then calls Split().
    // Returns null if all the particles are on the same side of the plane, and therefore no new softbody is created.
    // On a successful split, returns the new softbody that remains on the negative side of the plane.
    public SoftBody SplitByPlane(Plane plane)
    {
        var positiveSide = new List<SoftBodyParticle>(Particles.Count);
        var negativeSide = new List<SoftBodyParticle>(Particles.Count);
        
        foreach (var p in Particles)
        {
            if (plane.GetSide(p.Rigidbody.position))
                positiveSide.Add(p);
            else
                negativeSide.Add(p);
        }

        if (positiveSide.Count == 0 || negativeSide.Count == 0)
            return null;

        return Split(negativeSide);
    }


    #region SoftBodyCreator

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


    // Editor code that updates the component according to the changes in the inspector.
#if UNITY_EDITOR
    public bool DEBUG_LiveUpdate = false;
    [MinValue(0)] public int DEBUG_UpdateFrameSkip = 60;


    private Vector3[] _startPositions;
    private Quaternion[] _startRotations;
    private int _updatesLeftForUpdate = 0;


    private void Start()
    {
        if (OnSplit == null)
            OnSplit = new SoftBodySplitEvent();

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

    #endregion SoftBodyCreator
}

