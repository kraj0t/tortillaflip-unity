using UnityEngine;

public class GyroInput : MonoBehaviour
{
    public static GyroInput Instance { get; private set; }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple " + this + " in scene!");
            Destroy(this);
        }
    }

    public Vector3 LocalRotate = new Vector3(0, 0, 180);
    public Vector3 WorldRotate = new Vector3(90, 180, 0);


    void Start()
    {
        Input.gyro.enabled = true;
    }


    public static Quaternion GetCorrectedGyro()
    {
        return GetCorrectedGyro(Instance.LocalRotate, Instance.WorldRotate);
    }


    private static Quaternion GetCorrectedGyro(Vector3 localRotate, Vector3 worldRotate)
    {
        var gyroRot = Input.gyro.attitude;
        var localAdjust = Quaternion.Euler(localRotate);
        var worldAdjust = Quaternion.Euler(worldRotate);
        return worldAdjust * gyroRot * localAdjust;
    }
}