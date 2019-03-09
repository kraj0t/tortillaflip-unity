using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class RigidBodyExtension : MonoBehaviour
{
    [Min(1)] public float Density = 1;
    [Min(0)] public float MaxAngularVelocity = 7;
    //public float MaxDepenetrationVelocity = 10;
    //public float SleepThreshold = 0.001f;
    [Min(0)] public int SolverIterations = 6;
    [Min(0)] public int SolverVelocityIterations = 1;
    [Min(0)] public float SleepThreshold = 0.005f;


    private Rigidbody _rb;
    public Rigidbody Rigidbody { get => _rb ? _rb : _rb = GetComponent<Rigidbody>(); }


    void Start()
    {
        RecalculateMassByDensity();
        Rigidbody.maxAngularVelocity = MaxAngularVelocity;
        //Rigidbody.maxDepenetrationVelocity = MaxDepenetrationVelocity;
        //Rigidbody.sleepThreshold = SleepThreshold;
        Rigidbody.solverIterations = SolverIterations;
        Rigidbody.solverVelocityIterations = SolverVelocityIterations;
        Rigidbody.sleepThreshold = SleepThreshold;
    }


    [Button]
    public void RecalculateMassByDensity()
    {
        Rigidbody.SetDensity(Density);
        Rigidbody.mass = Rigidbody.mass; // Unity bug? Mass seems to not update unless I add this line.
    }
}
