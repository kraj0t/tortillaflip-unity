using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class RigidBodyExtension : MonoBehaviour
{
    [Required]
    public RigidbodyExtensionProfile Profile;

    public bool ApplyProfileOnStart = false;


    #region Inspector read-only properties to debug the actual current values of the Rigidbody
    [ShowNativeProperty] public float ActualMaxAngularVelocity { get => Rigidbody.maxAngularVelocity; }
    [ShowNativeProperty] public int ActualSolverIterations { get => Rigidbody.solverIterations; }
    [ShowNativeProperty] public int ActualSolverVelocityIterations { get => Rigidbody.solverVelocityIterations; }
    [ShowNativeProperty] public float ActualSleepThreshold { get => Rigidbody.sleepThreshold; }

    [ShowNativeProperty] public Vector3 ActualInertiaTensor { get => Rigidbody.inertiaTensor; }
    [ShowNativeProperty] public Vector3 ActualInertiaTensorRotation { get => Rigidbody.inertiaTensorRotation.eulerAngles; }

    [ShowNativeProperty] public Vector3 ActualCenterOfMass { get => Rigidbody.centerOfMass; }
    [ShowNativeProperty] public Vector3 ActualWorldCenterOfMass { get => Rigidbody.worldCenterOfMass; }

    [ShowNativeProperty] public float ActualMaxDepenetrationVelocity { get => Rigidbody.maxDepenetrationVelocity; }
    #endregion Inspector read-only properties


    public Rigidbody Rigidbody { get => GetComponent<Rigidbody>(); }


    void Start()
    {
        if (ApplyProfileOnStart)
            ApplyProfile();
    }


    [Button("Apply Profile now!")]
    public void ApplyProfile()
    {
        Profile.Apply(GetComponent<Rigidbody>());
    }
}