using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Joints are destroyed by Unity after breaking. Additionally, the callback does not include
/// the reference to the Joint. That's why we need this class.
/// Seriously, Unity...
/// </summary>
[Serializable]
public class SoftBodyJointBreakEventData
{
    public readonly SoftBodyParticle JointOwner;
    public readonly SoftBodyParticle JointConnectedBody;
    public readonly float BreakForce;
    public readonly ConfigurableJoint DestroyedJointReference;

    public SoftBodyJointBreakEventData(SoftBodyParticle jointOwner, SoftBodyParticle jointConnectedBody, float breakForce, ConfigurableJoint destroyedJoint)
    {
        JointOwner = jointOwner;
        JointConnectedBody = jointConnectedBody;
        BreakForce = breakForce;
        DestroyedJointReference = destroyedJoint;
    }
}


[Serializable]
public class SoftBodyJointBreakEvent : UnityEvent<SoftBodyJointBreakEventData>
{
}


[Serializable]
public class SoftBodyConnection
{
    [HideInInspector] public SoftBodyParticle OwnerParticle;
    public SoftBodyParticle ConnectedParticle;
    [ReadOnly] public ConfigurableJoint Joint;
    [ReadOnly] public bool HasBeenRegisteredAsBroken;


    public bool isBroken { get => Joint == null; }
}


