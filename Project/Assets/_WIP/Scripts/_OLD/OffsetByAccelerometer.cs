using UnityEngine;

public class OffsetByAccelerometer : MonoBehaviour
{
    public float Multiplier = 1;

    [Tooltip("Tweak this to reduce accelerometer jitter.")]

    [Min(0.01f)]
    public float JitterSmoothingFactor = 1;

    public Vector3 Rotation;
    
    private Vector3 _smoothAccel;


    private void Start()
    {
        //_smoothAccel = Input.acceleration;
        _smoothAccel = GetGyroAccel();
    }


    void Update_WITH_ACCELEROMETER()
    {
        _smoothAccel = Vector3.Lerp( _smoothAccel, Input.acceleration, Time.deltaTime / JitterSmoothingFactor);
        var rigidBody = GetComponent<Rigidbody>();
        var rot = Quaternion.Euler(Rotation);
        var parentPos = transform.parent ? transform.parent.position : Vector3.zero;
        var localPos = rot * (_smoothAccel * Multiplier);
        rigidBody.MovePosition(parentPos + localPos);
    }


    private void Update()
    {
        _smoothAccel = Vector3.Lerp( _smoothAccel, GetGyroAccel(), Time.deltaTime / JitterSmoothingFactor);

        var rigidBody = GetComponent<Rigidbody>();
        var rot = Quaternion.Euler(Rotation);
        var parentPos = transform.parent ? transform.parent.position : Vector3.zero;
        var localPos = rot * (_smoothAccel * Multiplier);
        rigidBody.MovePosition(parentPos + localPos);
    }


    private Vector3 GetGyroAccel()
    {
        Vector3 accel = Input.gyro.userAcceleration;
//Euler(-g.y, -g.z, g.x);
        //return new Vector3(-accel.x, -accel.y, accel.z);
        return new Vector3(accel.y, -accel.z, -accel.x);
    }
}
