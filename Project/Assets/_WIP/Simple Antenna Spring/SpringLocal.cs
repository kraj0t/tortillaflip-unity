using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringLocal : MonoBehaviour
{
    public Transform Target;

    public float Drag;
    public float SpringForce;
    public Rigidbody SpringRB;


    private Vector3 LocalDistance;//Distance between the two points
    private Vector3 LocalVelocity;//Velocity converted to local space

    private void FixedUpdate()
    {
        //Calculate the distance between the two points
        LocalDistance = Target.InverseTransformDirection(Target.position - SpringRB.position);
        SpringRB.AddRelativeForce(LocalDistance * SpringForce);//Apply Spring

        //Calculate the local velocity of the SpringObj point
        LocalVelocity = SpringRB.transform.InverseTransformDirection(SpringRB.velocity);
        SpringRB.AddRelativeForce(-LocalVelocity * Drag);//Apply Drag
    }
}
