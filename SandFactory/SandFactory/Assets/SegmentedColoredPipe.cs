using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private MeshFilter currentMeshFilter;
    private float initialFrameTwist = 0f;

    // Lưu số lượng điểm ban đầu để tính toán segment
    private int initialPointCount = 0;

    // === MỚI: Lưu list index của từng segment màu ===
    [System.Serializable]
    public class SegmentIndexRange
    {
        public int totalPoints;
        public SegmentIndexRange( int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                this.totalPoints++;
            }
        }
    }

    // List lưu trữ thông tin index của từng segment
    public List<SegmentIndexRange> segmentIndexRanges = new List<SegmentIndexRange>();
    // === KẾT THÚC PHẦN MỚI ===

    void Start()
    {
        //GenerateSegmentedPipe();
    }

    bool isMoving = false;
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    if (isMoving) return;
        //    RemoveVertextList();
        //}
    }
    public void RemoveVertextList(float time ,int count = 1)
    {
        isMoving = true;

        int countRemove = 0;
        
        for(int i = 0; i < count; i++)
        {
            var lastSegment = segmentIndexRanges[^1];
            segmentIndexRanges.Remove(lastSegment);
            countRemove += lastSegment.totalPoints;
        } 

     

        float duration = time * count;
        float eachDuration = duration / countRemove;

        // Tạo sequence
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < countRemove; i++)
        {
            // Chèn vào sequence: delay rồi gọi Moving()
            seq.AppendInterval(eachDuration)
               .AppendCallback(() =>
               {
                   Moving(eachDuration); // gọi mỗi lần
               });
        }

        // Khi tất cả duration hoàn tất
        seq.OnComplete(() =>
        {
            isMoving = false;
        });

    }

    void OnValidate()
    {
        if (Application.isPlaying && roundedPolylinePipe != null)
        {
            //GenerateSegmentedPipe();
        }
    }

    [ContextMenu("Generate Segmented Pipe")]
    public void GenerateSegmentedPipe(List<Color> segmentColors)
    {
        this.segmentColors = new List<Color>(segmentColors);
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

        // Lưu số lượng điểm ban đầu
        initialPointCount = pathPoints.Count;

        // === MỚI: Tính toán và lưu index ranges cho từng segment ===
        CalculateSegmentIndexRanges();
        // === KẾT THÚC PHẦN MỚI ===

        CalculateRotationMinimizingFrames();
        CreateSegmentedPipeMesh();
    }

    // === MỚI: Phương thức tính toán index ranges ===
    void CalculateSegmentIndexRanges()
    {
        segmentIndexRanges.Clear();

        float pointsPerSegment = (float)initialPointCount / segmentColors.Count;

        for (int segIdx = 0; segIdx < segmentColors.Count; segIdx++)
        {
            int startIdx = Mathf.RoundToInt(segIdx * pointsPerSegment);
            int endIdx = Mathf.RoundToInt((segIdx + 1) * pointsPerSegment) - 1;

            // Đảm bảo segment cuối cùng bao gồm tất cả điểm còn lại
            if (segIdx == segmentColors.Count - 1)
            {
                endIdx = initialPointCount - 1;
            }

            SegmentIndexRange range = new SegmentIndexRange(
                startIdx,
                endIdx
            );

            segmentIndexRanges.Add(range);

            Debug.Log($"Segment {segIdx} (Color: {segmentColors[segIdx]}): Points {startIdx} to {endIdx} ({range.totalPoints} points)");
        }
    }


    // === KẾT THÚC PHẦN MỚI ===

    void GetPathFromRoundedPolyline()
    {
        System.Type polylineType = typeof(RoundedPolylinePipe);
        var pathPointsField = polylineType.GetField("pathPoints",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pathTangentsField = polylineType.GetField("pathTangents",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (pathPointsField != null && pathTangentsField != null)
        {
            pathPoints = new List<Vector3>((List<Vector3>)pathPointsField.GetValue(roundedPolylinePipe));
            pathTangents = new List<Vector3>((List<Vector3>)pathTangentsField.GetValue(roundedPolylinePipe));
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
        currentMeshFilter = null;
        segmentIndexRanges.Clear(); // Xóa luôn index ranges
    }

    void CreateSegmentedPipeMesh()
    {
        GameObject pipeObject = new GameObject("ColoredPipe");
        pipeObject.transform.SetParent(transform);
        pipeObject.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = pipeObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = pipeObject.AddComponent<MeshRenderer>();

        Mesh pipeMesh = CreateCurvedPipeMesh();
        meshFilter.mesh = pipeMesh;
        currentMeshFilter = meshFilter;

        Material[] materials = new Material[segmentColors.Count];
        for (int i = 0; i < segmentColors.Count; i++)
        {
            materials[i] = new Material(pipeMaterial != null ? pipeMaterial : new Material(Shader.Find("Standard")));
            materials[i].color = segmentColors[i];
        }
        meshRenderer.materials = materials;

        segmentObjects.Add(pipeObject);
    }

    void Moving(float duration)
    {
        if (pathPoints.Count < 3) return;

        float distance = Vector3.Distance(pathPoints[0], pathPoints[1]);
        Vector3 saving = Vector3.zero;

        // Di chuyển các điểm theo thuật toán của bạn
        for (int i = pathPoints.Count - 1; i >= 0; i--)
        {
            var temp = pathPoints[i];
            if (i == pathPoints.Count - 1)
            {
                pathPoints[i] -= new Vector3(0, distance, 0);
            }
            else
            {
                pathPoints[i] = saving;
            }
            saving = temp;
        }

        // Cắt bỏ điểm cuối cùng
        pathPoints.RemoveAt(pathPoints.Count - 1);
        pathTangents.RemoveAt(pathTangents.Count - 1);

        if (pathNormals.Count > 0)
            pathNormals.RemoveAt(pathNormals.Count - 1);
        if (pathBinormals.Count > 0)
            pathBinormals.RemoveAt(pathBinormals.Count - 1);

        // Cập nhật mesh
        UpdatePipeMesh();

    }
    


    void RecalculateTangents()
    {
        pathTangents.Clear();

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 tangent;

            if (i == 0)
            {
                tangent = (pathPoints[i + 1] - pathPoints[i]).normalized;
            }
            else if (i == pathPoints.Count - 1)
            {
                tangent = (pathPoints[i] - pathPoints[i - 1]).normalized;
            }
            else
            {
                Vector3 tangent1 = (pathPoints[i] - pathPoints[i - 1]).normalized;
                Vector3 tangent2 = (pathPoints[i + 1] - pathPoints[i]).normalized;
                tangent = (tangent1 + tangent2).normalized;
            }

            pathTangents.Add(tangent);
        }
    }

    void UpdatePipeMesh()
    {
        if (currentMeshFilter == null)
        {
            Debug.LogWarning("No mesh filter to update!");
            return;
        }

        if (pathNormals.Count > 0 && pathTangents.Count > 0)
        {
            Vector3 oldNormal = pathNormals[0];
            Vector3 oldTangent = pathTangents[0];

            RecalculateTangents();

            Vector3 newTangent = pathTangents[0];
            Vector3 reference = Mathf.Abs(Vector3.Dot(newTangent, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
            Vector3 newBinormal = Vector3.Cross(newTangent, reference).normalized;
            Vector3 newNormal = Vector3.Cross(newBinormal, newTangent).normalized;

            Vector3 projectedOldNormal = (oldNormal - Vector3.Dot(oldNormal, newTangent) * newTangent).normalized;
            float angle = Mathf.Atan2(Vector3.Dot(Vector3.Cross(newNormal, projectedOldNormal), newTangent),
                                       Vector3.Dot(newNormal, projectedOldNormal));
            initialFrameTwist = angle;
        }
        else
        {
            RecalculateTangents();
        }

        CalculateRotationMinimizingFrames();
        Mesh updatedMesh = CreateCurvedPipeMesh();
        currentMeshFilter.mesh = updatedMesh;
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

        // Tính segment dựa trên số điểm ban đầu, không phải số điểm hiện tại
        float pointsPerSegment = (float)initialPointCount / segmentColors.Count;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 point = pathPoints[i];
            Vector3 tangent = pathTangents[i];
            Vector3 normal = pathNormals[i];
            Vector3 binormal = pathBinormals[i];

            // Tính segment index dựa trên vị trí trong mảng ban đầu
            // Điểm ở đầu mảng thuộc segment đầu tiên
            int currentSegmentIndex = Mathf.Min((int)(i / pointsPerSegment), segmentColors.Count - 1);

            int circleStartIndex = vertices.Count;

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

            if (i > 0)
            {
                int prevCircleStart = circleStartIndex - radialSegments;

                for (int j = 0; j < radialSegments; j++)
                {
                    int current = circleStartIndex + j;
                    int next = circleStartIndex + (j + 1) % radialSegments;
                    int prevCurrent = prevCircleStart + j;
                    int prevNext = prevCircleStart + (j + 1) % radialSegments;

                    int quadSegmentIndex = currentSegmentIndex;

                    trianglesPerSegment[quadSegmentIndex].Add(prevCurrent);
                    trianglesPerSegment[quadSegmentIndex].Add(next);
                    trianglesPerSegment[quadSegmentIndex].Add(current);

                    trianglesPerSegment[quadSegmentIndex].Add(prevCurrent);
                    trianglesPerSegment[quadSegmentIndex].Add(prevNext);
                    trianglesPerSegment[quadSegmentIndex].Add(next);
                }
            }
        }

        CreateStartCap(vertices, trianglesPerSegment[0], normals, uvs, pathPoints[0],
                      pathTangents[0], pathNormals[0], pathBinormals[0]);

        //int lastSegmentIndex = Mathf.Min((int)((pathPoints.Count - 1) / pointsPerSegment), segmentColors.Count - 1);
        //CreateEndCap(vertices, trianglesPerSegment[lastSegmentIndex], normals, uvs,
        //            pathPoints[pathPoints.Count - 1], pathTangents[pathPoints.Count - 1],
        //            pathNormals[pathPoints.Count - 1], pathBinormals[pathPoints.Count - 1]);

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

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

        Vector3 tangent0 = pathTangents[0];
        Vector3 reference = Mathf.Abs(Vector3.Dot(tangent0, Vector3.up)) > 0.9f ? Vector3.forward : Vector3.up;
        Vector3 binormal0 = Vector3.Cross(tangent0, reference).normalized;
        Vector3 normal0 = Vector3.Cross(binormal0, tangent0).normalized;

        if (initialFrameTwist != 0f)
        {
            Quaternion twistRotation = Quaternion.AngleAxis(initialFrameTwist * Mathf.Rad2Deg, tangent0);
            normal0 = twistRotation * normal0;
            binormal0 = twistRotation * binormal0;
        }

        pathNormals.Add(normal0);
        pathBinormals.Add(binormal0);

        for (int i = 1; i < pathPoints.Count; i++)
        {
            Vector3 prevNormal = pathNormals[i - 1];
            Vector3 prevTangent = pathTangents[i - 1];
            Vector3 currTangent = pathTangents[i];

            if (Vector3.Dot(prevTangent, currTangent) > 0.9999f)
            {
                pathNormals.Add(prevNormal);
                pathBinormals.Add(pathBinormals[i - 1]);
                continue;
            }

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