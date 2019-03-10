using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class SimpleClickMoveRigidbody : MonoBehaviour
{
    public float Speed = 5;


    public Rigidbody Body { get; private set; }


    private Vector3 _dest;


    private void Start()
    {
        Body = GetComponent<Rigidbody>();
        _dest = Body.position;
    }


    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                _dest = hit.point;
            }
        }
    }


    private void FixedUpdate()
    {
        var curPos = Body.position;
        var toDest = _dest - curPos;
        var dir = toDest.normalized;
        var dist = Vector3.Dot(dir, toDest);
        var movedDist = Mathf.Min(dist, Time.fixedDeltaTime * Speed);
        var offset = dir * movedDist;
        Body.MovePosition(curPos + offset);
    }
}
