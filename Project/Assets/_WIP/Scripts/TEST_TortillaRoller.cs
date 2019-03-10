using NaughtyAttributes;
using UnityEngine;

[ExecuteAfter(typeof(RigidbodyGyro))]
public class TEST_TortillaRoller : MonoBehaviour
{
    [InfoBox("Quick hack for making the tortilla roll around.")]

    public Rigidbody[] Bodies;

    public float TorqueMultiplier = .01f;


    void FixedUpdate()
    {
        if (Bodies == null || Bodies.Length == 0)
            return;

        var restRot = Quaternion.AngleAxis(0, Vector3.up);
        var gyroRot = GyroInput.GetCorrectedGyro();
        var offsetFromRestRot = restRot * gyroRot;
        var offsetEuler = offsetFromRestRot.eulerAngles;
        offsetEuler.y = 0;

        var b = Bodies[0];
        b.AddTorque(offsetEuler * TorqueMultiplier, ForceMode.Acceleration);
    }


    public void InitFromParentObject(GameObject parent)
    {
        Bodies = parent.GetComponentsInChildren<Rigidbody>();
    }
}
