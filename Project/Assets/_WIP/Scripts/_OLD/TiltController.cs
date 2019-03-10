using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
    using UnityEditor;
#endif

public class TiltController : MonoBehaviour
{
    public bool applyAcceleration = true;
    public bool applyRotation = true;

    public Vector3 rotationOffset;
    
    [Header( "GYROSCOPE" )]
    public bool allowGyroscopeInput = true;
    public float gyroscopeAccelerationSensitivity = 1f;
    public float gyroscopeAccelerationThreshold = 0.1f;

    [ Header( "ACCELEROMETER" )]
    public bool allowAccelerometerInput = true;
    public float accelerometerSensitivity = 1f;

    [Header( "MOUSE" )]
    public bool allowMouseInput = true;
    public float mouseSensitivity = 1f;
    public float mouseWheelSensitivity = 1f;

    [Header("UNUSED PARAMETERS")]
    [Tooltip( "A higher value 'stiffens' the tilt input, so that the user does not need to tilt the device so much." )]
    public float tiltStiffness = 1f;

    [Tooltip( "Controls how much the transform will roll (rotation in the Z axis)." )]
    public float tiltRollFactor = 0.2f;

    [Tooltip( "Tweak this to reduce accelerometer jitter. Try not to use this to smooth paddle speed. Use OrbitController.smoothingSpeed for that." )]
    [Range( 0.01f, 1f )]
    public float tiltJitterSmoothingFactor = 0.5f;

    [Tooltip( "Tweak this to set the base angle for the tilt input. Usually, users will hold the phone on their hand and look down on it. So, this value helps you accomodate for that." )]
    public float baseTiltDownfaceAngle = 45f;


    [ReadOnly] public Vector3 currentInputAcceleration;
    [ReadOnly] public Quaternion currentInputRotation;


//    private OrbitController m_orbit;
    private Vector3 m_smoothedTiltAccel;
    private Transform m_transform;


    #region MONOBEHAVIOUR METHODS
    private void Awake()
    {
        m_transform = transform;
    }


    void Start()
    {
        //m_orbit = GetComponent<OrbitController>();
        m_smoothedTiltAccel = Input.acceleration;

        Input.gyro.enabled = true;

        // TO DO : move the Input handling to three different classes.
    }


    void Update()
    {
        // TODO: usar SystemInfo.supports* para usar el gyro si es posible. 
        // En este enlace dicen que en Android hay que setear dichos booleanos a true manualmente: https://feedback.unity3d.com/suggestions/clarify-how-a-device-should-be-checked-if-available
        if ( allowGyroscopeInput && SystemInfo.supportsGyroscope ) {
            _UpdateWithGyro();
        }


#if UNITY_EDITOR        
        else if ( allowAccelerometerInput && ( UnityEditor.EditorApplication.isRemoteConnected || SystemInfo.supportsAccelerometer || SystemInfo.deviceType == DeviceType.Handheld || Application.isMobilePlatform ) ) {            
#else
        else if ( allowAccelerometerInput && (Application.isEditor || SystemInfo.supportsAccelerometer || SystemInfo.deviceType == DeviceType.Handheld || Application.isMobilePlatform) ) {
#endif
            _UpdateWithAccelerometer();
        }

        else if ( allowMouseInput ) {
            _UpdateWithMouse();
        }

        else {
            currentInputAcceleration = Vector3.zero;
            currentInputRotation = Quaternion.identity;
        }

        var rigidBody = GetComponent<Rigidbody>();
        if ( applyAcceleration ) {
            //m_transform.Translate( Camera.main.transform.rotation * currentInputAcceleration, m_transform.parent );
            rigidBody.MovePosition(rigidBody.position + currentInputAcceleration);
        }

        if ( applyRotation ) {
            var rot = Quaternion.Euler(rotationOffset) * currentInputRotation;
            // m_transform.localRotation = rot;
            rigidBody.MoveRotation(rot);
        }
    }


#if UNITY_EDITOR
    void OnGUI()
    {
        float yPos = 0f;
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "m_currentInputAcceleration: " + currentInputAcceleration.ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "m_currentInputRotation: " + currentInputRotation.eulerAngles.ToString() );
        GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Input.gyro.userAcceleration: " + Input.gyro.userAcceleration.ToString() );
        GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Input.gyro.attitude.eulerAngles: " + Input.gyro.attitude.eulerAngles.ToString() );
        GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Input.acceleration: " + Input.acceleration.ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "m_smoothedTiltAccel: " + m_smoothedTiltAccel.ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "mouse X: " + Input.GetAxis( "Mouse X" ).ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "mouse Y: " + Input.GetAxis( "Mouse Y" ).ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Horizontal: " + Input.GetAxis( "Horizontal" ).ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Fire1: " + Input.GetAxis( "Fire1" ).ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Jump: " + Input.GetAxis( "Jump" ).ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Submit: " + Input.GetAxis( "Submit" ).ToString() );
        //GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Cancel: " + Input.GetAxis( "Cancel" ).ToString() );
        GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Input.gyro.rotationRate: " + Input.gyro.rotationRate.ToString() );
        GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Input.gyro.rotationRateUnbiased: " + Input.gyro.rotationRateUnbiased.ToString() );
        GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Screen.orientation: " + Screen.orientation.ToString() );
        GUI.Label( new Rect( 5f, yPos += 15f, 400f, 30f ), "Input.deviceOrientation: " + Input.deviceOrientation.ToString() );
    }
#endif
    #endregion


