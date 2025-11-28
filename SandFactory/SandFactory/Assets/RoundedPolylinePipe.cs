using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoundedPolylinePipe : MonoBehaviour
{
    [Header("Polyline Settings")]
    [SerializeField] private Transform[] controlPoints;
    [SerializeField] private float cornerRadius = 0.5f;
    [SerializeField] private int cornerSegments = 8;
    [SerializeField] private float pointSpacing = 0.1f;

    [Header("Outer Pipe Settings")]
    [SerializeField] private float pipeRadius = 0.5f;
    [SerializeField] private int radialSegments = 16;
    [SerializeField] private Material pipeMaterial;
    [SerializeField] private bool doubleSided = false;

    [Header("Inner Pipe Settings")]
    [SerializeField] private bool generateInnerPipe = true;
    [SerializeField] private float innerPipeRadius = 0.3f;
    [SerializeField] private Material innerPipeMaterial;
    [SerializeField] private bool innerDoubleSided = false;

    [SerializeField] private Material outLine;

    [Header("Visualization")]
    [SerializeField] private bool showPolyline = true;
    [SerializeField] private bool showControlPoints = true;
    [SerializeField] private Color polylineColor = Color.red;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private List<Vector3> pathPoints = new List<Vector3>();
    private List<Vector3> pathTangents = new List<Vector3>();

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
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

        GenerateRoundedPolyline();
        Mesh pipeMesh = CreateOptimizedPipeMesh();

        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>();

        meshFilter.mesh = pipeMesh;

        SetupMaterials();
    }

    void SetupMaterials()
    {
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (generateInnerPipe)
        {
            Material[] materials = new Material[3];
            materials[0] = pipeMaterial;
            materials[1] = innerPipeMaterial != null ? innerPipeMaterial : pipeMaterial;
            materials[2] = outLine;
            meshRenderer.materials = materials;
        }
        else
        {
            meshRenderer.material = pipeMaterial;
        }
    }

    void GenerateRoundedPolyline()
    {
        pathPoints.Clear();
        pathTangents.Clear();

        List<Vector3> rawPoints = new List<Vector3>();
        List<Vector3> rawTangents = new List<Vector3>();

        GenerateRawPolyline(rawPoints, rawTangents);
        ResamplePathEvenly(rawPoints, rawTangents);
    }

    void GenerateRawPolyline(List<Vector3> points, List<Vector3> tangents)
    {
        if (cornerRadius <= 0.001f)
        {
            for (int i = 0; i < controlPoints.Length; i++)
            {
                Vector3 tangent = Vector3.forward;
                if (i < controlPoints.Length - 1)
                    tangent = (controlPoints[i + 1].position - controlPoints[i].position).normalized;
                else if (i > 0)
                    tangent = (controlPoints[i].position - controlPoints[i - 1].position).normalized;

                points.Add(controlPoints[i].position);
                tangents.Add(tangent);
            }
            return;
        }

        for (int i = 0; i < controlPoints.Length; i++)
        {
            if (i == 0)
            {
                Vector3 curr = controlPoints[i].position;
                Vector3 next = controlPoints[i + 1].position;
                Vector3 dir = (next - curr).normalized;
                float dist = Vector3.Distance(curr, next);
                float radius = Mathf.Min(cornerRadius, dist * 0.5f);

                points.Add(curr);
                tangents.Add(dir);

                Vector3 endPoint = next - dir * radius;
                float segmentLength = Vector3.Distance(curr, endPoint);
                int straightSegments = Mathf.Max(1, Mathf.CeilToInt(segmentLength / (pointSpacing * 0.5f)));

                for (int j = 1; j <= straightSegments; j++)
                {
                    float t = j / (float)straightSegments;
                    points.Add(Vector3.Lerp(curr, endPoint, t));
                    tangents.Add(dir);
                }
            }
            else if (i == controlPoints.Length - 1)
            {
                Vector3 curr = controlPoints[i].position;
                Vector3 prev = controlPoints[i - 1].position;
                Vector3 dir = (curr - prev).normalized;

                points.Add(curr);
                tangents.Add(dir);
            }
            else
            {
                Vector3 prev = controlPoints[i - 1].position;
                Vector3 curr = controlPoints[i].position;
                Vector3 next = controlPoints[i + 1].position;

                Vector3 dirIn = (curr - prev).normalized;
                Vector3 dirOut = (next - curr).normalized;

                float distToPrev = Vector3.Distance(curr, prev);
                float distToNext = Vector3.Distance(curr, next);
                float maxRadius = Mathf.Min(distToPrev, distToNext) * 0.5f;
                float radius = Mathf.Min(cornerRadius, maxRadius);

                Vector3 p0 = curr - dirIn * radius;
                Vector3 p3 = curr + dirOut * radius;

                float controlDistance = radius * 0.552284749831f;
                Vector3 p1 = p0 + dirIn * controlDistance;
                Vector3 p2 = p3 - dirOut * controlDistance;

                for (int j = 0; j <= cornerSegments; j++)
                {
                    float t = j / (float)cornerSegments;
                    Vector3 point = CalculateCubicBezier(p0, p1, p2, p3, t);
                    Vector3 tangent = CalculateCubicBezierDerivative(p0, p1, p2, p3, t).normalized;

                    points.Add(point);
                    tangents.Add(tangent);
                }

                if (i < controlPoints.Length - 1)
                {
                    Vector3 nextCornerStart = next - dirOut * Mathf.Min(cornerRadius, distToNext * 0.5f);
                    float segmentLength = Vector3.Distance(p3, nextCornerStart);
                    int straightSegments = Mathf.Max(1, Mathf.CeilToInt(segmentLength / (pointSpacing * 0.5f)));

                    for (int j = 1; j < straightSegments; j++)
                    {
                        float t = j / (float)straightSegments;
                        points.Add(Vector3.Lerp(p3, nextCornerStart, t));
                        tangents.Add(dirOut);
                    }
                }
            }
        }
    }

    void ResamplePathEvenly(List<Vector3> rawPoints, List<Vector3> rawTangents)
    {
        if (rawPoints.Count < 2) return;

        float totalLength = 0f;
        List<float> segmentLengths = new List<float>();
        for (int i = 0; i < rawPoints.Count - 1; i++)
        {
            float length = Vector3.Distance(rawPoints[i], rawPoints[i + 1]);
            segmentLengths.Add(length);
            totalLength += length;
        }

        int targetPointCount = Mathf.Max(2, Mathf.CeilToInt(totalLength / pointSpacing));
        float actualSpacing = totalLength / (targetPointCount - 1);

        pathPoints.Add(rawPoints[0]);
        pathTangents.Add(rawTangents[0]);

        float currentDistance = actualSpacing;
        int currentSegment = 0;
        float segmentProgress = 0f;

        while (currentDistance < totalLength && pathPoints.Count < targetPointCount)
        {
            while (currentSegment < segmentLengths.Count &&
                   segmentProgress + segmentLengths[currentSegment] < currentDistance)
            {
                segmentProgress += segmentLengths[currentSegment];
                currentSegment++;
            }

            if (currentSegment >= segmentLengths.Count) break;

            float segmentStartDistance = segmentProgress;
            float segmentEndDistance = segmentProgress + segmentLengths[currentSegment];
            float t = (currentDistance - segmentStartDistance) / segmentLengths[currentSegment];

            Vector3 point = Vector3.Lerp(rawPoints[currentSegment], rawPoints[currentSegment + 1], t);
            Vector3 tangent = Vector3.Lerp(rawTangents[currentSegment], rawTangents[currentSegment + 1], t).normalized;

            pathPoints.Add(point);
            pathTangents.Add(tangent);

            currentDistance += actualSpacing;
        }

        if (pathPoints.Count < targetPointCount)
        {
            pathPoints.Add(rawPoints[rawPoints.Count - 1]);
            pathTangents.Add(rawTangents[rawTangents.Count - 1]);
        }
    }

    Mesh CreateOptimizedPipeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Optimized Pipe Mesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        List<int> outerTriangles = new List<int>();
        List<int> innerTriangles = new List<int>();

        // Chỉ tạo mesh tại các điểm cần thiết
        List<int> optimizedIndices = OptimizePathPoints();
        
        int outerVertexStart = vertices.Count;
        CreateOptimizedPipeMesh(vertices, outerTriangles, uvs, normals, pipeRadius, doubleSided, outerVertexStart, optimizedIndices);

        if (generateInnerPipe)
        {
            int innerVertexStart = vertices.Count;
            CreateOptimizedPipeMesh(vertices, innerTriangles, uvs, normals, innerPipeRadius, innerDoubleSided, innerVertexStart, optimizedIndices);
        }

        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();

        mesh.subMeshCount = generateInnerPipe ? 2 : 1;
        mesh.SetTriangles(outerTriangles, 0);
        if (generateInnerPipe)
        {
            mesh.SetTriangles(innerTriangles, 1);
        }

        mesh.RecalculateBounds();
        return mesh;
    }

    List<int> OptimizePathPoints()
    {
        List<int> optimizedIndices = new List<int>();
        
        if (pathPoints.Count < 2) return optimizedIndices;

        // Luôn thêm điểm đầu tiên
        optimizedIndices.Add(0);

        // Ngưỡng góc để xác định đoạn thẳng (cosine của góc)
        float straightThreshold = 0.999f; // ~2.5 độ

        for (int i = 1; i < pathPoints.Count - 1; i++)
        {
            Vector3 prevDir = (pathPoints[i] - pathPoints[i-1]).normalized;
            Vector3 nextDir = (pathPoints[i+1] - pathPoints[i]).normalized;
            
            float dot = Vector3.Dot(prevDir, nextDir);
            
            // Nếu góc đổi hướng đủ lớn (không phải đoạn thẳng), thêm điểm này
            if (dot < straightThreshold)
            {
                optimizedIndices.Add(i);
            }
        }

        // Luôn thêm điểm cuối cùng
        optimizedIndices.Add(pathPoints.Count - 1);

        return optimizedIndices;
    }

    void CreateOptimizedPipeMesh(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs, List<Vector3> normals,
                               float radius, bool isDoubleSided, int vertexOffset, List<int> optimizedIndices)
    {
        if (optimizedIndices.Count < 2) return;

        List<Vector3> frameNormals = new List<Vector3>();
        List<Vector3> frameBinormals = new List<Vector3>();
        CalculateRotationMinimizingFrames(frameNormals, frameBinormals);

        int startVertexIndex = vertices.Count;

        // Chỉ tạo vertices tại các điểm tối ưu
        for (int idx = 0; idx < optimizedIndices.Count; idx++)
        {
            int i = optimizedIndices[idx];
            Vector3 point = pathPoints[i];
            Vector3 normal = frameNormals[i];
            Vector3 binormal = frameBinormals[i];

            for (int j = 0; j <= radialSegments; j++)
            {
                float angle = (j / (float)radialSegments) * Mathf.PI * 2f;
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);

                Vector3 offset = (normal * x + binormal * y) * radius;
                Vector3 vertex = point + offset;
                vertices.Add(vertex);

                Vector3 normalDir = (vertex - point).normalized;
                normals.Add(normalDir);

                float u = j / (float)radialSegments;
                float v = idx / (float)(optimizedIndices.Count - 1);
                uvs.Add(new Vector2(u, v));
            }
        }

        // Tạo triangles giữa các điểm tối ưu
        for (int i = 0; i < optimizedIndices.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int current = startVertexIndex + i * (radialSegments + 1) + j;
                int next = current + radialSegments + 1;

                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(current + 1);

                triangles.Add(current + 1);
                triangles.Add(next);
                triangles.Add(next + 1);
            }
        }

        if (isDoubleSided)
        {
            int vertexCount = vertices.Count - startVertexIndex;
            int innerStartIndex = vertices.Count;

            for (int i = 0; i < vertexCount; i++)
            {
                vertices.Add(vertices[startVertexIndex + i]);
                normals.Add(-normals[startVertexIndex + i]);
                uvs.Add(uvs[startVertexIndex + i]);
            }

            for (int i = 0; i < optimizedIndices.Count - 1; i++)
            {
                for (int j = 0; j < radialSegments; j++)
                {
                    int current = innerStartIndex + i * (radialSegments + 1) + j;
                    int next = current + radialSegments + 1;

                    triangles.Add(current);
                    triangles.Add(current + 1);
                    triangles.Add(next);

                    triangles.Add(current + 1);
                    triangles.Add(next + 1);
                    triangles.Add(next);
                }
            }
        }
    }

    Vector3 CalculateCubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 point = uuu * p0;
        point += 3 * uu * t * p1;
        point += 3 * u * tt * p2;
        point += ttt * p3;

        return point;
    }

    Vector3 CalculateCubicBezierDerivative(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 derivative = -3 * uu * p0;
        derivative += 3 * uu * p1 - 6 * u * t * p1;
        derivative += 6 * u * t * p2 - 3 * tt * p2;
        derivative += 3 * tt * p3;

        return derivative;
    }

    void CalculateRotationMinimizingFrames(List<Vector3> outNormals, List<Vector3> outBinormals)
    {
        outNormals.Clear();
        outBinormals.Clear();

        if (pathPoints.Count == 0) return;

        Vector3 tangent0 = pathTangents[0];
        Vector3 reference = Mathf.Abs(Vector3.Dot(tangent0, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
        Vector3 binormal0 = Vector3.Cross(tangent0, reference).normalized;
        Vector3 normal0 = Vector3.Cross(binormal0, tangent0).normalized;

        outNormals.Add(normal0);
        outBinormals.Add(binormal0);

        for (int i = 1; i < pathPoints.Count; i++)
        {
            Vector3 prevNormal = outNormals[i - 1];
            Vector3 prevBinormal = outBinormals[i - 1];
            Vector3 prevTangent = pathTangents[i - 1];
            Vector3 currTangent = pathTangents[i];

            Vector3 v1 = pathPoints[i] - pathPoints[i - 1];

            if (Vector3.Dot(prevTangent, currTangent) > 0.9999f)
            {
                outNormals.Add(prevNormal);
                outBinormals.Add(prevBinormal);
                continue;
            }

            float c1 = 2.0f / Vector3.Dot(v1, v1);
            Vector3 rL = prevNormal - c1 * Vector3.Dot(v1, prevNormal) * v1;

            Vector3 tL = currTangent - prevTangent;

            float c2 = 2.0f / Vector3.Dot(tL, tL);
            Vector3 newNormal = rL - c2 * Vector3.Dot(tL, rL) * tL;
            newNormal.Normalize();

            Vector3 newBinormal = Vector3.Cross(currTangent, newNormal).normalized;

            outNormals.Add(newNormal);
            outBinormals.Add(newBinormal);
        }
    }

    void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Length < 2)
            return;

        if (showControlPoints)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < controlPoints.Length; i++)
            {
                if (controlPoints[i] != null)
                {
                    Gizmos.DrawSphere(controlPoints[i].position, 0.1f);
                }
            }

            Gizmos.color = Color.gray;
            for (int i = 0; i < controlPoints.Length - 1; i++)
            {
                if (controlPoints[i] != null && controlPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(controlPoints[i].position, controlPoints[i + 1].position);
                }
            }
        }

        if (showPolyline && pathPoints.Count > 1)
        {
            Gizmos.color = polylineColor;
            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            }
        }
    }
    [ContextMenu("Save Mesh to Asset")]
    public void SaveMeshToAsset()
    {
#if UNITY_EDITOR
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            Debug.LogWarning("No mesh to save!");
            return;
        }

        // Tạo một mesh mới để lưu vì mesh hiện tại có thể là mesh tạm thời
        Mesh meshToSave = Instantiate(meshFilter.sharedMesh);

        // Đảm bảo thư mục tồn tại
        string folderPath = "Assets/GeneratedMeshes";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedMeshes");
        }

        // Tạo tên file duy nhất
        string meshName = "TubeMesh_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string assetPath = folderPath + "/" + meshName + ".asset";

        // Lưu mesh
        AssetDatabase.CreateAsset(meshToSave, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log("Mesh saved at: " + assetPath);
#endif
    }
}