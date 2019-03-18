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
/// DISCLAIMER: the 'breakForce' will sometimes not correspond to the joint.
/// 
/// Joints are destroyed by Unity after breaking. Additionally, the callback does not include
/// the reference to the Joint. That's why we need this class.
/// 
/// More info on SoftBodyParticle.OnJointBreak()
/// 
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


/// <summary>
/// DISCLAIMER: the 'breakForce' will sometimes not correspond to the joint.
/// More info about this in SoftBodyParticle.OnJointBreak()
/// </summary>
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
    [ReadOnly] public bool isAlreadyRegisteredAsBroken;


    public bool isBroken { get => Joint == null; }
}


[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class SoftBodyParticle : MonoBehaviour
{
    // TODO: not sure if the renderer should go in another component.

    public SkinnedMeshRenderer SkinnedRenderer;

    [ReorderableList]
    public List<SoftBodyConnection> Connections = new List<SoftBodyConnection>();
    private HashSet<SoftBodyParticle> _connectedParticlesSet;
    public IEnumerable<SoftBodyParticle> ConnectedParticles { get => _connectedParticlesSet; }

    //xpublic List<SoftBodyParticle> ConnectedParticles = new List<SoftBodyParticle>();
    //x[SerializeField] /*[HideInInspector]*/ [ReadOnly] List<ConfigurableJoint> _jointsByConnectedParticles = new List<ConfigurableJoint>();

    //xprivate List<SoftBodyParticle> _remoteConnections = new List<SoftBodyParticle>();


    public SoftBodyJointBreakEvent OnJointBroken;


    // We need to cache the state of the Joints (alive/broken) because Unity nulls them after they break.
    // This uses the same indices as the ConnectedParticles dictionary.
    [SerializeField] [ReadOnly] private bool[] _previousJointStates;//y



    //private Mesh _mesh;

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

        _previousJointStates = GetCurrentJointStates();//y

        //_mesh = Instantiate<Mesh>(SkinnedRenderer.sharedMesh);
        //_mesh.MarkDynamic();
        //SkinnedRenderer.sharedMesh = _mesh;

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
        //DestroyImmediate(_mesh);
    }


    public bool IsConnectedTo(SoftBodyParticle other)
    {
        return _connectedParticlesSet.Contains(other);
    }


    public static bool IsJointBroken(Joint j)
    {
        return j == null;
    }


    public SoftBodyConnection GetConnection(SoftBodyParticle connectedParticle)
    {
        foreach (var conn in Connections)
            if (conn.ConnectedParticle == connectedParticle)
                return conn;
        return null;
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




    //private IEnumerator OnJointBreak(float breakForce)
    private void OnJointBreak(float breakForce)
    {
        // As you can see, this callback does not include the reference to the Joint. Moreover, Joints are 
        // destroyed by Unity after they break, but this happens asynchronously. Therefore, we need to keep track 
        // of the status of our joints, and keep polling them here until we find out that they have just been destroyed.

        // BIG CAVEAT: the 'breakForce' will sometimes not belong to the joint that we find. This is because
        // the destruction of the Joint component happens asynchronously. Therefore, since we are looping 
        // to check the first joint that we detect as newly broken, we cannot be sure that the joint that
        // we find is the joint that Unity invoked this callback for.

        //zStartCoroutine(FindBrokenJoint(breakForce));
    }


    private bool _isFindingBrokenJoint;

    private IEnumerator FindBrokenJoint(float breakForce)
    {
        // Force this coroutine to run one at a time.
        if (_isFindingBrokenJoint)
        {
            Debug.Log("aqui esperando...", this);
            yield return null;
        }
        _isFindingBrokenJoint = true;

        var statesOnBreakFrame = (bool[])_previousJointStates.Clone(); //y
        var wereAlreadyRegisteredAsBrokenAtBreakFrame = new bool[Connections.Count];
        for (int i = 0; i < wereAlreadyRegisteredAsBrokenAtBreakFrame.Length; i++)
            wereAlreadyRegisteredAsBrokenAtBreakFrame[i] = Connections[i].isAlreadyRegisteredAsBroken;

        //UnityEditor.EditorApplication.isPaused = true;

        // As I said, we need to wait *at least* one frame to be sure that the joint is destroyed. Sigh...
        yield return null;

        // TODO: modify the mesh (via shader distortion or vertexstreambuffer)

        // NOTE: please Unity, just make this a proper callback and pass the Joint reference...

        // Find out which of the joints broke.
        var foundIndices = new List<int>();
        //zvar foundIndex = -1;
        int iterations = 0;
        while (foundIndices.Count == 0)
        //zwhile (foundIndex == -1)
        {
            //// Reverse traversal for easy removal later if needed.
            //for (int i = Connections.Count - 1; i >= 0; i--)
            for (int i = 0; i < Connections.Count; i++)
            //xfor (int i = 0; i < ConnectedParticles.Count; i++)
            {
                //xvar j = _jointsByConnectedParticles[i];
                var conn = Connections[i];
                var jointState = !IsJointBroken(conn.Joint);//y
                if (!jointState && statesOnBreakFrame[i]) //y
                //yif (conn.isBroken && !wereAlreadyRegisteredAsBrokenAtBreakFrame[i])
                {
                    foundIndices.Add(i);
                    //zfoundIndex = i;
                    conn.isAlreadyRegisteredAsBroken = true;
                }
            }
            if (foundIndices.Count == 0)
            //zif (foundIndex == -1)
                yield return null;

            iterations++;
            if (iterations > 10)
                Debug.LogError("ALREADY SPENT " + iterations.ToString() + " ITERATIONS LOOKING FOR THE JOINT THAT BROKE", this);
        }

        foreach (var brokenConnectionIndex in foundIndices)
        {
            //xvar p = ConnectedParticles[i];
            var conn = Connections[brokenConnectionIndex];
            //zvar conn = Connections[foundIndex];

            //Debug.Log("Broken joint from <b>" + conn.OwnerParticle.name + "</b> to <b>" + conn.ConnectedParticle.name + "</b>, breakForce <b>" + breakForce.ToString() + "</b>", this);

            // So much work... just for sending what should have been passed as an event argument... -_-
            //xOnJointBroken.Invoke(new SoftBodyJointBreakEventData(this, p, breakForce, _jointsByConnectedParticles[brokenConnectionIndex]));
            OnJointBroken.Invoke(new SoftBodyJointBreakEventData(conn.OwnerParticle, conn.ConnectedParticle, breakForce, conn.Joint));
        }

        _previousJointStates = GetCurrentJointStates();//y

        // Allow a next coroutine to keep working.
        _isFindingBrokenJoint = false;
    }


    private void FixedUpdate()
    {
        // INSTEAD OF LISTENING FOR BROKEN JOINTS, JUST QUERY THEM EVERY FRAME.

        float fakeBreakForce = -1;

        var wereAlreadyRegisteredAsBrokenAtBreakFrame = new bool[Connections.Count];
        for (int i = 0; i < wereAlreadyRegisteredAsBrokenAtBreakFrame.Length; i++)
            wereAlreadyRegisteredAsBrokenAtBreakFrame[i] = Connections[i].isAlreadyRegisteredAsBroken;

        // TODO: modify the mesh (via shader distortion or vertexstreambuffer)

        // NOTE: please Unity, just make this a proper callback and pass the Joint reference...

        // Find out which of the joints broke.
        var foundIndices = new List<int>();
            
        //// Reverse traversal for easy removal later if needed.
        //for (int i = Connections.Count - 1; i >= 0; i--)
        for (int i = 0; i < Connections.Count; i++)
        {
            var conn = Connections[i];
            if (conn.isBroken && !wereAlreadyRegisteredAsBrokenAtBreakFrame[i])
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
            OnJointBroken.Invoke(new SoftBodyJointBreakEventData(conn.OwnerParticle, conn.ConnectedParticle, fakeBreakForce, conn.Joint));
        }
    }

    private bool[] GetCurrentJointStates()//y
    {
        //yvar states = new bool[ConnectedParticles.Count];
        var states = new bool[Connections.Count];
        //yfor (int i = 0; i < ConnectedParticles.Count; i++)
        for (int i = 0; i < Connections.Count; i++)
            //ystates[i] = !IsJointBroken(_jointsByConnectedParticles[i]);
            states[i] = !IsJointBroken(Connections[i].Joint);
        return states;
    }


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