    #region INPUT METHODS
    void _UpdateWithGyro()
    {
        Vector3 accel = Input.gyro.userAcceleration * gyroscopeAccelerationSensitivity;
        if ( Input.gyro.userAcceleration.magnitude > gyroscopeAccelerationThreshold )
            //m_currentInputAcceleration = accel;
            currentInputAcceleration = new Vector3( -accel.x, -accel.y, accel.z );
        else
            currentInputAcceleration = Vector3.zero;



        
        //Quaternion gyroRot = GyroToUnity_FromUnityDoc( Input.gyro.attitude );
        //Quaternion gyroRot = Input.gyro.attitude;
        //Quaternion gyroRot = GyroToUnity( Input.gyro.attitude );
        Quaternion gyroRot = GyroToUnity_CustomTrialAndErrorForPortrait( Input.gyro.attitude );
        currentInputRotation = gyroRot;
    }


    void _UpdateWithAccelerometer()
    {
        m_smoothedTiltAccel = Vector3.Lerp( m_smoothedTiltAccel, Input.acceleration, Time.deltaTime / tiltJitterSmoothingFactor );

        Vector3 tiltDir = m_smoothedTiltAccel.normalized;

        Vector3 translatedTiltDir = AccelerometerToUnity(tiltDir);

        // This line sets the base tilt to be a bit facedown.
        translatedTiltDir = Quaternion.Euler( baseTiltDownfaceAngle, 0f, 0f ) * translatedTiltDir;

        Vector3 tiltAfterSensitivity = ( translatedTiltDir + Vector3.forward * tiltStiffness ).normalized;

        // Using RotateTowards() ensures correct wrapping of radians.
        //        m_orbit.desiredDirection = Vector3.RotateTowards( m_orbit.desiredDirection, tiltAfterSensitivity, Mathf.Infinity, Mathf.Infinity );

        // This line tilts the camera with the device tilt.
        //        m_orbit.orbitBinormalRotation = Quaternion.Euler( 0f, 0f, -tiltAfterSensitivity.x * 360f * tiltRollFactor );

        //Vector3 accel = Input.acceleration * accelerometerSensitivity;
        //Vector3 userAcceleration = Input.acceleration.normalized;
        Vector3 userAcceleration = AccelerometerToUnity( Input.acceleration.normalized );
        Vector3 actualAccel = userAcceleration * accelerometerSensitivity;
        currentInputAcceleration = actualAccel;

        //currentInputRotation = Quaternion.Euler( Input.acceleration.normalized * Mathf.Rad2Deg );
        //currentInputRotation = GyroToUnity_FromUnityDoc( currentInputRotation);
        currentInputRotation = Quaternion.Euler( AccelerometerToUnity( Input.acceleration.normalized ) * Mathf.Rad2Deg );
    }


