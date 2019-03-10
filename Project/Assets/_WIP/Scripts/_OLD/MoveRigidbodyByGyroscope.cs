using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class MoveRigidbodyByGyroscope : MonoBehaviour
{
    public Vector3 forceVec = Vector3.one;
    public float forceFloat = 1f;

    public float magnitudeThreshold = 0.5f;
    public float magnitudeMax = 2f;
    public float dampingPower = 2f;

    public bool onlyWhenTouching = false;
    public bool useSpeed = true;

    public Slider sliderX;


    private Rigidbody _rb;

    private Vector3 _currentAccel;


    void Start()
    {
        _rb = GetComponent<Rigidbody>();

        Input.compensateSensors = true;

        Input.gyro.updateInterval = 0.01f;
    }


    void Update()
    {
        Vector3 accel = Input.gyro.userAcceleration;
        //accel = new Vector3( -accel.x, -accel.y, accel.z );



        // Threshold and max.
        if ( accel.magnitude < magnitudeThreshold )
            accel = Vector3.zero;
        else if ( accel.magnitude > magnitudeMax ) {
            Debug.Log( "ABOVE MAXIMUM: " + accel.magnitude );
            accel = accel.normalized * magnitudeMax;
        }



        // Enhance big movements, filter out small movements.        
        float magnitudeNormalizedForPow = Mathf.Max(0, accel.magnitude - magnitudeThreshold) / (magnitudeMax - magnitudeThreshold);
        accel = accel * Mathf.Pow( magnitudeNormalizedForPow, dampingPower );



        if ( sliderX ) {
            float xNormalized = accel.x / magnitudeMax;
            sliderX.normalizedValue = xNormalized * 0.5f + 0.5f;
        }



        accel = Vector3.Scale( accel, forceVec ) * forceFloat;


        // Make the movement related to the camera view.
        //accel = Camera.main.transform.rotation * accel;



        //_rb.AddForce( Input.gyro.userAcceleration.x * forceVec, ForceMode.Force );
        //_rb.AddForce( Input.gyro.userAcceleration.x * forceVec, ForceMode.VelocityChange );

        _currentAccel = accel;
    }


    void FixedUpdate()
    {
        if ( !onlyWhenTouching || ( onlyWhenTouching && Input.touchCount != 0 ) ) {
            if ( useSpeed )
                _rb.AddForce( _currentAccel );
            //_rb.velocity += _currentAccel * Time.fixedDeltaTime;
            //_rb.AddForce( _currentAccel, ForceMode.VelocityChange );
            else
                _rb.position += _currentAccel * Time.fixedDeltaTime * 0.1f;
        }
    }
}