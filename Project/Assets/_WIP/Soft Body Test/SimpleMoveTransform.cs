using UnityEngine;


[DisallowMultipleComponent]
public class SimpleMoveTransform : MonoBehaviour
{
    public float MaxSpeed = 5;
    public float Accel = 20;
    public float RunMultiplier = 2;
    public float Decel = 5;

    private Vector3 _speed;


    void Update()
    {
        var horiz = Input.GetAxis("Horizontal");
        var forward = Input.GetAxis("Vertical");
        float vert = (Input.GetKey(KeyCode.Q) ? -1 : 0) + (Input.GetKey(KeyCode.E) ? 1 : 0);
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

        var offset = Time.deltaTime * _speed;
        transform.position += offset;
    }
}