    void _UpdateWithMouse()
    {
        float mouseX = Input.GetAxis( "Mouse X" ) * mouseSensitivity;
        float mouseY = Input.GetAxis( "Mouse Y" ) * mouseSensitivity;
        float mouseZ = Input.GetAxis( "Mouse ScrollWheel" ) * mouseWheelSensitivity;

        Quaternion deltaRot = Quaternion.Euler( mouseX, mouseY, 0f );
        //        Vector3 newDir = deltaRot * m_orbit.desiredDirection;

        // Using RotateTowards() ensures correct wrapping of radians [0,360]
        //        m_orbit.desiredDirection = Vector3.RotateTowards( m_orbit.desiredDirection, newDir, Mathf.Infinity, Mathf.Infinity );

        currentInputAcceleration = new Vector3( mouseX, mouseY, mouseZ );

        // TO DO : update rotation with mouse or whatever
        //m_currentInputRotation
    }
    #endregion






    #region HELPER STATIC METHODS
    // The Gyroscope is right-handed.  Unity is left handed.
    private static Quaternion GyroToUnity_FromUnityDoc( Quaternion q )
    {
        return new Quaternion( q.x, q.y, -q.z, -q.w );
    }


    private static Quaternion GyroToUnity_CustomTrialAndErrorForPortrait(Quaternion q)
    {
        var g = Input.gyro.attitude.eulerAngles;
        return Quaternion.Euler(-g.y, -g.z, g.x);
        //return new Quaternion(q.y, q.z, -q.x, -q.w);
    }
    

    private static Quaternion GyroToUnity(Quaternion q)
    {
        Quaternion remapQuaternion = Quaternion.identity;
        Quaternion screenOrientationRemapQuaternion = Quaternion.identity;

#if UNITY_EDITOR
        if (EditorApplication.isRemoteConnected)
        {
            remapQuaternion = Quaternion.Euler(90, 90, 0);
            if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft)
            {
                screenOrientationRemapQuaternion = new Quaternion(0, 0, 0.7071f, 0.7071f);
            }
            else if (Input.deviceOrientation == DeviceOrientation.LandscapeRight)
            {
                screenOrientationRemapQuaternion = new Quaternion(0, 0, -0.7071f, 0.7071f);
            }
            else if (Input.deviceOrientation == DeviceOrientation.Portrait)
            {
                screenOrientationRemapQuaternion = new Quaternion(0, 0, 1, 0);
            }
            else if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown)
            {
                screenOrientationRemapQuaternion = new Quaternion(0, 0, 0, 1);
            }
            else if (Input.deviceOrientation == DeviceOrientation.FaceUp)
            {
                screenOrientationRemapQuaternion = new Quaternion(0, 1, 0, 0);
            }
            else if (Input.deviceOrientation == DeviceOrientation.FaceDown)
            {
                screenOrientationRemapQuaternion = new Quaternion(0, -1, 0, 0);
            }
        }
#elif UNITY_IPHONE
        remapQuaternion = Quaternion.Euler(90, 90, 0);
        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            screenOrientationRemapQuaternion = new Quaternion(0, 0, 0.7071, 0.7071);
        }
        else if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            screenOrientationRemapQuaternion = new Quaternion(0, 0, -0.7071, 0.7071);
        }
        else if (Screen.orientation == ScreenOrientation.Portrait)
        {
            screenOrientationRemapQuaternion = new Quaternion(0, 0, 1, 0);
        }
        else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            screenOrientationRemapQuaternion = new Quaternion(0, 0, 0, 1);
        }
#elif UNITY_ANDROID
        remapQuaternion = Quaternion.Euler(90, 90, 0);
        if (Screen.orientation == ScreenOrientation.LandscapeLeft)
        {
            screenOrientationRemapQuaternion = new Quaternion(0, 0, 0.7071f, -0.7071f);
        }
        else if (Screen.orientation == ScreenOrientation.LandscapeRight)
        {
            screenOrientationRemapQuaternion = new Quaternion(0, 0, -0.7071f, -0.7071f);
        }
        else if (Screen.orientation == ScreenOrientation.Portrait)
        {
            screenOrientationRemapQuaternion = new Quaternion(0, 0, 0, 1);
        }
        else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
        {
            screenOrientationRemapQuaternion = new Quaternion(0, 0, 1, 0);
        }
#endif
        return remapQuaternion * q * screenOrientationRemapQuaternion;
    }


    private static Vector3 AccelerometerToUnity( Vector3 v )
    {
        return new Vector3( -v.x, -v.z, -v.y );
    }

        
    private static Quaternion AccelerometerToUnity(Quaternion q)
    {
        return Quaternion.identity;
    }
    #endregion
}
