using UnityEngine;

public class GyroDebug : MonoBehaviour
{
    public bool DrawGyro = true;
    public bool UseGyroToUnity = false;

    public Vector3 LocalRotate = new Vector3(0, 0, 180);
    public Vector3 WorldRotate = new Vector3(90, 180, 0);

    public float MovementOffset = 0.2f;


    private Vector3 _startPos;


    private void Start()
    {
        if (Application.isPlaying)
            _startPos = transform.position;
    }


    private void Update()
    {
        var localAdjust = Quaternion.Euler(LocalRotate);
        var worldAdjust = Quaternion.Euler(WorldRotate);

        var gyroAccel = Input.gyro.userAcceleration;
        var gyroToUnity = UseGyroToUnity ? GyroToUnity(Quaternion.identity) : Quaternion.identity;
        //var adjustedGyroAccel = worldAdjust * (localAdjust * (gyroToUnity * gyroAccel));
        var adjustedGyroAccel = gyroToUnity * gyroAccel;

        var q = DrawGyro ? (UseGyroToUnity ? GyroToUnity(Input.gyro.attitude) : Input.gyro.attitude) : transform.rotation;

        transform.rotation = worldAdjust * q * localAdjust;
        transform.position = _startPos + transform.rotation * (adjustedGyroAccel * MovementOffset);
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        UnityEditor.Handles.PositionHandle(transform.position, transform.rotation);
    }
#endif

    private void OnGUI()
    {
        GUI.Label(new Rect(10,10,100,20), Input.deviceOrientation.ToString());
        GUI.Label(new Rect(10,30,100,20), Screen.orientation.ToString());
    }


    // The Gyroscope is right-handed.  Unity is left handed.
    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }

}
