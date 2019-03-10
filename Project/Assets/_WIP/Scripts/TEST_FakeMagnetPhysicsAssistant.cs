using NaughtyAttributes;
using UnityEngine;

[ExecuteAfter(typeof(RigidbodyGyro))]
public class TEST_FakeMagnetPhysicsAssistant : MonoBehaviour
{
    [InfoBox("Quick hack for making the tortilla toss feel easier and more rewarding.")]
    public Transform Target;

    public Rigidbody[] Bodies;

    //public Vector3 WorldAxesMultipliers = new Vector3(1, 0, 1);

    //public float MovementSmoothness = 2;

    //public float TEST_AssistedAccel = 1;

    public float TEST_UpSpeedForMinimumEffect = .1f;
    public float TEST_UpSpeedForMaximumEffect = 3;

    public float TEST_ThresholdDistance = 0.1f;
    public float TEST_DistanceForMaxEffect = 0.5f;

    public float TEST_MoveMultiplier = 1;


    void FixedUpdate()
    {
        /*
        foreach (var b in Bodies)
        {
            var currentPos = b.position;
            var targetPos = Target.position;

            var upSpeedFactor = Mathf.Max(0, b.velocity.y) / TEST_UpSpeedForMaximumEffect;

            var smoothedPos = Vector3.Lerp(currentPos, targetPos, upSpeedFactor * Time.deltaTime / MovementSmoothness);
            b.MovePosition(smoothedPos);
        }
        */

        if (Bodies == null || Bodies.Length == 0)
            return;

        var firstBody = Bodies[0];
        var currentPos = firstBody.position;
        var targetPos = Target.position;

        var upSpeedFactor = Mathf.Max(0, firstBody.velocity.y - TEST_UpSpeedForMinimumEffect) / TEST_UpSpeedForMaximumEffect;
        upSpeedFactor = Mathf.Clamp01(upSpeedFactor);


        var toTarget = targetPos - currentPos;
        var dir = toTarget.normalized;
        var dist = Vector3.Dot(toTarget, dir);
        var distanceFactor = Mathf.Max(0, dist - TEST_ThresholdDistance) / TEST_DistanceForMaxEffect;
        distanceFactor = Mathf.Clamp01(distanceFactor);

        if (Mathf.Approximately(0, distanceFactor * upSpeedFactor))
            return;

        //var accel = dir * (distanceFactor * upSpeedFactor * TEST_AssistedAccel);

        var moveStep = dir * (Time.deltaTime * distanceFactor * upSpeedFactor * TEST_MoveMultiplier);

        foreach (var b in Bodies)
        {
            //b.AddForce(accel, ForceMode.Acceleration);
            b.MovePosition(b.position + moveStep);
        }
    }


    public void InitFromParentObject(GameObject parent)
    {
        Bodies = parent.GetComponentsInChildren<Rigidbody>();
    }
}
