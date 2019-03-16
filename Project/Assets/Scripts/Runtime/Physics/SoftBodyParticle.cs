using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SkinnedMeshRenderer))]
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
        public readonly ConfigurableJoint DestroyedJoint;

        public JointBreakEventData(SoftBodyParticle jointOwner, SoftBodyParticle jointConnectedBody, float breakForce, ConfigurableJoint destroyedJoint)
        {
            JointOwner = jointOwner;
            JointConnectedBody = jointConnectedBody;
            BreakForce = breakForce;
            DestroyedJoint = destroyedJoint;
        }
    }


    [Serializable]
    public class JointBreakEvent : UnityEvent<JointBreakEventData>
    {
    }



    [Serializable]
    public class ParticleJointDictionary : SerializableDictionary<SoftBodyParticle, ConfigurableJoint> { }
    public ParticleJointDictionary ConnectedParticlesAndJoints;


    public JointBreakEvent OnJointBroken;
    public JointBreakEvent OnMeshChanged;


    // We need to cache references to the Joints because Unity nulls them after they break.
    // This uses the same indices as the ConnectedParticles array.
    private List<ConfigurableJoint> _joints;


    private Rigidbody _rb;
    public Rigidbody Rigidbody
    {
#if UNITY_EDITOR
        get => _rb ? _rb : _rb = GetComponent<Rigidbody>();
#else
        get => _rb;
#endif
    }


    private SkinnedMeshRenderer _skin;
    public SkinnedMeshRenderer Skin
    {
#if UNITY_EDITOR
        get => _skin ? _skin : _skin = GetComponent<SkinnedMeshRenderer>();
#else
        get => _skin;
#endif
    }


    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _skin = GetComponent<SkinnedMeshRenderer>();

        foreach (var pair in ConnectedParticlesAndJoints)
            pair.Key.OnJointBroken.AddListener(TryRemoveConnectedBoneFromSkin);
    }


    public void RegisterJoint(SoftBodyParticle other, ConfigurableJoint j)
    {
        ConnectedParticlesAndJoints[other] = j;
    }


    // TODO: this could go in another component, which should handle the renderer.
    private void TryRemoveConnectedBoneFromSkin(JointBreakEventData e)
    {
        var possibleBone = e.JointOwner.transform;
        for (int i = 0; i < Skin.bones.Length; i++)
            if (Skin.bones[i] == possibleBone)
                Skin.bones[i] = null;
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
        Debug.DrawRay(transform.position, transform.forward, Color.red, 1);
        Debug.Log("Se ha roto con fuerza " + breakForce);
        //UnityEditor.EditorApplication.isPaused = true;

        // Apparently, we need to wait one frame to be sure that the joint is destroyed. Sigh...
        yield return null;




        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// AQUIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //OnJointBroken.Invoke(new JointBreakEventData(this, ));







        ////////////        RefreshMesh();
        ///



        //   - remove the bone from the skinned mesh renderer
        //   - modify the mesh (shader or vertexstream)


        /*
         ***********
         * CODE THAT WOULD ALLOW US TO KNOW EXACTLY WHICH JOINT BROKE:
         * (I DO NOT NEED THIS RIGHT NOW, BUT I LEAVE THIS CODE JUST IN CASE, WE MIGHT WANT PARTICLE FX)
         * 
        // NOTE: please Unity, just make this a proper callback and pass the Joint reference...

        

        // Check which of the joint(s) broke.
        _previousStates
        // Emit particles in the direction of the break, in the direction of the joint's axis.
        // Would need to have cached the directions, because we can't have access to the joint at this point.
        //if (joint == null)
        //{
        // ...
        //}
        */
    }


    private void OnDrawGizmos()
    {
        DoDrawGizmos(new Color(1, 1, 0, .7f));
    }

    private void OnDrawGizmosSelected()
    {
        DoDrawGizmos(Color.green);
    }

    private void DoDrawGizmos(Color col)
    {
        var pos = Rigidbody.position;

        Gizmos.color = col;
        foreach (var conn in ConnectedParticlesAndJoints.Keys)
            Gizmos.DrawLine(pos, conn.Rigidbody.position);
    }
}
