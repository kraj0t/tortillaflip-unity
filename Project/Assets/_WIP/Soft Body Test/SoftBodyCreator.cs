using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

public class SoftBodyCreator : MonoBehaviour
{
    //[Required]
    // TODO: Must be scene object
    [BoxGroup("Original Particle - NOT PREFAB, MUST BE IN SCENE")]
    public Rigidbody OriginalParticle;

    [BoxGroup("Shape")] [MinValue(.0001f)] public float ColliderDiameter = 0.03f;
    [BoxGroup("Shape")] [MinValue(1)] public int CountX = 5;
    [BoxGroup("Shape")] [MinValue(1)] public int CountY = 2;
    [BoxGroup("Shape")] [MinValue(1)] public int CountZ = 3;

    [Tooltip("Distance between particles. It is multiplied by the ColliderDiameter. Leave to 1 to have touching colliders on creation. IMPORTANT: LinearLimits are relative to this.")]
    [BoxGroup("Shape")] [MinValue(0)]  public float DistanceMultiplier = 1;

    [BoxGroup("Shape")] public bool OmitCorners = true;


    [BoxGroup("Joints")] public bool ConnectDiagonals = false;
    [BoxGroup("Joints")] public bool EnablePreprocessing = true;
    [BoxGroup("Joints")] [MinValue(0)] public float LinearSpring = 200;
    [BoxGroup("Joints")] [MinValue(0)] public float LinearDamper = 10;

    [Tooltip("This is multiplied by the ColliderDiameter and the DistanceMultiplier.")]
    [BoxGroup("Joints")] [MinValue(0)] public float LinearLimitRelative = .9f;

    [BoxGroup("Joints")] [MinValue(0)] public float AngularSpring = 2;
    [BoxGroup("Joints")] [MinValue(0)] public float AngularDamper = .05f;
    [BoxGroup("Joints")] [MinValue(0)] public float AngularLimit = 45f;

    [Tooltip("This is multiplied by the ColliderDiameter and the DistanceMultiplier.")]
    [BoxGroup("Joints")] [MinValue(0)] public float HardLinearLimitRelative = 1.333f;

    [BoxGroup("Joints")] [MinValue(0)] public float HardAngularLimit = 60f;
    [BoxGroup("Joints")] [MinValue(0.01f)] public float BreakForce = Mathf.Infinity;
    [BoxGroup("Joints")] [MinValue(0.01f)] public float BreakTorque = Mathf.Infinity;


    [SerializeField]
    [HideInInspector]
    private Rigidbody[] _particles;


    public bool IsCreated { get => _particles != null && _particles.Length > 0; }


    public float DistanceBetweenParticles { get => ColliderDiameter * DistanceMultiplier; }


    private void Start()
    {
        if (!IsCreated)
            Create();        
    }


#if UNITY_EDITOR
    public bool DEBUG_LiveUpdate = false;
    [MinValue(0)] public int DEBUG_UpdateFrameSkip = 60;
    private int _updatesToSkip = 0;
    private void FixedUpdate()
    {
        if (!DEBUG_LiveUpdate)
            return;

        _updatesToSkip--;
        if (_updatesToSkip >= 0)
            return;
        _updatesToSkip = DEBUG_UpdateFrameSkip;

        ResetAllJointValues();
    }
#endif


    [Button("Reset All Joint Values")]
    [ContextMenu("Reset All Joint Values")]
    public void ResetAllJointValues()
    {
        if (!IsCreated)
            return;

        foreach (var p in _particles)
        {
            if (!p)
                continue;

            foreach (var j in p.GetComponents<ConfigurableJoint>())
                SetJointValues(j);
            p.WakeUp();
        }
    }


    [Button]
    [ContextMenu("Recreate")]
    public void Recreate()
    {
        Destroy();
        Create();
    }


    public void Destroy()
    {
        if (_particles == null)
            return;

        foreach (var rb in _particles)
        {
            if (!rb || rb == OriginalParticle)
                continue;

            if (Application.isPlaying)
                Destroy(rb.gameObject);
            else
                DestroyImmediate(rb.gameObject);
        }

        _particles = null;
    }


