using UnityEngine;
using UnityEngine.Serialization;

public class TortillaCameraController : MonoBehaviour
{
    // NOTE: THIS IS A QUICK DIRTY HACK.

    public Tortilla Target;
    public AnimationCurve DistanceBySpeed;
    [Min(.1f)] public float Smoothness = .1f;


    private float _dist;


    void Update()
    {
        var speed = Target.Rigidbody.velocity.magnitude;
        var idealDist = DistanceBySpeed.Evaluate(speed);
        _dist = Mathf.Lerp(_dist, idealDist, Time.deltaTime / Smoothness);

        var targetPos = Target.transform.position;
        var dir = transform.forward;
        transform.position = targetPos + dir * _dist;
    }
}
