using UnityEngine;
using System.Collections.Generic;

public class SegmentedColoredPipe : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private RoundedPolylinePipe roundedPolylinePipe;

    [Header("Segmentation Settings")]
    [SerializeField] private List<Color> segmentColors = new List<Color>();
    [SerializeField] private float pipeRadius = 0.5f;
    [SerializeField] private int radialSegments = 16;

    [Header("Material Settings")]
    [SerializeField] private Material pipeMaterial;

    private List<GameObject> segmentObjects = new List<GameObject>();
    private List<Vector3> pathPoints = new List<Vector3>();
    private List<Vector3> pathTangents = new List<Vector3>();
    private List<Vector3> pathNormals = new List<Vector3>();
    private List<Vector3> pathBinormals = new List<Vector3>();

    void Start()
    {
        GenerateSegmentedPipe();
    }

    void OnValidate()
    {
        if (Application.isPlaying && roundedPolylinePipe != null)
        {
            GenerateSegmentedPipe();
        }
    }

    [ContextMenu("Generate Segmented Pipe")]
    public void GenerateSegmentedPipe()
    {
        ClearSegments();

        if (roundedPolylinePipe == null)
        {
            Debug.LogWarning("RoundedPolylinePipe reference is missing!");
            return;
        }

        if (segmentColors == null || segmentColors.Count == 0)
        {
            Debug.LogWarning("Need at least one segment color!");
            return;
        }

        GetPathFromRoundedPolyline();

        if (pathPoints.Count < 2)
        {
            Debug.LogWarning("Not enough path points!");
            return;
        }

        CalculateRotationMinimizingFrames();
        CreateSegmentedPipeMesh();
    }

    void GetPathFromRoundedPolyline()
    {
        System.Type polylineType = typeof(RoundedPolylinePipe);
        var pathPointsField = polylineType.GetField("pathPoints",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pathTangentsField = polylineType.GetField("pathTangents",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (pathPointsField != null && pathTangentsField != null)
        {
            pathPoints = (List<Vector3>)pathPointsField.GetValue(roundedPolylinePipe);
            pathTangents = (List<Vector3>)pathTangentsField.GetValue(roundedPolylinePipe);
        }
        else
        {
            Debug.LogError("Could not access path data from RoundedPolylinePipe!");
        }
    }

    void ClearSegments()
    {
        foreach (GameObject segment in segmentObjects)
        {
            if (segment != null)
            {
                if (Application.isPlaying)
                    Destroy(segment);
                else
                    DestroyImmediate(segment);
            }
        }
        segmentObjects.Clear();
    }

    void CreateSegmentedPipeMesh()
    {
        // Tạo một mesh duy nhất cho cả đường ống
        GameObject pipeObject = new GameObject("ColoredPipe");
        pipeObject.transform.SetParent(transform);
        pipeObject.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = pipeObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = pipeObject.AddComponent<MeshRenderer>();

        Mesh pipeMesh = CreateCurvedPipeMesh();
        meshFilter.mesh = pipeMesh;

        // Tạo materials cho từng segment
        Material[] materials = new Material[segmentColors.Count];
        for (int i = 0; i < segmentColors.Count; i++)
        {
            materials[i] = new Material(pipeMaterial != null ? pipeMaterial : new Material(Shader.Find("Standard")));
            materials[i].color = segmentColors[i];
        }
        meshRenderer.materials = materials;

        segmentObjects.Add(pipeObject);
    }

    Mesh CreateCurvedPipeMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Curved Segmented Pipe";

        List<Vector3> vertices = new List<Vector3>();
        List<int>[] trianglesPerSegment = new List<int>[segmentColors.Count];
        for (int i = 0; i < segmentColors.Count; i++)
        {
            trianglesPerSegment[i] = new List<int>();
        }
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        // Tính toán độ dài từng đoạn
        float totalLength = CalculatePathLength();
        float segmentLength = totalLength / segmentColors.Count;

        // Tạo vòng tròn vertices cho mỗi điểm trên path
        float accumulatedLength = 0f;
        int currentSegmentIndex = 0;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 point = pathPoints[i];
            Vector3 tangent = pathTangents[i];
            Vector3 normal = pathNormals[i];
            Vector3 binormal = pathBinormals[i];

            // Xác định segment hiện tại dựa trên độ dài tích lũy
            if (i > 0)
            {
                accumulatedLength += Vector3.Distance(pathPoints[i - 1], pathPoints[i]);
                currentSegmentIndex = Mathf.Min((int)(accumulatedLength / segmentLength), segmentColors.Count - 1);
            }

            int circleStartIndex = vertices.Count;

            // Tạo vòng tròn vertices
            for (int j = 0; j < radialSegments; j++)
            {
                float angle = j / (float)radialSegments * Mathf.PI * 2f;
                float x = Mathf.Cos(angle);
                float y = Mathf.Sin(angle);

                Vector3 localOffset = (normal * x + binormal * y) * pipeRadius;
                Vector3 vertex = point + localOffset;
                vertices.Add(vertex);

                Vector3 vertexNormal = localOffset.normalized;
                normals.Add(vertexNormal);

                float u = j / (float)radialSegments;
                float v = i / (float)(pathPoints.Count - 1);
                uvs.Add(new Vector2(u, v));
            }

            // Tạo triangles nối với vòng tròn trước đó
            if (i > 0)
            {
                int prevCircleStart = circleStartIndex - radialSegments;

                for (int j = 0; j < radialSegments; j++)
                {
                    int current = circleStartIndex + j;
                    int next = circleStartIndex + (j + 1) % radialSegments;
                    int prevCurrent = prevCircleStart + j;
                    int prevNext = prevCircleStart + (j + 1) % radialSegments;

                    // Xác định segment cho cặp quad này
                    int quadSegmentIndex = currentSegmentIndex;

                    // Thêm triangles vào segment tương ứng (đảo ngược để normal hướng ra ngoài)
                    trianglesPerSegment[quadSegmentIndex].Add(prevCurrent);
                    trianglesPerSegment[quadSegmentIndex].Add(next);
                    trianglesPerSegment[quadSegmentIndex].Add(current);

                    trianglesPerSegment[quadSegmentIndex].Add(prevCurrent);
                    trianglesPerSegment[quadSegmentIndex].Add(prevNext);
                    trianglesPerSegment[quadSegmentIndex].Add(next);
                }
            }
        }

        // Tạo end caps
        CreateStartCap(vertices, trianglesPerSegment[0], normals, uvs, pathPoints[0],
                      pathTangents[0], pathNormals[0], pathBinormals[0]);

        CreateEndCap(vertices, trianglesPerSegment[segmentColors.Count - 1], normals, uvs,
                    pathPoints[pathPoints.Count - 1], pathTangents[pathPoints.Count - 1],
                    pathNormals[pathPoints.Count - 1], pathBinormals[pathPoints.Count - 1]);

        // Gán vertices và normals
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        // Gán submeshes cho từng segment
        mesh.subMeshCount = segmentColors.Count;
        for (int i = 0; i < segmentColors.Count; i++)
        {
            mesh.SetTriangles(trianglesPerSegment[i].ToArray(), i);
        }

        mesh.RecalculateBounds();
        return mesh;
    }

    void CreateStartCap(List<Vector3> vertices, List<int> triangles, List<Vector3> normals,
                       List<Vector2> uvs, Vector3 center, Vector3 tangent, Vector3 normal, Vector3 binormal)
    {
        int centerIndex = vertices.Count;
        Vector3 capNormal = -tangent;

        vertices.Add(center);
        normals.Add(capNormal);
        uvs.Add(new Vector2(0.5f, 0.5f));

        int circleStartIndex = vertices.Count;

        for (int i = 0; i < radialSegments; i++)
        {
            float angle = i / (float)radialSegments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            Vector3 localOffset = (normal * x + binormal * y) * pipeRadius;
            Vector3 vertex = center + localOffset;
            vertices.Add(vertex);

            normals.Add(capNormal);
            uvs.Add(new Vector2((x + 1) * 0.5f, (y + 1) * 0.5f));
        }

        for (int i = 0; i < radialSegments; i++)
        {
            int current = circleStartIndex + i;
            int next = circleStartIndex + (i + 1) % radialSegments;

            triangles.Add(centerIndex);
            triangles.Add(next);
            triangles.Add(current);
        }
    }

    void CreateEndCap(List<Vector3> vertices, List<int> triangles, List<Vector3> normals,
                     List<Vector2> uvs, Vector3 center, Vector3 tangent, Vector3 normal, Vector3 binormal)
    {
        int centerIndex = vertices.Count;
        Vector3 capNormal = tangent;

        vertices.Add(center);
        normals.Add(capNormal);
        uvs.Add(new Vector2(0.5f, 0.5f));

        int circleStartIndex = vertices.Count;

        for (int i = 0; i < radialSegments; i++)
        {
            float angle = i / (float)radialSegments * Mathf.PI * 2f;
            float x = Mathf.Cos(angle);
            float y = Mathf.Sin(angle);

            Vector3 localOffset = (normal * x + binormal * y) * pipeRadius;
            Vector3 vertex = center + localOffset;
            vertices.Add(vertex);

            normals.Add(capNormal);
            uvs.Add(new Vector2((x + 1) * 0.5f, (y + 1) * 0.5f));
        }

        for (int i = 0; i < radialSegments; i++)
        {
            int current = circleStartIndex + i;
            int next = circleStartIndex + (i + 1) % radialSegments;

            triangles.Add(centerIndex);
            triangles.Add(current);
            triangles.Add(next);
        }
    }

    float CalculatePathLength()
    {
        float length = 0f;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            length += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        return length;
    }

    void CalculateRotationMinimizingFrames()
    {
        pathNormals.Clear();
        pathBinormals.Clear();

        if (pathPoints.Count == 0) return;

        // Frame ban đầu
        Vector3 tangent0 = pathTangents[0];
        Vector3 reference = Mathf.Abs(Vector3.Dot(tangent0, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
        Vector3 binormal0 = Vector3.Cross(tangent0, reference).normalized;
        Vector3 normal0 = Vector3.Cross(binormal0, tangent0).normalized;

        pathNormals.Add(normal0);
        pathBinormals.Add(binormal0);

        // Tính toán frame cho các điểm còn lại
        for (int i = 1; i < pathPoints.Count; i++)
        {
            Vector3 prevNormal = pathNormals[i - 1];
            Vector3 prevTangent = pathTangents[i - 1];
            Vector3 currTangent = pathTangents[i];

            // Nếu tangent gần như không đổi, giữ nguyên frame
            if (Vector3.Dot(prevTangent, currTangent) > 0.9999f)
            {
                pathNormals.Add(prevNormal);
                pathBinormals.Add(pathBinormals[i - 1]);
                continue;
            }

            // Rotation Minimizing Frame algorithm
            Vector3 v1 = pathPoints[i] - pathPoints[i - 1];
            float c1 = 2.0f / Vector3.Dot(v1, v1);
            Vector3 rL = prevNormal - c1 * Vector3.Dot(v1, prevNormal) * v1;

            Vector3 tL = currTangent - prevTangent;
            float c2 = 2.0f / Vector3.Dot(tL, tL);
            Vector3 newNormal = rL - c2 * Vector3.Dot(tL, rL) * tL;
            newNormal.Normalize();

            Vector3 newBinormal = Vector3.Cross(currTangent, newNormal).normalized;
            pathNormals.Add(newNormal);
            pathBinormals.Add(newBinormal);
        }
    }

    [ContextMenu("Clear Segments")]
    public void ClearAllSegments()
    {
        ClearSegments();
    }
}