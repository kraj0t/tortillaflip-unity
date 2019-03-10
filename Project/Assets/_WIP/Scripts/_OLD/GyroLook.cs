using UnityEngine;

public class GyroLook : MonoBehaviour
{
    public Vector3 LocalRotate = new Vector3(0, 0, 180);
    public Vector3 WorldRotate = new Vector3(90, 180, 0);

    public float RotationSmoothness = .5f;
    public float MovementSmoothness = .5f;

    public float MovementMagnitude = 1;


    private float _initialYAngle = 0f;
    private float _appliedGyroYAngle = 0f;
    private float _calibrationYAngle = 0f;


    void FixedUpdate()
    {
        ApplyGyroRotation();
        ApplyCalibration();
        ApplyAcceleration();
    }
    void Start()
    {
        Input.gyro.enabled = true;
        //Application.targetFrameRate = 60;
    }
    /*void OnGUI()
    {
        if (GUILayout.Button("Calibrate", GUILayout.Width(300), GUILayout.Height(100)))
        {
            CalibrateYAngle();
        }
    }*/
    public void CalibrateYAngle()
    {
        _calibrationYAngle = _appliedGyroYAngle - _initialYAngle; // Offsets y angle in case it wasn't
    }




    void ApplyGyroRotation()
    {
        var gyroRot = Input.gyro.attitude;
        var localAdjust = Quaternion.Euler(LocalRotate);
        var worldAdjust = Quaternion.Euler(WorldRotate);
        var targetRot = worldAdjust * gyroRot * localAdjust;

        /*transform.rotation = Input.gyro.attitude;
        transform.Rotate(LocalRotate, Space.Self); //Swap "handedness" ofquaternionfromgyro.
        transform.Rotate(WorldRotate, Space.World); //Rotate to make sense as a camera pointing out the back
        appliedGyroYAngle = transform.eulerAngles.y; // Save the angle around y axis for use in calibration.*/

        var rigidBody = GetComponent<Rigidbody>();

        var currentRot = rigidBody.rotation;
        var smoothedRot = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime / RotationSmoothness);

        //rigidBody.MoveRotation(transform.rotation);
        rigidBody.MoveRotation(smoothedRot);


        //appliedGyroYAngle = rot.eulerAngles.y;
    }

    void ApplyCalibration()
    {
        //transform.Rotate(0f, -calibrationYAngle, 0f, Space.World); // Rotates y angle back however much it deviated
    }

    void ApplyAcceleration()
    {
        var localAdjust = Quaternion.Euler(LocalRotate);
        var worldAdjust = Quaternion.Euler(WorldRotate);
        //var gyro = Input.gyro.attitude;
        //var targetRot = worldAdjust * gyro * localAdjust;
        var gyroAccel = Input.gyro.userAcceleration;
        //var adjustedGyroAccel = (worldAdjust * localAdjust) * gyroAccel;
        var adjustedGyroAccel = worldAdjust * (localAdjust * gyroAccel);

        var rigidBody = GetComponent<Rigidbody>();
        var originPos = transform.parent ? transform.parent.position : Vector3.zero;
        //var localPos = adjustedGyroAccel * MovementMagnitude;
        var localPos = rigidBody.rotation * (gyroAccel * MovementMagnitude);

        var currentPos = rigidBody.position;
        var targetPos = originPos + localPos;
        var smoothedPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime / MovementSmoothness);

        rigidBody.MovePosition(smoothedPos);
    }
}