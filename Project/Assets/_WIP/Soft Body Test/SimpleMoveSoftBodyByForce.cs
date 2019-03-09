using NaughtyAttributes;
using UnityEngine;


public class SimpleMoveSoftBodyByForce : MonoBehaviour
{
    [Required]
    public SoftBodyCreator SoftBody;

    public float Force = 10;
    public ForceMode Mode = ForceMode.Acceleration;
    public float Torque = 10;
    //public bool AffectedByMass = false;
    //public float MaxSpeed = 1;
    public float RunMultiplier = 2;


    private Vector3 _inputAccel;
    private Vector3 _inputTorque;


    void Update()
    {
        var horiz = Input.GetAxis("Horizontal");
        float vert = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
        var forward = Input.GetAxis("Vertical");
        var run = Input.GetButton("Fire3");
        var rotate = Input.GetButton("Fire1");

        var multip = run ? RunMultiplier : 1;

        _inputAccel = rotate ? Vector3.zero : RunMultiplier * new Vector3(horiz, vert, forward);
        _inputTorque = !rotate ? Vector3.zero : RunMultiplier * new Vector3(horiz, vert, forward);
    }


    private void FixedUpdate()
    {
        if (!SoftBody || !SoftBody.IsCreated)
            return;

        var accel = _inputAccel * Force;
        var torque = _inputTorque * Torque;

        for (int i = 0; i < SoftBody.Count; i++)
        {
            var particle = SoftBody.Get(i);

            if (particle)
            {
                particle.AddForce(accel, Mode);
                particle.AddTorque(_inputTorque);
            }

            //TODO: add torque
        }
    }
}
