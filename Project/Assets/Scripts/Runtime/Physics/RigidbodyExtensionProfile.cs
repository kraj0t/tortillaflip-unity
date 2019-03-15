using NaughtyAttributes;
using UnityEngine;


[CreateAssetMenu(fileName = "MyRigidbodyExtensionProfile", menuName = "WIP/Physics/Rigidbody Extension Profile", order = -1000)]
public class RigidbodyExtensionProfile : ScriptableObject
{
    [InfoBox("Density is measured in kg/m3. Some common densities: cork=240, pine=373, oak=710, olive oil=913, water=997, plastic=1175, limestone=1450, rubber=1522, concrete=2400, iron=7870, gold=19320")]
    [Min(1)] public float Density = 1450;

    [Space(20)]
    [Min(0)] public float MaxAngularVelocity = 7;
    [Min(0)] public int SolverIterations = 6;
    [Min(0)] public int SolverVelocityIterations = 1;
    [Min(0)] public float SleepThreshold = 0.005f;

    [Space(20)]
    public bool OverrideCenterOfMass = false;
    [EnableIf("OverrideCenterOfMass")] [Label("Center Of Mass (Local)")] public Vector3 CenterOfMass;

    [Space(20)]
    public bool OverrideInertiaTensor = false;
    [EnableIf("OverrideInertiaTensor")] public Vector3 InertiaTensor = new Vector3(.1f, .1f, .1f);
    [EnableIf("OverrideInertiaTensor")] public Vector3 InertiaTensorRotation;

    [Space(20)]
    [Min(0)] public float MaxDepenetrationVelocity = 1e+32f;

    [Space(20)]
    [Label("Detect Collisions (REALTIME ONLY)")]
    public bool DetectCollisions = true;


    public void Apply(Rigidbody rb)
    {
        Debug.Log("WHEN CREATING A RigidbodyExtensionProfile, ITS VALUES SHOULD BE COPIED FROM THE PHYSICS SETTINGS!");
        Debug.Log("SHOW SOME WIDGET FOR EDITING THE INERTIA TENSOR AND CENTER OF MASS");

        rb.maxAngularVelocity = MaxAngularVelocity;
        rb.solverIterations = SolverIterations;
        rb.solverVelocityIterations = SolverVelocityIterations;
        rb.sleepThreshold = SleepThreshold;

        rb.maxDepenetrationVelocity = MaxDepenetrationVelocity;

        rb.detectCollisions = DetectCollisions;

        // Inertia tensor depends on the mass and the center of mass, so we calculate those first.
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
