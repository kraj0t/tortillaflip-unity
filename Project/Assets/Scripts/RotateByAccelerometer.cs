using UnityEngine;


public class RotateByAccelerometer : MonoBehaviour
{
    public float smoothSpeed = 4f;

    public float shakeImpulseFactor = 0.01f;

    public bool rotateGravity = true;

    public Vector3 tiltFixEuler;


    private float m_startGravityMagnitude;


    void Start()
    {
        m_startGravityMagnitude = Physics.gravity.magnitude;

        Vector3 a = Input.acceleration;
        Vector3 n = a.normalized;
        Vector3 fixedNormalizedDir = new Vector3( -n.x, n.y, n.z );
        transform.localRotation = Quaternion.LookRotation( fixedNormalizedDir );
        Physics.gravity = fixedNormalizedDir * m_startGravityMagnitude;
    }


    void Update()
    {
        Vector3 a = Input.acceleration;
        Vector3 n = a.normalized;

        Vector3 fixedNormalizedDir = new Vector3( -n.x, n.y, n.z );
        float lerpT = (smoothSpeed - (0.5f * smoothSpeed * Mathf.Pow(n.y, 2f))) * Time.deltaTime;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.LookRotation( fixedNormalizedDir ), lerpT);
        Physics.gravity = fixedNormalizedDir * m_startGravityMagnitude * a.magnitude;

        Debug.DrawRay( transform.position, Physics.gravity * 0.01f, Color.red );
        Debug.DrawRay( transform.position, transform.forward * 0.1f, Color.blue );
    }

    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.Label( new Rect( 5f, 20f, 300f, 30f ), "Input.acceleration: " + Input.acceleration.ToString() );
    }
}
