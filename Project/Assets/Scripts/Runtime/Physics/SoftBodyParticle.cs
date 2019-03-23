using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif


// DISCLAIMER: because of a bug in Unity's OnJointBreak(), the breakForce cannot be used.
[Serializable]
public class SoftBodyJointBreakEventData
{
    public readonly SoftBodyParticle JointOwner;
    public readonly SoftBodyParticle JointConnectedBody;
    //public readonly float BreakForce;
    public readonly ConfigurableJoint DestroyedJointReference;

    public SoftBodyJointBreakEventData(SoftBodyParticle jointOwner, SoftBodyParticle jointConnectedBody, /*float breakForce,*/ ConfigurableJoint destroyedJoint)
    {
        JointOwner = jointOwner;
        JointConnectedBody = jointConnectedBody;
        //BreakForce = breakForce;
        DestroyedJointReference = destroyedJoint;
    }
}


/// <summary>
/// DISCLAIMER: the 'breakForce' will sometimes not correspond to the joint.
/// More info about this in SoftBodyParticle.OnJointBreak()
/// </summary>
[Serializable]
public class SoftBodyJointBreakEvent : UnityEvent<SoftBodyJointBreakEventData>
{
}


[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class SoftBodyParticle : MonoBehaviour
{
    // TODO: refactor this class, renderer should go in another component.

    public SkinnedMeshRenderer SkinnedRenderer;

    [ReorderableList]
    public List<SoftBodyConnection> Connections = new List<SoftBodyConnection>();
    private HashSet<SoftBodyParticle> _connectedParticlesSet;

    public IEnumerable<SoftBodyParticle> ConnectedParticles
    {
        get
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                _connectedParticlesSet = new HashSet<SoftBodyParticle>();
                foreach (var conn in Connections)
                    _connectedParticlesSet.Add(conn.ConnectedParticle);
            }
#endif
            return _connectedParticlesSet;
        }
    }

    public bool CheckForBrokenJoints = true;

    public SoftBodyJointBreakEvent OnJointBroken;


    private Rigidbody _rb;
    public Rigidbody Rigidbody
    {
#if UNITY_EDITOR
        get => _rb ? _rb : _rb = GetComponent<Rigidbody>();
#else
        get => _rb;
#endif
    }


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        _connectedParticlesSet = new HashSet<SoftBodyParticle>();
        foreach (var conn in Connections)
            _connectedParticlesSet.Add(conn.ConnectedParticle);

        if (OnJointBroken == null)
            OnJointBroken = new SoftBodyJointBreakEvent();

        if (SkinnedRenderer)
            SkinnedRenderer.rootBone = this.transform;
    }


    public bool IsConnectedTo(SoftBodyParticle other)
    {
        return _connectedParticlesSet.Contains(other);
    }


    public static bool IsJointBroken(Joint j)
    {
        return j == null;
    }


    // Searches locally (only connections that this particle owns) and returns it if found.
    public SoftBodyConnection GetConnection(SoftBodyParticle connectedParticle)
    {
        foreach (var conn in Connections)
            if (conn.ConnectedParticle == connectedParticle)
                return conn;
        return null;
    }


    public void Clear()
    {
        // Delete any previously existing joint.
        foreach (var conn in Connections)
        {
            if (conn.Joint == null)
                continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(conn.Joint);
            else
#endif
                Destroy(conn.Joint);
        }
    }


    /* Leaving this here in case Unity decides to fix OnJointBreak()
    private bool _isFindingBrokenJoint;

    private IEnumerator OnJointBreak(float breakForce)
    {
        // As you can see, this callback does not receive the reference to the Joint that broke. Moreover, Joints 
        // are destroyed by Unity after they break, but this happens asynchronously. Therefore, we need to keep track 
        // of the status of our joints, and keep polling them here until we find out that they have just been destroyed.

        // BIG CAVEAT: the 'breakForce' will sometimes not belong to the joint that we find. This is because
        // the destruction of the Joint component happens asynchronously. Therefore, since we are looping 
        // to check the first joint that we detect as newly broken, we cannot be sure that the joint that
        // we find is the joint that Unity invoked this callback for.

        // Force this coroutine to run one at a time.
        if (_isFindingBrokenJoint)
            yield return null;
        _isFindingBrokenJoint = true;

        var wereAlreadyRegisteredAsBrokenAtBreakFrame = new bool[Connections.Count];
        for (int i = 0; i < wereAlreadyRegisteredAsBrokenAtBreakFrame.Length; i++)
            wereAlreadyRegisteredAsBrokenAtBreakFrame[i] = Connections[i].isAlreadyRegisteredAsBroken;

        // As I said, we need to wait *at least* one frame to be sure that the joint is destroyed. Sigh...
        yield return null;

        // Find out which of the joints broke.
        var foundIndices = new List<int>();
        int iterations = 0;
        while (foundIndices.Count == 0)
        {
            // Reverse traversal for easy removal later if needed.
            for (int i = Connections.Count - 1; i >= 0; i--)
            {
                var conn = Connections[i];
                var jointState = !IsJointBroken(conn.Joint);
                if (conn.isBroken && !wereAlreadyRegisteredAsBrokenAtBreakFrame[i])
                {
                    foundIndices.Add(i);
                    conn.isAlreadyRegisteredAsBroken = true;
                }
            }
            if (foundIndices.Count == 0)
                yield return null;

            iterations++;
            if (iterations > 10)
                Debug.LogError("ALREADY SPENT " + iterations.ToString() + " ITERATIONS LOOKING FOR THE JOINT THAT BROKE", this);
        }

        foreach (var brokenConnectionIndex in foundIndices)
        {
            var conn = Connections[brokenConnectionIndex];

            //Debug.Log("Broken joint from <b>" + conn.OwnerParticle.name + "</b> to <b>" + conn.ConnectedParticle.name + "</b>, breakForce <b>" + breakForce.ToString() + "</b>", this);

            // So much work... just for sending what should have been passed as an event argument... -_-
            OnJointBroken.Invoke(new SoftBodyJointBreakEventData(conn.OwnerParticle, conn.ConnectedParticle, breakForce, conn.Joint));
        }

        // Allow a next coroutine to keep working.
        _isFindingBrokenJoint = false;
    }
    */


    private void FixedUpdate()
    {
        if (!CheckForBrokenJoints)
            return;

        var wereAlreadyRegisteredAsBrokenAtBreakFrame = new bool[Connections.Count];
        for (int i = 0; i < wereAlreadyRegisteredAsBrokenAtBreakFrame.Length; i++)
            wereAlreadyRegisteredAsBrokenAtBreakFrame[i] = Connections[i].isAlreadyRegisteredAsBroken;

        // Find out which of the joints broke.
        var foundIndices = new List<int>();
            
        // Reverse traversal for easy removal later if needed.
        for (int i = Connections.Count - 1; i >= 0; i--)
        {
            var conn = Connections[i];
            if (conn.IsBroken && !wereAlreadyRegisteredAsBrokenAtBreakFrame[i])
            {
                foundIndices.Add(i);
                conn.isAlreadyRegisteredAsBroken = true;
            }
        }

        foreach (var brokenConnectionIndex in foundIndices)
        {
            var conn = Connections[brokenConnectionIndex];

            //Debug.Log("Broken joint from <b>" + conn.OwnerParticle.name + "</b> to <b>" + conn.ConnectedParticle.name + "</b>, breakForce <b>" + breakForce.ToString() + "</b>", this);

            // So much work... just for sending what should have been passed as an event argument... -_-
            OnJointBroken.Invoke(new SoftBodyJointBreakEventData(conn.OwnerParticle, conn.ConnectedParticle, conn.Joint));
        }
    }


    private void OnValidate()
    {
        foreach (var conn in Connections)
            conn.OwnerParticle = this;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DoDrawGizmos(new Color(.1f, .8f, .4f, .2f), new Color(1, 0, 0, .2f));
    }

    private void OnDrawGizmosSelected()
    {
        DoDrawGizmos(new Color(.1f, .8f, .4f, 1), new Color(1, 0, 0, 1));
    }

    private void DoDrawGizmos(Color connectedColor, Color brokenColor)
    {
        Handles.lighting = false;

        var pos = transform.position;

        foreach (var conn in Connections)
        {
            if (!conn.ConnectedParticle)
                continue;

            var AtoB = conn.ConnectedParticle.transform.position - pos;
            var dir = AtoB.normalized;
            var dist = Vector3.Dot(AtoB, dir);

            Handles.color = conn.IsBroken ? brokenColor : connectedColor;
            Handles.ArrowHandleCap(0, pos, Quaternion.LookRotation(dir), dist*.85f, EventType.Repaint);
        }
    }
#endif // UNITY_EDITOR
}
