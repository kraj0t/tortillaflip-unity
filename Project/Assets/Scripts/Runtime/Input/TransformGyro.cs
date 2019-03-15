using UnityEngine;


public class TransformGyro : MonoBehaviour
{
    public Transform Transform { get; private set; }

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
        Transform = transform;
    }

    
    void UpdateRotation()
    {
        var targetRot = GyroInput.GetCorrectedGyro();

        var currentRot = Transform.rotation;
        var smoothedRot = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime / RotationSmoothness);

        Transform.rotation = smoothedRot;
    }


    void UpdatePosition()
    {
        var gyroAccel = Input.gyro.userAcceleration;

        var originPos = transform.parent ? transform.parent.position : Vector3.zero;
        var localPos = Transform.rotation * (gyroAccel * MovementMagnitude);

        var currentPos = Transform.position;
        var targetPos = originPos + localPos;
        var smoothedPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime / MovementSmoothness);

        Transform.position = smoothedPos;
    }
}