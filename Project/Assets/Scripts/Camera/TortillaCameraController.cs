using UnityEngine;

public class TortillaCameraController : MonoBehaviour
{
    // NOTE: THIS IS A QUICK DIRTY HACK.

    public Tortilla Target;
    public AnimationCurve LocalZBySpeed;
    [Min(.1f)] public float SmoothSpeed = 10;

    void Update()
    {
        var speed = Target.Rigidbody.velocity.magnitude;
        var localPos = transform.localPosition;
        var targetZ = LocalZBySpeed.Evaluate(speed);
        localPos.z = Mathf.SmoothStep(localPos.z, targetZ, Time.deltaTime * SmoothSpeed);
        transform.localPosition = localPos;
    }
}
