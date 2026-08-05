using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MultimeterCableMesh : MonoBehaviour
{
    [Header("Cable Connection")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Cable Shape")]
    [Min(4)]
    public int curveSegments = 24;

    [Min(4)]
    public int radialSegments = 8;

    [Header("Cable Size")]
    [Min(0.001f)]
    public float cableRadius = 0.012f;

    [Header("Cable Sag")]
    public float sagAmount = 0.15f;

    [Header("Cable Smoothness")]
    [Range(0f, 1f)]
    public float curveSmoothness = 0.5f;

    [Header("Update")]
    public bool updateEveryFrame = true;

    private MeshFilter meshFilter;
    private Mesh cableMesh;


    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();

        cableMesh = new Mesh();
        cableMesh.name = "Dynamic Multimeter Cable";

        cableMesh.MarkDynamic();

        meshFilter.mesh = cableMesh;
    }


    private void Start()
    {
        GenerateCable();
    }


    private void Update()
    {
        if (updateEveryFrame)
        {
            GenerateCable();
        }
    }


    public void GenerateCable()
    {
        if (startPoint == null ||
            endPoint == null)
        {
            return;
        }

        if (cableMesh == null)
        {
            cableMesh = new Mesh();
            cableMesh.name = "Dynamic Multimeter Cable";

            meshFilter.mesh = cableMesh;
        }


        Vector3 start =
            startPoint.position;

        Vector3 end =
            endPoint.position;


        int vertexCount =
            (curveSegments + 1) *
            radialSegments;


        Vector3[] vertices =
            new Vector3[vertexCount];

        Vector3[] normals =
            new Vector3[vertexCount];

        Vector2[] uv =
            new Vector2[vertexCount];


        int triangleCount =
            curveSegments *
            radialSegments *
            6;

        int[] triangles =
            new int[triangleCount];


        // --------------------------------------------------
        // CREATE CURVE
        // --------------------------------------------------

        for (int i = 0;
             i <= curveSegments;
             i++)
        {
            float t =
                (float)i /
                curveSegments;


            // Smooth interpolation
            float smoothT =
                t * t * (3f - 2f * t);


            Vector3 center =
                Vector3.Lerp(
                    start,
                    end,
                    smoothT
                );


            // --------------------------------------------------
            // CABLE SAG
            // --------------------------------------------------

            float sag =
                Mathf.Sin(
                    t * Mathf.PI
                ) *
                sagAmount;


            center.y -= sag;


            // --------------------------------------------------
            // CREATE TUBE ORIENTATION
            // --------------------------------------------------

            Vector3 direction;

            if (i == 0)
            {
                direction =
                    GetCurvePoint(
                        start,
                        end,
                        0.01f
                    ) - start;
            }
            else if (i == curveSegments)
            {
                direction =
                    end -
                    GetCurvePoint(
                        start,
                        end,
                        0.99f
                    );
            }
            else
            {
                Vector3 previous =
                    GetCurvePoint(
                        start,
                        end,
                        Mathf.Max(
                            0f,
                            t - 0.01f
                        )
                    );

                Vector3 next =
                    GetCurvePoint(
                        start,
                        end,
                        Mathf.Min(
                            1f,
                            t + 0.01f
                        )
                    );

                direction =
                    next - previous;
            }


            direction.Normalize();


            Vector3 reference =
                Vector3.up;


            // Prevent parallel vectors
            if (Mathf.Abs(
                Vector3.Dot(
                    direction,
                    reference
                )
            ) > 0.95f)
            {
                reference =
                    Vector3.forward;
            }


            Vector3 right =
                Vector3.Cross(
                    direction,
                    reference
                ).normalized;


            Vector3 up =
                Vector3.Cross(
                    right,
                    direction
                ).normalized;


            // --------------------------------------------------
            // CREATE CABLE RING
            // --------------------------------------------------

            for (int j = 0;
                 j < radialSegments;
                 j++)
            {
                float angle =
                    (float)j /
                    radialSegments *
                    Mathf.PI *
                    2f;


                Vector3 radial =
                    Mathf.Cos(angle) *
                    right
                    +
                    Mathf.Sin(angle) *
                    up;


                int index =
                    i *
                    radialSegments
                    +
                    j;


                vertices[index] =
                    transform.InverseTransformPoint(
                        center +
                        radial *
                        cableRadius
                    );


                normals[index] =
                    radial;


                uv[index] =
                    new Vector2(
                        (float)j /
                        radialSegments,

                        t
                    );
            }
        }


        // --------------------------------------------------
        // CREATE TRIANGLES
        // --------------------------------------------------

        int triangleIndex = 0;


        for (int i = 0;
             i < curveSegments;
             i++)
        {
            for (int j = 0;
                 j < radialSegments;
                 j++)
            {
                int current =
                    i *
                    radialSegments
                    +
                    j;


                int next =
                    i *
                    radialSegments
                    +
                    (j + 1) %
                    radialSegments;


                int currentNext =
                    (i + 1) *
                    radialSegments
                    +
                    j;


                int nextNext =
                    (i + 1) *
                    radialSegments
                    +
                    (j + 1) %
                    radialSegments;


                // Triangle 1
                triangles[triangleIndex++] =
                    current;

                triangles[triangleIndex++] =
                    currentNext;

                triangles[triangleIndex++] =
                    nextNext;


                // Triangle 2
                triangles[triangleIndex++] =
                    current;

                triangles[triangleIndex++] =
                    nextNext;

                triangles[triangleIndex++] =
                    next;
            }
        }


        // --------------------------------------------------
        // APPLY MESH
        // --------------------------------------------------

        cableMesh.Clear();

        cableMesh.vertices =
            vertices;

        cableMesh.normals =
            normals;

        cableMesh.uv =
            uv;

        cableMesh.triangles =
            triangles;

        cableMesh.RecalculateBounds();
    }


    private Vector3 GetCurvePoint(
        Vector3 start,
        Vector3 end,
        float t
    )
    {
        float smoothT =
            t * t * (3f - 2f * t);


        Vector3 point =
            Vector3.Lerp(
                start,
                end,
                smoothT
            );


        float sag =
            Mathf.Sin(
                t * Mathf.PI
            ) *
            sagAmount;


        point.y -= sag;


        return point;
    }
}