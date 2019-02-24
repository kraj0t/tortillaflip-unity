using UnityEngine;
using System.Collections;


public class RotateWorldByAccelerometer : MonoBehaviour
{
    public float smoothSpeed = 4f;

    public float shakeImpulseFactor = 0.01f;

    public bool rotateGravity = true;

    public Vector3 tiltFixEuler;




    private float m_startGravityMagnitude;


    void Start()
    {
        m_startGravityMagnitude = Physics.gravity.magnitude;

        /*
        Quaternion tiltFix = Quaternion.Euler(tiltFixEuler);
        Vector3 rawDeviceEuler = Input.acceleration.normalized * 180f;
        //Vector3 rawDeviceEuler = Input.gyro.attitude.eulerAngles;
        Vector3 fixedEuler = new Vector3(rawDeviceEuler.x, -rawDeviceEuler.z, rawDeviceEuler.y);
        Quaternion fixedRot = tiltFix * Quaternion.Euler(fixedEuler);
        //transform.localRotation = fixedRot;
        */

        Vector3 a = Input.acceleration;
        Vector3 n = a.normalized;
        Vector3 fixedNormalizedDir = new Vector3( -n.x, n.y, n.z );
        transform.localRotation = Quaternion.LookRotation( fixedNormalizedDir );
        Physics.gravity = fixedNormalizedDir * m_startGravityMagnitude;
    }


    void Update()
    {
        Input.gyro.enabled = true;

        Vector3 a = Input.acceleration;
        Vector3 n = a.normalized;

        /*
        //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(Input.acceleration.normalized * 180f), 8f * Time.deltaTime);
        //transform.rotation = Quaternion.Lerp(transform.rotation, Input.gyro.attitude, 8f * Time.deltaTime);
        Quaternion tiltFix = Quaternion.Euler(tiltFixEuler);
        Vector3 rawDeviceEuler = Input.acceleration.normalized * 180f;
        Vector3 fixedEuler = new Vector3( rawDeviceEuler.z, rawDeviceEuler.y, rawDeviceEuler.x );
        //Vector3 rawDeviceEuler = Input.gyro.attitude.eulerAngles;
        //Vector3 fixedEuler = new Vector3(-rawDeviceEuler.x, rawDeviceEuler.z, rawDeviceEuler.y);
        Quaternion fixedRot = tiltFix * Quaternion.Euler(fixedEuler);
        Quaternion smoothedRot = Quaternion.Lerp(transform.localRotation, fixedRot, smoothSpeed * Time.deltaTime);

        //Physics.gravity = ( smoothedRot * Vector3.down ) * m_startGravityMagnitude;        
        Physics.gravity = new Vector3(-a.x, a.y, a.z) * m_startGravityMagnitude;

        //Physics.gravity = (Input.gyro.attitude * Vector3.down) * m_startGravityMagnitude;
        */
        
        Vector3 fixedNormalizedDir = new Vector3( -n.x, n.y, n.z );
        //float lerpT = smoothSpeed * Time.deltaTime;
        // TEST: the following line should alleviate the accelerometer jitter when it is in full vertical portrait mode.
        float lerpT = (smoothSpeed - (0.5f * smoothSpeed * Mathf.Pow(n.y, 2f))) * Time.deltaTime;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.LookRotation( fixedNormalizedDir ), lerpT);
        //Physics.gravity = Vector3.Lerp(Physics.gravity, fixedNormalizedDir * m_startGravityMagnitude, lerpT );
        //Physics.gravity = fixedNormalizedDir * m_startGravityMagnitude;
        Physics.gravity = fixedNormalizedDir * m_startGravityMagnitude * a.magnitude;

        /*
        //transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.LookRotation( n ), 8f * Time.deltaTime);
        //transform.localRotation = Input.gyro.attitude;
        Vector3 rawGyroEuler = Input.gyro.attitude.eulerAngles;
        Vector3 fixedGyroEuler = new Vector3( -rawGyroEuler.y, rawGyroEuler.x, rawGyroEuler.z);
        //fixedGyroEuler = -fixedGyroEuler;
        //transform.localRotation = Quaternion.Euler(fixedGyroEuler);
        */

        Debug.DrawRay( transform.position, Physics.gravity * 0.01f, Color.red );
        Debug.DrawRay( transform.position, transform.forward * 0.1f, Color.blue );
    }


    /*
    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label( new Rect( 5f, 20f, 300f, 30f ), "Input.acceleration: " + Input.acceleration.ToString() );
        GUI.Label( new Rect( 5f, 35f, 300f, 30f ), "Input.gyro.attitude: " + Input.gyro.attitude.eulerAngles.ToString() );
    }
    */
}
