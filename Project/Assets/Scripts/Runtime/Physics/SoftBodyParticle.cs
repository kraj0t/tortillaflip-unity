using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif


[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class SoftBodyParticle : MonoBehaviour
{
    /// <summary>
    /// Joints are destroyed by Unity after breaking. Additionally, the callback does not include
    /// the reference to the Joint. That's why we need this class.
    /// Seriously, Unity...
    /// </summary>
    [Serializable]
    public class JointBreakEventData
    {
        public readonly SoftBodyParticle JointOwner;
        public readonly SoftBodyParticle JointConnectedBody;
        public readonly float BreakForce;
        public readonly ConfigurableJoint DestroyedJointReference;

        public JointBreakEventData(SoftBodyParticle jointOwner, SoftBodyParticle jointConnectedBody, float breakForce, ConfigurableJoint destroyedJoint)
        {
            JointOwner = jointOwner;
            JointConnectedBody = jointConnectedBody;
            BreakForce = breakForce;
            DestroyedJointReference = destroyedJoint;
        }
    }


    [Serializable]
    public class JointBreakEvent : UnityEvent<JointBreakEventData>
    {
    }


    public SkinnedMeshRenderer SkinnedRenderer;


    public List<SoftBodyParticle> ConnectedParticles = new List<SoftBodyParticle>();
    [SerializeField] /*[HideInInspector]*/ [ReadOnly] List<ConfigurableJoint> _jointsByConnectedParticles = new List<ConfigurableJoint>();

    private List<SoftBodyParticle> _remoteConnections = new List<SoftBodyParticle>();


    public JointBreakEvent OnJointBroken;
    public JointBreakEvent OnMeshChanged;


    // We need to cache the state of the Joints (alive/broken) because Unity nulls them after they break.
    // This uses the same indices as the ConnectedParticles dictionary.
    [SerializeField] [ReadOnly] private bool[] _previousJointStates;
    
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
            OnJointBroken = new JointBreakEvent();

        _previousJointStates = GetCurrentJointStates();

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
                    RemoveSkinBone_DUMMY_BINDPOSE(SkinnedRenderer, bone, this.transform);
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
        var i = ConnectedParticles.IndexOf(other);
        return IsJointBroken(_jointsByConnectedParticles[i]);
    }
    

    public bool IsConnectionBrokenRecursive(SoftBodyParticle other)
    {
        var isBroken = false;

        /*
        IsConnectionBroken
        _remoteConnections
        _jointsByConnectedParticles
        */

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
                    RemoveSkinBone_DUMMY_BINDPOSE(SkinnedRenderer, bone, this.transform);
                }
            }
        }

        return isBroken;
    }


    public void Clear()
    {
        RefreshJointsArraySize();

        // Delete any previously existing joint.
        for (int i = 0; i < _jointsByConnectedParticles.Count; i++)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(_jointsByConnectedParticles[i]);
            else
                Destroy(_jointsByConnectedParticles[i]);
#else
                Destroy(_jointsByConnectedParticles[i]);
#endif

            _jointsByConnectedParticles[i] = null;

            _remoteConnections.Clear();

            ConnectedParticles[i].UnregisterRemoteJoint(this);
        }
    }


    public void RegisterJoint(SoftBodyParticle other, ConfigurableJoint j)
    {
        RefreshJointsArraySize();

        var i = ConnectedParticles.IndexOf(other);
        _jointsByConnectedParticles[i] = j;

        other.RegisterRemoteJoint(this);
    }


    private void RegisterRemoteJoint(SoftBodyParticle owner)
    {
        if (_remoteConnections.IndexOf(owner) == -1)
            _remoteConnections.Add(owner);
    }


    private void UnregisterRemoteJoint(SoftBodyParticle owner)
    {
        _remoteConnections.Remove(owner);
    }


    // TODO: this could go in another component, which should handle the renderer.
    private void TryRemoveConnectedBoneFromSkin(JointBreakEventData e)
    {
        if (e.JointOwner != this && e.JointConnectedBody != this)
            return;

        var possibleBone = e.JointOwner == this ? e.JointConnectedBody.transform : e.JointOwner.transform;
        if (RemoveSkinBone_DUMMY_BINDPOSE(SkinnedRenderer, possibleBone, this.transform))
            Debug.Log("Removed bone " + possibleBone.name + " from skin " + SkinnedRenderer.name + " and particle " + this.name);
    }


    // Returns false if the bone is not influencing the skin and there is no need to remove it.
    public static bool RemoveSkinBone_DUMMY_BINDPOSE(SkinnedMeshRenderer skin, Transform bone, Transform parentBone)
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


    // Returns false if the bone is not influencing the skin.
    public static bool RemoveSkinBone(SkinnedMeshRenderer skin, Transform bone)
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


    /*
    [Button("Recreate Joints")]
    public void RecreateJoints()
    {
        // Register to joint breaks events.
        for (int i = 0; i < ConnectedParticles.Length; i ++)
        {
            var p = ConnectedParticles[i];

        }
    }
    */


    private IEnumerator OnJointBreak(float breakForce)
    //private void OnJointBreak(float breakForce)
    {
        var statesOnFrameBreak = (bool[])_previousJointStates.Clone();

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
            for (int i = 0; i < ConnectedParticles.Count; i++)
            {
                var j = _jointsByConnectedParticles[i];
                var jointState = !IsJointBroken(j);
                if (!jointState && statesOnFrameBreak[i])
                    foundIndices.Add(i);
            }
            if (foundIndices.Count == 0)
                yield return null;

            iterations++;
            if (iterations > 10)
                Debug.LogError("ALREADY SPENT " + iterations.ToString() + " ITERATIONS LOOKING FOR THE JOINT THAT BROKE", this);
        }

        foreach (var i in foundIndices)
        {
            var p = ConnectedParticles[i];
            Debug.Log("Se ha roto el joint de <b>" + this.name + "</b> con <b>" + p.name + "</b>, fuerza = <b>" + breakForce.ToString() + "</b>", this);

            // So much work... just for sending what should have been passed as an event argument... -_-
            OnJointBroken.Invoke(new JointBreakEventData(this, p, breakForce, _jointsByConnectedParticles[i]));
        }

        _previousJointStates = GetCurrentJointStates();
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

    private bool[] GetCurrentJointStates()
    {
        var states = new bool[ConnectedParticles.Count];
        for (int i = 0; i < ConnectedParticles.Count; i++)
            states[i] = !IsJointBroken(_jointsByConnectedParticles[i]);
        return states;
    }


    private void OnValidate()
    {
        RefreshJointsArraySize();
        
    }

    private void RefreshJointsArraySize()
    {
        while (_jointsByConnectedParticles.Count > ConnectedParticles.Count)
            _jointsByConnectedParticles.RemoveAt(_jointsByConnectedParticles.Count - 1);
        while (_jointsByConnectedParticles.Count != ConnectedParticles.Count)
            _jointsByConnectedParticles.Add(null);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        DoDrawGizmos(new Color(.3f, .3f, 1, .3f), new Color(1, 0, 0, .3f));
    }

    private void OnDrawGizmosSelected()
    {
        DoDrawGizmos(new Color(.3f, .3f, 1, 1), new Color(1, 0, 0, 1));
    }

    private void DoDrawGizmos(Color connectedColor, Color brokenColor)
    {
        Handles.lighting = false;

        var pos = Rigidbody.position;

        for (int i = 0; i < ConnectedParticles.Count; i++)
        {
            var AtoB = ConnectedParticles[i].Rigidbody.position - pos;
            var dir = AtoB.normalized;
            var dist = Vector3.Dot(AtoB, dir);

            Handles.color = _jointsByConnectedParticles[i] ? connectedColor : Handles.xAxisColor;
            Handles.ArrowHandleCap(0, pos, Quaternion.LookRotation(dir), dist*.85f, EventType.Repaint);
        }
    }
#endif // UNITY_EDITOR
}
