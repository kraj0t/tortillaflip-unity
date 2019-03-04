using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoftBodyCreator : MonoBehaviour
{
    public Rigidbody Prefab;

    public float Offset = 1;

    public float Spring = 100;
    public float Damper = 0.5f;

    public int Width = 5;
    public int Height = 2;
    public int Depth = 3;


    void Start()
    {
        var particles = new Rigidbody[Width * Height * Depth];

        // Create and position all particles.
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Depth; k++)
                {
                    var localPos = new Vector3(i * Offset, j * Offset, k * Offset);
                    var rb = Instantiate<Rigidbody>(Prefab, transform);
                    rb.transform.localPosition = localPos;
                    particles[Index(i, j, k)] = rb;
                }
            }
        }

        // Connect particles via joints.
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int k = 0; k < Depth; k++)
                {
                    var rb = particles[Index(i, j, k)];

                    if (i < Width - 1)
                        ConnectBodies(rb, particles[Index(i + 1, j, k)]);

                    if (j < Height - 1)
                        ConnectBodies(rb, particles[Index(i, j + 1, k)]);

                    if (k < Depth - 1)
                        ConnectBodies(rb, particles[Index(i, j, k + 1)]);


                    /*if (i < Dimension - 1 && j < Dimension - 1)
                        ConnectBodies(rb, particles[(i + 1) * Dimension + j + 1]);

                    if (i < Dimension - 1 && j > 0)
                        ConnectBodies(rb, particles[(i + 1) * Dimension + j - 1]);
                        */
                }
            }
        }
    }


    /*
    void Start_2D()
    {
        var particles = new Rigidbody[Dimension * Dimension];

        // Create and position all particles.
        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                var localPos = new Vector3(i * Offset, 0, j * Offset);
                var rb = Instantiate<Rigidbody>(Prefab, transform);
                rb.transform.localPosition = localPos;
                particles[i * Dimension + j] = rb;
            }
        }

        // Connect particles via joints.
        for (int i = 0; i < Dimension; i++)
        {
            for (int j = 0; j < Dimension; j++)
            {
                var rb = particles[i * Dimension + j];

                if (i < Dimension - 1)
                    ConnectBodies(rb, particles[(i + 1) * Dimension + j]);

                if (j < Dimension - 1)
                    ConnectBodies(rb, particles[i * Dimension + j + 1]);

                
                if (i < Dimension - 1 && j < Dimension - 1)
                    ConnectBodies(rb, particles[(i + 1) * Dimension + j + 1]);

                if (i < Dimension - 1 && j > 0)
                    ConnectBodies(rb, particles[(i + 1) * Dimension + j - 1]);
                
            }
        }
    }
    */


    private int Index(int i, int j, int k)
    {
        return i * Height * Depth + j * Depth + k;
    }


    private SpringJoint ConnectBodies(Rigidbody a, Rigidbody b)
    {
        var joint = a.gameObject.AddComponent<SpringJoint>();
        joint.connectedBody = b;
        joint.spring = Spring;
        joint.damper = Damper;
        return joint;
    }
}