    public void Create()
    {
        // TODO: optimize this method's code, especially the loops.

        if (CountX * CountY * CountZ == 0)
            return;

        _particles = new Rigidbody[CountX * CountY * CountZ];

        // Update the original particle and set it as the center one.
        var radius = ColliderDiameter * .5f;
        foreach (var j in OriginalParticle.GetComponents<ConfigurableJoint>())
            DestroyImmediate(j, false);
        var col = OriginalParticle.GetComponent<SphereCollider>();
        if (!col)
            col = OriginalParticle.gameObject.AddComponent<SphereCollider>();
        col.radius = radius;
        var centerIndex = Index(CountX / 2, CountY / 2, CountZ / 2);
        _particles[centerIndex] = OriginalParticle;

        // Create and position all other particles.
        var origin = OriginalParticle.position;
        var dist = DistanceBetweenParticles;
        var parent = OriginalParticle.transform.parent;
        var halfSize = .5f * dist * new Vector3(CountX, CountY, CountZ) - new Vector3(radius, radius, radius);
        for (int i = 0; i < CountX; i++)
        {
            for (int j = 0; j < CountY; j++)
            {
                for (int k = 0; k < CountZ; k++)
                {
                    if (OmitCorners && IsCorner(i, j, k))
                        continue;

                    var index = Index(i, j, k);
                    if (index == centerIndex)
                        continue;

                    //var localPos = -halfSize + dist * new Vector3(i, j, k);
                    var pos = origin - halfSize + dist * new Vector3(i, j, k);
                    var rb = Instantiate<Rigidbody>(OriginalParticle, parent);
                    //rb.transform.localPosition = localPos;
                    rb.transform.position = pos;
                    //rb.position = pos;
                    Set(index, rb);
                }
            }
        }

        // Connect particles via joints.
        for (int i = 0; i < CountX; i++)
        {
            for (int j = 0; j < CountY; j++)
            {
                for (int k = 0; k < CountZ; k++)
                {
                    if (OmitCorners && IsCorner(i, j, k))
                        continue;

                    var rb = Get(i, j, k);

                    if (i < CountX - 1)
                        ConnectConfigurable(rb, Get(i + 1, j, k));

                    if (j < CountY - 1)
                        ConnectConfigurable(rb, Get(i, j + 1, k));

                    if (k < CountZ - 1)
                        ConnectConfigurable(rb, Get(i, j, k + 1));

                    if (ConnectDiagonals)
                    {
                        if (i < CountX - 1 && j < CountY - 1)
                            ConnectConfigurable(rb, Get(i + 1, j + 1, k));

                        if (i < CountX - 1 && k < CountZ - 1)
                            ConnectConfigurable(rb, Get(i + 1, j, k + 1));

                        if (j < CountY - 1 && k < CountZ - 1)
                            ConnectConfigurable(rb, Get(i, j + 1, k + 1));


                        /*if (i < Dimension - 1 && j < Dimension - 1)
                            ConnectBodies(rb, _particles[(i + 1) * Dimension + j + 1]);

                        if (i < Dimension - 1 && j > 0)
                            ConnectBodies(rb, _particles[(i + 1) * Dimension + j - 1]);
                            */
                    }
                }
            }
        }
    }


    // TODO: make these with IEnumerable or something
    #region Array

    public int Count { get => _particles == null ? 0 : _particles.Length; }


    // i -> Width, j -> Height, k -> Depth
    public Rigidbody Get(int i, int j, int k)
    {
        return _particles[Index(i, j, k)];
    }


    // One-dimensional index getter
    public Rigidbody Get(int index)
    {
        return _particles[index];
    }


    private void Set(int i, int j, int k, Rigidbody rb)
    {
        _particles[Index(i, j, k)] = rb;
    }


    private void Set(int index, Rigidbody rb)
    {
        _particles[index] = rb;
    }


    private int Index(int i, int j, int k)
    {
        return i * CountY * CountZ + j * CountZ + k;
    }


    private bool IsCorner(int i, int j, int k)
    {
        return (i == 0 || i == CountX - 1) && (j == 0 || j == CountY - 1) && (k == 0 || k == CountZ - 1);
    }

    #endregion Array


    private Joint ConnectConfigurable(Rigidbody a, Rigidbody b)
    {
        if (!a || !b)
            return null;

        var joint = a.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = b;
        joint.configuredInWorldSpace = true;
        //joint.autoConfigureConnectedAnchor = true;

        var dir = (b.position - a.position).normalized;
        joint.axis = dir;
        joint.secondaryAxis = Quaternion.LookRotation(dir) * Vector3.right;

        SetJointValues(joint);

Debug.Log(" QUE NO CHOQUEN ENTRE LAS PARTICULAS!! MOSTARAR UN MENSAJE DE ERROR SI OCURRE!!");

        return joint;
    }


    private void SetJointValues(ConfigurableJoint joint)
    {
        joint.enablePreprocessing = EnablePreprocessing;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        var linearLimit = new SoftJointLimit() { limit = DistanceBetweenParticles * LinearLimitRelative };
        joint.linearLimit = linearLimit;

        var angularLimitNeg = new SoftJointLimit() { limit = -AngularLimit };
        joint.lowAngularXLimit = angularLimitNeg;
        var angularLimit = new SoftJointLimit() { limit = AngularLimit };
        joint.highAngularXLimit = angularLimit;
        joint.angularYLimit = angularLimit;
        joint.angularZLimit = angularLimit;

        var linearDrive = new JointDrive() { positionSpring = LinearSpring, positionDamper = LinearDamper, maximumForce = Mathf.Infinity };
        joint.xDrive = linearDrive;

        joint.rotationDriveMode = RotationDriveMode.Slerp;
        var angularDrive = new JointDrive() { positionSpring = AngularSpring, positionDamper = AngularDamper, maximumForce = Mathf.Infinity };
        joint.slerpDrive = angularDrive;

        joint.breakForce = BreakForce;
        joint.breakTorque = BreakTorque;

        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = DistanceBetweenParticles * HardLinearLimitRelative;
        joint.projectionAngle = HardAngularLimit;
    }


    /*
    private void CopyJoint(ConfigurableJoint src, ConfigurableJoint dst)
    {
        // .......
        Debug.LogError("NOT IMPLEMENTED!!!");
    }
    
    

    private ConfigurableJoint Connect(Rigidbody a, Rigidbody b)
    {
        if (!a || !b)
            return null;

        // Use the particle's joint, or dupplicate it if it's already in use.
        var joint = a.gameObject.GetComponent<ConfigurableJoint>();
        if (joint.connectedBody)
        {
            var oldJoint = joint;
            joint = a.gameObject.AddComponent<ConfigurableJoint>();
            CopyJoint(oldJoint, joint);
        }
        joint.connectedBody = b;

        var dir = (b.position - a.position).normalized;
        joint.axis = dir;
        joint.secondaryAxis = Quaternion.LookRotation(dir) * Vector3.right;

        Debug.Log(" QUE NO CHOQUEN ENTRE LAS PARTICULAS!! MOSTARAR UN MENSAJE DE ERROR SI OCURRE!!");

        return joint;
    }
    */
}
