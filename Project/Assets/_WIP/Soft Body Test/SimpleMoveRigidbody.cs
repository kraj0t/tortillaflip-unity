using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class SimpleMoveRigidbody : MonoBehaviour
{
    public float MaxSpeed = 5;
    public float Accel = 20;
    public float RunMultiplier = 2;
    public float Decel = 5;

    private Vector3 _speed;


    public Rigidbody Body { get; private set; }


    private void Start()
    {
        Body = GetComponent<Rigidbody>();
    }


    void Update()
    {
        var horiz = Input.GetAxis("Horizontal");
        float vert = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
        var forward = Input.GetAxis("Vertical");
        var run = Input.GetButton("Fire3");

        var multip = run ? RunMultiplier : 1;

        if (Mathf.Approximately(0, horiz) && Mathf.Approximately(0, vert) && Mathf.Approximately(0, forward))
        {
            _speed -= Time.deltaTime * _speed * Decel;
        }
        else
        {
            if (_speed.magnitude <= multip * MaxSpeed)
            {
                _speed += Time.deltaTime * Accel * multip * new Vector3(horiz, vert, forward);
                _speed = Vector3.ClampMagnitude(_speed, multip * MaxSpeed);
            }
            else
                _speed -= Time.deltaTime * _speed * Decel;
        }

        _speed = Vector3.ClampMagnitude(_speed, RunMultiplier * MaxSpeed);
    }


    private void FixedUpdate()
    {
        var offset = Time.fixedDeltaTime * _speed;
        Body.MovePosition(Body.position + offset);
    }
}