[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class SoftBodyParticle : MonoBehaviour
{
    public SkinnedMeshRenderer SkinnedRenderer;

    [ReorderableList]
    public List<SoftBodyConnection> Connections = new List<SoftBodyConnection>();

    //xpublic List<SoftBodyParticle> ConnectedParticles = new List<SoftBodyParticle>();
    //x[SerializeField] /*[HideInInspector]*/ [ReadOnly] List<ConfigurableJoint> _jointsByConnectedParticles = new List<ConfigurableJoint>();

    //xprivate List<SoftBodyParticle> _remoteConnections = new List<SoftBodyParticle>();


    public SoftBodyJointBreakEvent OnJointBroken;
    public SoftBodyJointBreakEvent OnMeshChanged;


    //x// We need to cache the state of the Joints (alive/broken) because Unity nulls them after they break.
    //x// This uses the same indices as the ConnectedParticles dictionary.
    //x[SerializeField] [ReadOnly] private bool[] _previousJointStates;
    
    private Mesh _mesh;
    
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

        if (OnJointBroken == null)
            OnJointBroken = new SoftBodyJointBreakEvent();

        //x_previousJointStates = GetCurrentJointStates();

        _mesh = Instantiate<Mesh>(SkinnedRenderer.sharedMesh);
        _mesh.MarkDynamic();
        SkinnedRenderer.sharedMesh = _mesh;

        // TESTING TO SEE IF IT FIXES THE WEIRD OFFSET WHEN REMOVING BONE WEIGHTS:
        SkinnedRenderer.rootBone = this.transform;

        /*
        //////////////////////////////////////////////////////////////////////////////////
        // TEST:
        // TRY REMOVING THE WEIGHTS FOR THE BONES THAT ARE NOT CONNECTED <-- THIS IS NOT GOING TO BE GOOD, YOU NEED TO PRESERVE THE JOINTS AS LONG AS POSSIBLE!
        Debug.Log("TODO: RECORRER LA JERARQUIA DE CONEXIONES PARA VER SI AUN HAY ALGUNA CONEXION INDIRECTA.");
        Debug.Log("TODO: ADEMÁS, NO TE BASES SOLO EN LOS ConnectedParticles LOCALES, LAS CONEXIONES SON RECÍPROCAS!");
        foreach (var bone in SkinnedRenderer.bones)
        {
            if (bone != this.transform)
            {
                var isConnected = false;
                foreach (var conn in ConnectedParticles)
                {
                    if (bone == conn.transform)
                    {
                        isConnected = true;
                    }
                }
                if (!isConnected)
                {
                    DeactivateBoneByReplacingWithDummy(SkinnedRenderer, bone, this.transform);
                }
            }
        }
        // end TEST
        //////////////////////////////////////////////////////////////////////////////////
        ///*/

        /*
        this.OnJointBroken.AddListener(TryRemoveConnectedBoneFromSkin);
        foreach (var boneTransform in SkinnedRenderer.bones)
        {
            var p = boneTransform.GetComponent<SoftBodyParticle>();
            if (p)
                p.OnJointBroken.AddListener(TryRemoveConnectedBoneFromSkin);
        }
        */
    }


    private void OnDestroy()
    {
        DestroyImmediate(_mesh);
    }


    public static bool IsJointBroken(Joint j)
    {
        return j == null;
    }


    public bool IsConnectionBroken(SoftBodyParticle other)
    {
        //xvar i = ConnectedParticles.IndexOf(other);
        //xreturn IsJointBroken(_jointsByConnectedParticles[i]);
        foreach (var c in Connections)
            if (c.ConnectedParticle == other)
                return c.isBroken;
        throw new InvalidOperationException("Queried the state of a connection that does not belong to the SoftBodyParticle!");
    }
    

    public bool IsConnectionBrokenRecursive(SoftBodyParticle other)
    {
        // TODO: if this works, consider removing the _remoteConnections array, as it might be not needed anymore.

        var isBroken = false;

        /*
        IsConnectionBroken
        _remoteConnections
        _jointsByConnectedParticles
        */

        Debug.Log("TODO: RECORRER LA JERARQUIA DE CONEXIONES PARA VER SI AUN HAY ALGUNA CONEXION INDIRECTA.");
        Debug.Log("TODO: ADEMÁS, NO TE BASES SOLO EN LOS ConnectedParticles LOCALES, LAS CONEXIONES SON RECÍPROCAS!");

        /*
        foreach (var bone in SkinnedRenderer.bones)
        {
            if (bone != this.transform)
            {
                var isConnected = false;
                foreach (var conn in ConnectedParticles)
                {
                    if (bone == conn.transform)
                    {
                        isConnected = true;
                    }
                }
                if (!isConnected)
                {
                    DeactivateBoneByReplacingWithDummy(SkinnedRenderer, bone, this.transform);
                }
            }
        }
        */
        return isBroken;
    }


    public void Clear()
    {
        //xRefreshJointsArraySize();

        // Delete any previously existing joint.
        //xfor (int i = 0; i < _jointsByConnectedParticles.Count; i++)
        foreach (var conn in Connections)
        {
            if (conn.Joint == null)
                continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                //xDestroyImmediate(_jointsByConnectedParticles[i]);
                DestroyImmediate(conn.Joint);
            else
#endif
                //xDestroy(_jointsByConnectedParticles[i]);
                Destroy(conn.Joint);

            //x_jointsByConnectedParticles[i] = null;

            //x_remoteConnections.Clear();

            //xConnectedParticles[i].UnregisterRemoteJoint(this);
        }

        //Connections.Clear();
    }


    //xpublic void RegisterJoint(SoftBodyParticle other, ConfigurableJoint j)
    //x{
    //xRefreshJointsArraySize();

    //xvar i = ConnectedParticles.IndexOf(other);
    //x_jointsByConnectedParticles[i] = j;

    //xother.RegisterRemoteJoint(this);


    //x}



    //x
    //xprivate void RegisterRemoteJoint(SoftBodyParticle owner)
    //x{
    //xif (_remoteConnections.IndexOf(owner) == -1)
    //x_remoteConnections.Add(owner);
    //x}


    //xprivate void UnregisterRemoteJoint(SoftBodyParticle owner)
    //x{
    //x_remoteConnections.Remove(owner);
    //x}


    private IEnumerator OnJointBreak(float breakForce)
    //private void OnJointBreak(float breakForce)
    {
        //xvar statesOnBreakFrame = (bool[])_previousJointStates.Clone();
        var wereRegisteredAsBrokenAtBreakFrame = new bool[Connections.Count];
        for (int i = 0; i < wereRegisteredAsBrokenAtBreakFrame.Length; i++)
            wereRegisteredAsBrokenAtBreakFrame[i] = Connections[i].HasBeenRegisteredAsBroken;

        //UnityEditor.EditorApplication.isPaused = true;

        // Apparently, we need to wait one frame to be sure that the joint is destroyed. Sigh...
        yield return null;

        // TODO: modify the mesh (via shader distortion or vertexstreambuffer)

        // NOTE: please Unity, just make this a proper callback and pass the Joint reference...

        // Find out which of the joints broke.
        int iterations = 0;
        var foundIndices = new List<int>();
        while (foundIndices.Count == 0)
        {
            // Reverse traversal for easy removal later if needed.
            //for (int i = Connections.Count - 1; i >= 0; i--)
            for (int i = 0; i < Connections.Count; i++)
            //xfor (int i = 0; i < ConnectedParticles.Count; i++)
            {
                //xvar j = _jointsByConnectedParticles[i];
                var conn = Connections[i];
                //xvar jointState = !IsJointBroken(j);
                if (conn.isBroken && !wereRegisteredAsBrokenAtBreakFrame[i])
                {
                    foundIndices.Add(i);
                    conn.HasBeenRegisteredAsBroken = true;
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
            //xvar p = ConnectedParticles[i];
            var conn = Connections[brokenConnectionIndex];
            Debug.Log("Se ha roto el joint de <b>" + conn.OwnerParticle.name + "</b> con <b>" + conn.ConnectedParticle.name + "</b>, fuerza = <b>" + breakForce.ToString() + "</b>", this);

            // So much work... just for sending what should have been passed as an event argument... -_-
            //xOnJointBroken.Invoke(new SoftBodyJointBreakEventData(this, p, breakForce, _jointsByConnectedParticles[brokenConnectionIndex]));
            OnJointBroken.Invoke(new SoftBodyJointBreakEventData(conn.OwnerParticle, conn.ConnectedParticle, breakForce, conn.Joint));
        }

        //x_previousJointStates = GetCurrentJointStates();
    }


    /*
        private void FixedUpdate()
        {
            // INSTEAD OF LISTENING FOR BROKEN JOINTS, JUST QUERY THEM EVERY FRAME.

            // Check if a joint broke.
            for (int i = 0; i < ConnectedParticles.Count; i++)
            {
                var j = _jointsByConnectedParticles[i];
                var jointState = !IsJointBroken(j);
                if (!jointState && _previousJointStates[i])
                {
                    Debug.Log("ENCONTRADO JOINT ROTO", this);

                    // So much work... just for sending what should have been passed as an event argument... -_-
                    var breakForce = 0f;
                    OnJointBroken.Invoke(new JointBreakEventData(this, ConnectedParticles[i], breakForce, j));
                }
            }

            _previousJointStates = GetCurrentJointStates();
        }
    */

    //xprivate bool[] GetCurrentJointStates()
    //x{
    //xvar states = new bool[ConnectedParticles.Count];
    //x        for (int i = 0; i < ConnectedParticles.Count; i++)
    //xstates[i] = !IsJointBroken(_jointsByConnectedParticles[i]);
    //xreturn states;
    //x}


    //xprivate void OnValidate()
    //x{
    //xRefreshJointsArraySize();
    //x}


    private void OnValidate()
    {
        foreach (var conn in Connections)
            conn.OwnerParticle = this;
    }

    //xprivate void RefreshJointsArraySize()
    //x{
    //xwhile (_jointsByConnectedParticles.Count > ConnectedParticles.Count)
    //x_jointsByConnectedParticles.RemoveAt(_jointsByConnectedParticles.Count - 1);
    //xwhile (_jointsByConnectedParticles.Count != ConnectedParticles.Count)
    //x_jointsByConnectedParticles.Add(null);
    //x}


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

        //xfor (int i = 0; i < ConnectedParticles.Count; i++)
        foreach (var conn in Connections)
        {
            if (!conn.ConnectedParticle)
                continue;

            var AtoB = conn.ConnectedParticle.transform.position - pos;
            var dir = AtoB.normalized;
            var dist = Vector3.Dot(AtoB, dir);

            //xHandles.color = _jointsByConnectedParticles[i] ? connectedColor : Handles.xAxisColor;
            Handles.color = conn.isBroken ? brokenColor : connectedColor;
            Handles.ArrowHandleCap(0, pos, Quaternion.LookRotation(dir), dist*.85f, EventType.Repaint);
        }
    }
#endif // UNITY_EDITOR
}
