using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidbodyGyro : MonoBehaviour
{
    public Rigidbody Body { get; private set; }

    public Vector3 LocalRotate = new Vector3(0, 0, 180);
    public Vector3 WorldRotate = new Vector3(90, 180, 0);

    public float RotationSmoothness = .5f;
    public float MovementSmoothness = .5f;

    public float MovementMagnitude = 1;


    void FixedUpdate()
    {
        UpdateRotation();
        UpdatePosition();
    }


    void Start()
    {
        Body = GetComponent<Rigidbody>();
        Input.gyro.enabled = true;
    }

    
    void UpdateRotation()
    {
        var gyroRot = Input.gyro.attitude;
        var localAdjust = Quaternion.Euler(LocalRotate);
        var worldAdjust = Quaternion.Euler(WorldRotate);
        var targetRot = worldAdjust * gyroRot * localAdjust;

        var currentRot = Body.rotation;
        var smoothedRot = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime / RotationSmoothness);

        Body.MoveRotation(smoothedRot);
    }


    void UpdatePosition()
    {
        var gyroAccel = Input.gyro.userAcceleration;

        var originPos = transform.parent ? transform.parent.position : Vector3.zero;
        var localPos = Body.rotation * (gyroAccel * MovementMagnitude);

        var currentPos = Body.position;
        var targetPos = originPos + localPos;
        var smoothedPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime / MovementSmoothness);

        Body.MovePosition(smoothedPos);
    }
}