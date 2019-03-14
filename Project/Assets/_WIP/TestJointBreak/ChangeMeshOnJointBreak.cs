using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(SkinnedMeshRenderer))]
public class ChangeMeshOnJointBreak : MonoBehaviour
{
    [Serializable]
    public struct JointCombination
    {
        public Mesh Mesh;
        //public Renderer RendererToEnable;
        public Transform RootBone;

        // TODO: SHOW WHICH ARE THE ORIGINAL BONES OF THE MESH, FOR EASIER EDIT... IF IT IS POSSIBLE.

        public Transform Bone0;
        public Transform Bone1;
        public Transform Bone2;
        public Transform Bone3;

        [Tooltip("False for broken joint")]
        public bool[] RequiredStates;

        // TODO: BUTTON TO SET THIS MESH RIGHT AWAY, GOOD FOR EDITING
    }


    //public SkinnedMeshRenderer Renderer;
    public SkinnedMeshRenderer Renderer { get => GetComponent<SkinnedMeshRenderer>(); }
    public Joint[] Joints;
    public JointCombination[] JointCombinations;


    public UnityEvent OnJointBroken;
    public UnityEvent OnMeshChanged;


    //private bool[] _previousStates;
    private bool[] _currentStates;


    private void OnValidate()
    {
        if (JointCombinations.Length != Mathf.Pow(2, Joints.Length))
        {
            Debug.LogError("You need to define a MeshCombination for every possible combination of the Joints!", this);
        }
        else
        {
            foreach (var combi in JointCombinations)
                if (combi.RequiredStates.Length != Joints.Length)
                    Debug.LogError("In each combination, you need to set the required states for each of the joints!", this);
        }
    }


    private IEnumerator OnJointBreak(float breakForce)
    //private void OnJointBreak(float breakForce)
    {
        Debug.DrawRay(transform.position, transform.forward, Color.red, 1);
        Debug.Log("Se ha roto con fuerza " + breakForce);
        //UnityEditor.EditorApplication.isPaused = true;

        // Apparently, we need to wait one frame to be sure that the joint is destroyed. Sigh...
        yield return null;

        OnJointBroken.Invoke();

        RefreshMesh();

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

    private void Start()
    {
        /*
         * I'M GONNA DO THIS ON THE INSPECTOR... FOR NOW
        // Start listening to the other joints in case they break.
        foreach (var j in Joints)
        {
            if (j.gameObject != this.gameObject)
            {
                foreach (var bhvr in j.gameObject.GetComponents<ChangeMeshOnJointBreak>())
                {
                    bhvr.OnJointBroken.AddListener
                }
            }
        }
        */

        RefreshMesh();

        //_previousStates = _currentStates;
    }


    [Button]
    public void RefreshMesh()
    {
        RefreshCurrentStates();

        var combi = FindCombination(_currentStates);
        
        //if (combi.Mesh != Renderer.sharedMesh)
        {
            // Workaround proposed here to recalculate the renderer.bounds --> https://forum.unity.com/threads/recalculating-renderer-bounds.79081/
            // TODO: find a better solution for ^this, if any.
            Renderer.updateWhenOffscreen = false;
            Renderer.sharedMesh = combi.Mesh;
            Renderer.rootBone = combi.RootBone;
            Renderer.bones = new Transform[] { combi.Bone0, combi.Bone1, combi.Bone2, combi.Bone3 };
            Renderer.updateWhenOffscreen = true;

            OnMeshChanged.Invoke();
        }
    }


    private void RefreshCurrentStates()
    {
        if (_currentStates == null || _currentStates.Length != Joints.Length)
            _currentStates = new bool[Joints.Length];

        for (int i = 0; i < _currentStates.Length; i++)
            _currentStates[i] = Joints[i] != null;
    }


    private JointCombination FindCombination(bool[] states)
    {
        foreach (var combi in JointCombinations)
        {
            var found = true;
            for (int i = 0; i < Joints.Length; i++)
            {
                if (combi.RequiredStates[i] != _currentStates[i])
                {
                    found = false;
                    break;
                }
            }

            if (found)
                return combi;
        }
     
        // TODO: ensure this can never happen.
        
        throw new Exception("No combination found! This should never happen!");
    }
}
