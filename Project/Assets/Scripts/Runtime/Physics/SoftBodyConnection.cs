using NaughtyAttributes;
using System;
using UnityEngine;


[Serializable]
public class SoftBodyConnection
{
    [HideInInspector] public SoftBodyParticle OwnerParticle;
    public SoftBodyParticle ConnectedParticle;
    [ReadOnly] public ConfigurableJoint Joint;
    [ReadOnly] public bool isAlreadyRegisteredAsBroken;


    public bool IsBroken { get => Joint == null; }


    // Returns true if the line between the two connected particles intersects with the given plane.
    public bool IntersectsWithPlane(Plane plane)
    {
        var aPos = OwnerParticle.Rigidbody.position;
        var bPos = ConnectedParticle.Rigidbody.position;

        //plane.SameSide()
        //Physics.inter

        return false;
    }
}

