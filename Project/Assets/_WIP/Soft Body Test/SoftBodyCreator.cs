//using Sirenix.OdinInspector;
using UnityEngine;


public class SoftBodyCreator : MonoBehaviour
{
    public enum JointType
    {
        Spring,
        Fixed,
        Configurable
    }


    //[Required]
    [Header("Particle")]
    public Rigidbody Prefab;

    [Header("Shape")]
    public int Width = 5;
    public int Height = 2;
    public int Depth = 3;
    public float Offset = 1;
    public bool OmitCorners = true;

    [Header("Joints")]
    public bool ConnectDiagonals = false;
    public JointType Type;
    public bool EnablePreprocessing = true;
    //[ShowIf("Type", JointType.Spring)]
    public float Spring = 1000;
    //[ShowIf("Type", JointType.Spring)]
    public float Damper = 10;
    public float Limit = 0.05f;
    public float AngularLimit = 30f;
    public float Bounciness = 0;
    public float ContactDistance = 0.001f;
    public float RotationSpring = 500;
    public float BreakForce = Mathf.Infinity;
    public float BreakTorque = Mathf.Infinity;

    [Space(60)]
    public bool DEBUG_RECREATE_BUTTON;

    [SerializeField]
    [HideInInspector]
    private Rigidbody[] _particles;


    public bool IsCreated { get => _particles != null && _particles.Length > 0; }


    private void Start()
    {
        if (!IsCreated)
            Create();
    }


    private void OnValidate()
    {
        if (DEBUG_RECREATE_BUTTON)
            Recreate();
        DEBUG_RECREATE_BUTTON = false;
    }


    [ContextMenu("Recreate")]
    //[Button]
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
            if (!rb)
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
        _particles = new Rigidbody[Width * Height * Depth];

        // Create and position all particles.
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Depth; k++)
                {
                    if (OmitCorners && (i == 0 || i == Width - 1) && (j == 0 || j == Height - 1) && (k == 0 || k == Depth - 1))
                        continue;

                    var localPos = new Vector3(i * Offset, j * Offset, k * Offset);
                    var rb = Instantiate<Rigidbody>(Prefab, transform);
                    rb.transform.localPosition = localPos;
                    _particles[Index(i, j, k)] = rb;
                }
            }
        }

        // Connect particles via joints.
        JointConnectDelegate connectDelegate;
        if (Type == JointType.Spring)
            connectDelegate = new JointConnectDelegate(ConnectSpring);
        else if (Type == JointType.Configurable)
            connectDelegate = new JointConnectDelegate(ConnectConfigurable);
        else
            connectDelegate = new JointConnectDelegate(ConnectFixed);
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Depth; k++)
                {
                    if (OmitCorners && (i == 0 || i == Width - 1) && (j == 0 || j == Height - 1) && (k == 0 || k == Depth - 1))
                        continue;

                    var rb = Get(i, j, k);

                    if (i < Width - 1)
                        connectDelegate(rb, Get(i + 1, j, k));

                    if (j < Height - 1)
                        connectDelegate(rb, Get(i, j + 1, k));

                    if (k < Depth - 1)
                        connectDelegate(rb, Get(i, j, k + 1));

                    if (ConnectDiagonals)
                    {
                        if (i < Width && j < Height)
                            connectDelegate(rb, Get(i + 1, j + 1, k));

                        if (i < Width && k < Depth)
                            connectDelegate(rb, Get(i + 1, j, k + 1));

                        if (j < Height && k < Depth)
                            connectDelegate(rb, Get(i, j + 1, k + 1));


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


    private Rigidbody Get(int i, int j, int k)
    {
        return _particles[Index(i, j, k)];
    }


    private int Index(int i, int j, int k)
    {
        return i * Height * Depth + j * Depth + k;
    }


    private delegate Joint JointConnectDelegate(Rigidbody a, Rigidbody b);

    private Joint ConnectFixed(Rigidbody a, Rigidbody b)
    {
        var joint = a.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = b;
        joint.enablePreprocessing = EnablePreprocessing;
        return joint;
    }

    
    private Joint ConnectSpring(Rigidbody a, Rigidbody b)
    {
        var joint = a.gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = b;
        joint.spring = Spring;
        joint.damper = Damper;
        joint.enablePreprocessing = EnablePreprocessing;
        return joint;
    }


    private bool Is1(float x)
    {
        return Mathf.Approximately(1, Mathf.Abs(x));
    }


    private Joint ConnectConfigurable(Rigidbody a, Rigidbody b)
    {
        if (!a || !b)
            return null;

        var joint = a.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = b;

        joint.enablePreprocessing = EnablePreprocessing;
        //joint.autoConfigureConnectedAnchor = true;
        joint.configuredInWorldSpace = true;

        var aPos = a.position;
        var bPos = b.position;
        var AtoB = bPos - aPos;
        var dir = AtoB.normalized;
        joint.axis = dir;

        // It doesn't matter where the secondary axis points as long as it is different from the main axis.
        //var cross1 = Vector3.Cross(dir, Vector3.right);
        //var cross2 = Vector3.Cross(dir, cross1);
        //joint.secondaryAxis = cross2;
        //joint.secondaryAxis = Quaternion.AngleAxis(90, dir) * Vector3.forward;
        joint.secondaryAxis = Quaternion.LookRotation(dir) * Vector3.right;
        joint.secondaryAxis.Normalize();

        if (joint.secondaryAxis.Equals(dir))
            Debug.LogError("MIERDA");

        joint.xMotion = ConfigurableJointMotion.Limited;
        //joint.yMotion = ConfigurableJointMotion.Locked;
        //joint.zMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;
        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        joint.linearLimit = new SoftJointLimit() { limit = Limit };
        joint.angularYLimit = new SoftJointLimit() { limit = AngularLimit };
        joint.angularZLimit = new SoftJointLimit() { limit = AngularLimit };

        joint.xDrive = new JointDrive() { positionSpring = Spring, positionDamper = Damper, maximumForce = Mathf.Infinity };
        joint.angularYZDrive = new JointDrive() { positionSpring = RotationSpring, positionDamper = Damper, maximumForce = Mathf.Infinity };

        joint.breakForce = BreakForce;
        joint.breakTorque = BreakTorque;


        //HARDCODING
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = Limit;
        joint.projectionAngle = AngularLimit;



Debug.Log(" QUE NO CHOQUEN ENTRE LAS PARTICULAS!! MOSTARAR UN MENSAJE DE ERROR SI OCURRE!!");

        return joint;
    }
}
