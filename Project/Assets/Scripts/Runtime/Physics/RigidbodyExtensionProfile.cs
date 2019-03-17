using NaughtyAttributes;
using UnityEngine;


[CreateAssetMenu(fileName = "MyRigidbodyExtensionProfile", menuName = "WIP/Physics/Rigidbody Extension Profile", order = -1000)]
public class RigidbodyExtensionProfile : ScriptableObject
{
    [BoxGroup("Extended values")] [Min(0)] public float MaxAngularVelocity = 7;
    [BoxGroup("Extended values")] [Min(0)] public int SolverIterations = 6;
    [BoxGroup("Extended values")] [Min(0)] public int SolverVelocityIterations = 1;
    [BoxGroup("Extended values")] [Min(0)] public float SleepThreshold = 0.005f;
    [BoxGroup("Extended values")] [Min(0)] public float MaxDepenetrationVelocity = 1e+32f;

    [Space(20)]
    [BoxGroup("Extended values")]
    [Label("Detect Collisions (RUNTIME ONLY)")]
    public bool DetectCollisions = true;

    [BoxGroup("Mass by density")] public bool OverrideMass = false;
    [InfoBox("Density is measured in kg/m3. Take into account that overlapping colliders will add more mass. Also, often colliders are scaled bigger than needed, in which case a lower density should be used.\n\nSome common densities: cork=240, pine=373, oak=710, olive oil=913, water=997, plastic=1175, limestone=1450, rubber=1522, concrete=2400, iron=7870, gold=19320", visibleIf:"OverrideMass")]
    [BoxGroup("Mass by density")] [EnableIf("OverrideMass")] [Min(1)] public float Density = 1450;

    [BoxGroup("Center of mass")] public bool OverrideCenterOfMass = false;
    [BoxGroup("Center of mass")] [EnableIf("OverrideCenterOfMass")] [Label("Center Of Mass (Local)")] public Vector3 CenterOfMass;

    [BoxGroup("Inertia tensor (per-axis angular damp)")] public bool OverrideInertiaTensor = false;
    [BoxGroup("Inertia tensor (per-axis angular damp)")] [EnableIf("OverrideInertiaTensor")] public Vector3 InertiaTensor = new Vector3(.1f, .1f, .1f);
    [BoxGroup("Inertia tensor (per-axis angular damp)")] [EnableIf("OverrideInertiaTensor")] public Vector3 InertiaTensorRotation;


    public void Apply(Rigidbody rb)
    {
        // TODO: WHEN CREATING A RigidbodyExtensionProfile, ITS VALUES SHOULD BE COPIED FROM THE PHYSICS SETTINGS!

        // TODO: SHOW SOME WIDGET FOR EDITING THE INERTIA TENSOR AND CENTER OF MASS

        rb.maxAngularVelocity = MaxAngularVelocity;
        rb.solverIterations = SolverIterations;
        rb.solverVelocityIterations = SolverVelocityIterations;
        rb.sleepThreshold = SleepThreshold;

        rb.maxDepenetrationVelocity = MaxDepenetrationVelocity;

        rb.detectCollisions = DetectCollisions;

        // Inertia tensor depends on the mass and the center of mass, so we calculate those first.
        if (OverrideMass)
            SetDensityBugfix(rb);
        if (OverrideCenterOfMass)
            rb.centerOfMass = CenterOfMass;
        else
            rb.ResetCenterOfMass();

        // Inertia tensor.
        if (OverrideInertiaTensor)
        {
            rb.inertiaTensor = InertiaTensor;
            rb.inertiaTensorRotation = Quaternion.Euler(InertiaTensorRotation);
        }
        else
        {
            rb.ResetInertiaTensor();
        }
    }


    public void SetDensityBugfix(Rigidbody rb)
    {
        rb.SetDensity(Density);

        // Unity bug? Mass does not get updated unless I add this line. Found at the forums.
        rb.mass = rb.mass;
    }
}
