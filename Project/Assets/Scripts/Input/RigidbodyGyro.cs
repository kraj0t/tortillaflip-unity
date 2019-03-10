using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[ExecuteAfter(typeof(PhysicsManager))]
public class RigidbodyGyro : MonoBehaviour
{
    public Rigidbody Body { get; private set; }

    public Vector3 LocalRotate = new Vector3(0, 0, 180);
    public Vector3 WorldRotate = new Vector3(90, 180, 0);

    public float RotationSmoothness = .5f;
    public float MovementSmoothness = .5f;

    public float MovementMagnitude = 1;

    public float TEST_DividerToLimitAccelerationWhenRotatingFast = 10;


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


    public static Quaternion GetConvertedGyro(Vector3 localRotate, Vector3 worldRotate)
    {
        var gyroRot = Input.gyro.attitude;
        var localAdjust = Quaternion.Euler(localRotate);
        var worldAdjust = Quaternion.Euler(worldRotate);
        return worldAdjust * gyroRot * localAdjust;
    }

    
    void UpdateRotation()
    {
        var targetRot = GetConvertedGyro(LocalRotate, WorldRotate);

        var currentRot = Body.rotation;
        var smoothedRot = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime / RotationSmoothness);

        Body.MoveRotation(smoothedRot);
    }


    private Vector3 Vector3Abs(float x, float y, float z)
    {
        return new Vector3(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(z));
    }


    private Vector3 Vector3AbsClamp01(float x, float y, float z)
    {
        return new Vector3(Mathf.Clamp01(Mathf.Abs(x)), Mathf.Clamp01(Mathf.Abs(y)), Mathf.Clamp01(Mathf.Abs(z)));
    }


    void UpdatePosition()
    {
        var gyroAccel = Input.gyro.userAcceleration;


        // TEST LIMIT ACCEL IF ROTATING FAST
        // Trying to limit accelerometer input when the user is rotating quickly.
        // This is causing the frying pan to jerk forwards unrealistically.

        ////Naive attempt with only one scalar:
        ////var rotSpeed = Input.gyro.rotationRateUnbiased.magnitude;
        ////var accelerationFactor = 1 - (rotSpeed / TEST_DividerToLimitAccelerationWhenRotatingFast);
        ////accelerationFactor = Mathf.Max(0, accelerationFactor);
        ////gyroAccel *= accelerationFactor;

        // Nope. Need to adjust the axes here.
        ////var rotRate = Input.gyro.rotationRateUnbiased;
        ////var factoredRotRate = rotRate / TEST_DividerToLimitAccelerationWhenRotatingFast;
        ////var accelFactors = Vector3.one - new Vector3(Mathf.Clamp01(factoredRotRate.x), Mathf.Clamp01(factoredRotRate.y), Mathf.Clamp01(factoredRotRate.z));
        ////gyroAccel.Scale(accelFactors);

        //var gyroRot = GetConvertedGyro(LocalRotate, WorldRotate);
        //var gyroRot = smoothedRot;
        var gyroRot = Body.rotation;
        var gyroEuler = gyroRot.eulerAngles;
        var gyroFwd = gyroRot * Vector3.forward;
        var rotRate = Input.gyro.rotationRateUnbiased;
        var factoredRotRate = rotRate / TEST_DividerToLimitAccelerationWhenRotatingFast;
        var xAxisFactors = Vector3.one - Vector3Abs(0, gyroFwd.y, gyroFwd.z);
        var yAxisFactors = Vector3.one - Vector3Abs(gyroFwd.x, gyroFwd.y, 0);
        var zAxisFactors = Vector3.one - Vector3Abs(gyroFwd.x, 0, gyroFwd.z);
        var clampedRotRateFactors = Vector3.one - Vector3AbsClamp01(factoredRotRate.x, factoredRotRate.y, factoredRotRate.z);
        //var accelFactors = Vector3.one - new Vector3(Mathf.Clamp01(factoredRotRate.x), Mathf.Clamp01(factoredRotRate.y), Mathf.Clamp01(factoredRotRate.z));
        //var accelFactors = Vector3.Scale(Vector3.Scale(Vector3.Scale(clampedRotRateFactors, xAxisFactors), yAxisFactors), zAxisFactors);
        var x = Vector3.Lerp(xAxisFactors, Vector3.one, clampedRotRateFactors.x);
        var y = Vector3.Lerp(yAxisFactors, Vector3.one, clampedRotRateFactors.y);
        var z = Vector3.Lerp(zAxisFactors, Vector3.one, clampedRotRateFactors.z);
        var accelFactors = Vector3.Scale(Vector3.Scale(Vector3.Scale(clampedRotRateFactors, x), y), z);
        gyroAccel.Scale(accelFactors);
        _accelFactors = accelFactors;
        // end TEST LIMIT ACCEL IF ROTATING FAST



        var originPos = transform.parent ? transform.parent.position : Vector3.zero;
        var localPos = Body.rotation * (gyroAccel * MovementMagnitude);

        var currentPos = Body.position;
        var targetPos = originPos + localPos;
        var smoothedPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime / MovementSmoothness);

        Body.MovePosition(smoothedPos);
    }


    private Vector3 _accelFactors;
#if UNITY_EDITOR
    private int _lastSecond = 0;
    private float _rotAmountMaxLastSecond = 0;
    private float _rotAmountMaxCurrentSecond = 0;
    private Vector3 _accelFactorsDisplay;
    private void OnGUI()
    {
        float rotAmount = Input.gyro.rotationRateUnbiased.magnitude;

        var currentSecond = (int)Time.time;
        if (_lastSecond != currentSecond)
        {
            _lastSecond = currentSecond;
            _rotAmountMaxLastSecond = _rotAmountMaxCurrentSecond;
            _rotAmountMaxCurrentSecond = 0;

            _accelFactorsDisplay = _accelFactors;
        }
        _rotAmountMaxCurrentSecond = Mathf.Max(_rotAmountMaxCurrentSecond, rotAmount);

        GUILayout.Label("rotationRateUnbiased: " + Input.gyro.rotationRateUnbiased.ToString());
        GUILayout.Label("  ^magnitude: " + Input.gyro.rotationRateUnbiased.magnitude.ToString());
        GUILayout.Label("Max. magnitude: " + _rotAmountMaxLastSecond.ToString());
        GUILayout.Label("_accelFactors: " + _accelFactorsDisplay.ToString());
    }
#endif
}