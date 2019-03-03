using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance { get; private set; }


    public int TargetFrameRate = 60;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (Instance == null)
            Instance = this;
        else
            Debug.Log("Warning: multiple " + this + " in scene!");
    }


    void Start()
    {
#if UNITY_EDITOR
        if (UnityEditor.PlayerSettings.accelerometerFrequency != TargetFrameRate)
            Debug.LogError("The accelerometer frequency in the Player settings must match the runtime one.");
#endif

        Application.targetFrameRate = TargetFrameRate;

        var targetFrameStep = 1 / (float)TargetFrameRate;
        Input.gyro.updateInterval = targetFrameStep;
        Time.fixedDeltaTime = targetFrameStep;

        Physics.autoSimulation = false;
        Physics.autoSyncTransforms = false;
        //Physics.

        //Physics.
        //GameManager
    }


    private void FixedUpdate()
    {
        Physics.Simulate(Time.fixedDeltaTime);
    }
}
