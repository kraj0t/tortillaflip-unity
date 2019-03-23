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
}

