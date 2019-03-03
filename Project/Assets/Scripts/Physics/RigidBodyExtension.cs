using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class RigidBodyExtension : MonoBehaviour
{
    public float Density = 1;
    public float MaxAngularVelocity = 7;
    //public float MaxDepenetrationVelocity = 10;
    //public float SleepThreshold = 0.001f;
    public int SolverIterations = 6;
    public int SolverVelocityIterations = 1;

    public Rigidbody Rigidbody { get; private set; }

    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();

        Rigidbody.SetDensity(Density);
        Rigidbody.mass = Rigidbody.mass; // Unity bug? Mass seems to not update unless I add this line.
        Rigidbody.maxAngularVelocity = MaxAngularVelocity;
        //Rigidbody.maxDepenetrationVelocity = MaxDepenetrationVelocity;
        //Rigidbody.sleepThreshold = SleepThreshold;
        Rigidbody.solverIterations = SolverIterations;
        Rigidbody.solverVelocityIterations = SolverVelocityIterations;
    }
}
