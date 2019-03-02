using UnityEngine;
using System.Collections;


[RequireComponent( typeof( Rigidbody ) )]
public class RollBallExample : MonoBehaviour
{

    public float speed;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Input.gyro.enabled = true;
    }

    void FixedUpdate()
    {
        //float moveH = Input.gyro.userAcceleration.x;
        float moveH = Input.acceleration.x;
        //float moveV = Input.gyro.userAcceleration.y;
        float moveV = Input.acceleration.z; 

        Vector3 movement = new Vector3( moveH, 0.0f, moveV );

        rb.AddForce( movement * speed * Time.deltaTime );

        /*
        if ( SystemInfo.deviceType == DeviceType.Desktop ) {
            float moveHorizontal = Input.GetAxis( "Horizontal" );
            float moveVertical = Input.GetAxis( "Vertical" );

            Vector3 movement = new Vector3( moveHorizontal, 0.0f, moveVertical );

            rb.AddForce( movement * speed );
        } else {
            float moveH = Input.gyro.userAcceleration.x;
            float moveV = Input.gyro.userAcceleration.y;

            Vector3 movement = new Vector3( moveH, 0.0f, moveV );

            rb.AddForce( movement * speed * Time.deltaTime );
        }
        */
    }


}