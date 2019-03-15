using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

public class TortillaCameraController : MonoBehaviour
{
    // NOTE: THIS IS A QUICK DIRTY HACK.

    public Rigidbody Target;
    public AnimationCurve DistanceBySpeed;
    [Min(.1f)] public float Smoothness = .1f;


    public GameObject TargetGameObject { get => Target.gameObject; set => Target = value.GetComponentInChildren<Rigidbody>(); }


    private float _dist;


    void Update()
    {
        if (!Target)
            return;

        var speed = Target.velocity.magnitude;
        var idealDist = DistanceBySpeed.Evaluate(speed);
        _dist = Mathf.Lerp(_dist, idealDist, Time.deltaTime / Smoothness);

        var targetPos = Target.transform.position;
        var dir = transform.forward;
        transform.position = targetPos + dir * _dist;

        var constr = transform.parent.GetComponent<PositionConstraint>();
        var constrSrc = new ConstraintSource();
        constrSrc.sourceTransform = Target.transform;
        constr.SetSource(0, constrSrc);
    }
}
