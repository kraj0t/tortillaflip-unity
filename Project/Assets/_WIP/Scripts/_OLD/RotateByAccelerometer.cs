using UnityEngine;


public class RotateByAccelerometer : MonoBehaviour
{
    public float Smoothing = 10;


    private float _startGravityMagnitude;


    void Start()
    {
        _startGravityMagnitude = Physics.gravity.magnitude;

        Vector3 a = GetAccelerometer();
        Vector3 n = a.normalized;
        transform.localRotation = Quaternion.LookRotation( n );
        Physics.gravity = n * _startGravityMagnitude;
    }


    void Update()
    {
        Vector3 a = GetAccelerometer();
        Vector3 n = a.normalized;
        float lerpT = (Smoothing - (0.5f * Smoothing * Mathf.Pow(n.y, 2))) * Time.deltaTime;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.LookRotation( n ), lerpT);
        Physics.gravity = n * _startGravityMagnitude * a.magnitude;

        Debug.DrawRay( transform.position, Physics.gravity * 0.01f, Color.red );
        Debug.DrawRay( transform.position, transform.forward * 0.1f, Color.blue );
    }

    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label( new Rect( 5f, 20f, 300f, 30f ), "Input.acceleration: " + Input.acceleration.ToString() );
    }


    private Vector3 GetAccelerometer()
    {
        var a = Input.acceleration;
        return new Vector3(-a.x, a.y, a.z);
    }
}
