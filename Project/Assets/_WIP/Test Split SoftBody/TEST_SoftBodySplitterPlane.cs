using NaughtyAttributes;
using UnityEngine;

public class TEST_SoftBodySplitterPlane : MonoBehaviour
{
    public SoftBody Target;

    public float SplitForce = 1f;


    [Button("Do the split!")]
    public bool DoTheSplit()
    {
        var plane = new Plane(transform.forward, transform.position);

        var splitSoftBody = Target.SplitByPlane(plane);
        if (splitSoftBody == null)
            return false;

        // Cut the mesh
        Debug.Log("TODO: cut the mesh in two!");

        // Unweigh the bones of the other soft body (SHOULD BE DONE SOMEWHERE ELSE)
        Debug.Log("TODO: unweigh the bones after splitting a softbody!");

        // Apply inverted forces to each side.
        foreach (var p in Target.Particles)
            p.Rigidbody.AddForce(plane.normal * SplitForce);
        foreach (var p in splitSoftBody.Particles)
            p.Rigidbody.AddForce(-plane.normal * SplitForce);

        return true;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.color = new Color(.6f, .6f, .9f, .1f);
        UnityEditor.Handles.DrawSolidDisc(transform.position, transform.forward, .25f);
    }
#endif
}
