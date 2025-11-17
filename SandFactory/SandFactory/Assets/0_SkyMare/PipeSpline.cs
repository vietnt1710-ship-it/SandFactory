using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PipeSpline : MonoBehaviour
{
    [Header("Spline Settings")]
    [SerializeField] private Transform[] controlPoints;
    [SerializeField] private int splineResolution = 50;

    [Header("Pipe Settings")]
    [SerializeField] private float pipeRadius = 0.5f;
    [SerializeField] private int radialSegments = 16;
    [SerializeField] private Material pipeMaterial;

    [Header("Visualization")]
    [SerializeField] private bool showSpline = true;
    [SerializeField] private bool showControlPoints = true;
    [SerializeField] private Color splineColor = Color.red;

    private MeshFilter meshFilter;
    private List<Vector3> splinePoints = new List<Vector3>();
    private List<Vector3> splineTangents = new List<Vector3>();

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        GeneratePipe();
    }

    void OnValidate()
    {
        if (Application.isPlaying && controlPoints != null && controlPoints.Length >= 2)
        {
            GeneratePipe();
        }
    }

    public void GeneratePipe()
    {
        if (controlPoints == null || controlPoints.Length < 2)
        {
            Debug.LogWarning("Cần ít nhất 2 điểm điều khiển!");
            return;
        }

        GenerateSplinePoints();
        Mesh pipeMesh = CreatePipeMesh();

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh = pipeMesh;

        if (pipeMaterial != null)
        {
            GetComponent<MeshRenderer>().material = pipeMaterial;
        }
    }

    void GenerateSplinePoints()
    {
        splinePoints.Clear();
        splineTangents.Clear();

        for (int i = 0; i < controlPoints.Length - 1; i++)
        {
            Vector3 p0 = i > 0 ? controlPoints[i - 1].position : controlPoints[i].position;
            Vector3 p1 = controlPoints[i].position;
            Vector3 p2 = controlPoints[i + 1].position;
            Vector3 p3 = (i + 2 < controlPoints.Length) ? controlPoints[i + 2].position : p2;

            int segmentsPerCurve = splineResolution / (controlPoints.Length - 1);

            for (int j = 0; j < segmentsPerCurve; j++)
            {
                float t = j / (float)segmentsPerCurve;
                Vector3 point = CalculateCatmullRom(p0, p1, p2, p3, t);
                Vector3 tangent = CalculateCatmullRomDerivative(p0, p1, p2, p3, t).normalized;

                splinePoints.Add(point);
                splineTangents.Add(tangent);
            }
        }

        // Thêm điểm cuối
        splinePoints.Add(controlPoints[controlPoints.Length - 1].position);

        // Tangent cho điểm cuối
        if (splinePoints.Count >= 2)
        {
            splineTangents.Add((splinePoints[splinePoints.Count - 1] - splinePoints[splinePoints.Count - 2]).normalized);
        }
    }

    Vector3 CalculateCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    Vector3 CalculateCatmullRomDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;

        return 0.5f * (
            (-p0 + p2) +
            2f * (2f * p0 - 5f * p1 + 4f * p2 - p3) * t +
            3f * (-p0 + 3f * p1 - 3f * p2 + p3) * t2
        );
    }
    public bool doubleSided = true;
    Mesh CreatePipeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Pipe Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        // Sử dụng Rotation Minimizing Frame (RMF) để tránh xoắn
        Vector3 initialNormal = Vector3.up;
        if (Vector3.Dot(splineTangents[0], Vector3.up) > 0.99f)
        {
            initialNormal = Vector3.right;
        }

        Vector3 previousNormal = Vector3.Cross(splineTangents[0], initialNormal).normalized;
        Vector3 previousBinormal = Vector3.Cross(splineTangents[0], previousNormal).normalized;

        for (int i = 0; i < splinePoints.Count; i++)
        {
            Vector3 point = splinePoints[i];
            Vector3 tangent = splineTangents[i];

            // Rotation Minimizing Frame
            Vector3 normal, binormal;
            if (i == 0)
            {
                normal = previousNormal;
                binormal = previousBinormal;
            }
            else
            {
                // Tính toán frame mới dựa trên frame trước đó
                Vector3 v1 = splinePoints[i] - splinePoints[i - 1];
                float c1 = Vector3.Dot(v1, v1);
                Vector3 rL = previousNormal - (2f / c1) * Vector3.Dot(v1, previousNormal) * v1;
                Vector3 tL = tangent - (2f / c1) * Vector3.Dot(v1, tangent) * v1;

                Vector3 v2 = tangent - tL;
                float c2 = Vector3.Dot(v2, v2);

                if (c2 < 0.0001f)
                {
                    normal = rL;
                }
                else
                {
                    normal = rL - (2f / c2) * Vector3.Dot(v2, rL) * v2;
                }

                normal = normal.normalized;
                binormal = Vector3.Cross(tangent, normal).normalized;

                previousNormal = normal;
                previousBinormal = binormal;
            }

            // Tạo vòng tròn vertices
            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = (j / (float)radialSegments) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);

                Vector3 offset = (normal * x + binormal * y) * pipeRadius;
                Vector3 vertex = point + offset;
                vertices.Add(vertex);

                // Normal hướng ra ngoài
                Vector3 normalDir = (normal * x + binormal * y).normalized;
                normals.Add(normalDir);

                // UV mapping
                float u = j / (float)radialSegments;
                float v = i / (float)(splinePoints.Count - 1);
                uvs.Add(new Vector2(u, v));
            }
        }

        // Tạo triangles (mặt ngoài)
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int current = i * (radialSegments + 1) + j;
                int next = current + radialSegments + 1;

                // Mặt ngoài (counter-clockwise)
                triangles.Add(current);
                triangles.Add(current + 1);
                triangles.Add(next);

                triangles.Add(current + 1);
                triangles.Add(next + 1);
                triangles.Add(next);
            }
        }

        // Tạo mặt trong (nếu bật double-sided)
        if (doubleSided)
        {
            for (int i = 0; i < splinePoints.Count - 1; i++)
            {
                for (int j = 0; j < radialSegments; j++)
                {
                    int current = i * (radialSegments + 1) + j;
                    int next = current + radialSegments + 1;

                    // Mặt trong (clockwise - đảo ngược thứ tự)
                    triangles.Add(current);
                    triangles.Add(next);
                    triangles.Add(current + 1);

                    triangles.Add(current + 1);
                    triangles.Add(next);
                    triangles.Add(next + 1);
                }
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();

        mesh.RecalculateBounds();

        return mesh;
    }

    void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Length < 2)
            return;

        // Vẽ control points
        if (showControlPoints)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < controlPoints.Length; i++)
            {
                if (controlPoints[i] != null)
                {
                    Gizmos.DrawSphere(controlPoints[i].position, 0.1f);

                    // Vẽ số thứ tự
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(controlPoints[i].position + Vector3.up * 0.3f, i.ToString());
#endif
                }
            }

            // Vẽ đường nối giữa các control points
            Gizmos.color = Color.gray;
            for (int i = 0; i < controlPoints.Length - 1; i++)
            {
                if (controlPoints[i] != null && controlPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(controlPoints[i].position, controlPoints[i + 1].position);
                }
            }
        }

        // Vẽ spline
        if (showSpline && splinePoints.Count > 1)
        {
            Gizmos.color = splineColor;
            for (int i = 0; i < splinePoints.Count - 1; i++)
            {
                Gizmos.DrawLine(splinePoints[i], splinePoints[i + 1]);
            }
        }
    }
}