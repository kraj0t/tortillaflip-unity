using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
[AddComponentMenu("Tortilla Flip/Physics/Rigidbody Extension", -1000)]
public class RigidbodyExtension : MonoBehaviour
{
    [Required]
    public RigidbodyExtensionProfile Profile;

    public bool ApplyProfileOnStart = true;


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


    void OnDrawGizmosSelected()
    {
        const float kBoundsToGizmoFactor = .05f;
        var gizmoRadius = .01f;

        // Gather the bounds of all the colliders, to render the center of mass in a proper size.
        var cols = GetComponentsInChildren<Collider>();
        if (cols.Length > 0)
        {
            var totalBounds = cols[0].bounds;
            for (int i = 1; i < cols.Length; i++)
                totalBounds.Encapsulate(cols[i].bounds);
            gizmoRadius = kBoundsToGizmoFactor * Mathf.Max(totalBounds.size.x, totalBounds.size.y, totalBounds.size.z);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(ActualWorldCenterOfMass, gizmoRadius);
    }
}