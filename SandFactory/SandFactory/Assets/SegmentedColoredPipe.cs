using DG.Tweening;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SegmentedColoredPipe : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private RoundedPolylinePipe roundedPolylinePipe;

    [Header("Segmentation Settings")]
    //[SerializeField] private List<Color> segmentColors = new List<Color>();
    public ColorID colors;
    [SerializeField] private float pipeRadius = 0.5f;
    [SerializeField] private int radialSegments = 16;

    [Header("Material Settings")]
    [SerializeField] private Material pipeMaterial;

    [Header("Render Settings")]
    [SerializeField] private int maxRenderPoints = 50; // Số điểm tối đa để render mesh
    [SerializeField] private int maxInitialColors = 30; // Số màu tối đa dùng path ban đầu

    private List<GameObject> segmentObjects = new List<GameObject>();
    private List<Vector3> pathPoints = new List<Vector3>();
    private List<Vector3> pathTangents = new List<Vector3>();
    private List<Vector3> pathNormals = new List<Vector3>();
    private List<Vector3> pathBinormals = new List<Vector3>();

    private MeshFilter currentMeshFilter;
    private float initialFrameTwist = 0f;

    // Lưu số lượng điểm ban đầu để tính toán segment
    private int initialPointCount = 0;

    [System.Serializable]
    public class SegmentIndexRange
    {
        public int totalPoints;
        public int colorID;
        public int index;
        public SegmentIndexRange(int start, int end, int colorID, int index)
        {
            for (int i = start; i <= end; i++)
            {
                this.totalPoints++;
            }

            this.colorID = colorID;
            this.index = index;
        }
    }

    public List<SegmentIndexRange> segmentIndexRanges = new List<SegmentIndexRange>();
    void Start()
    {
        //GenerateSegmentedPipe();
    }

    bool isMoving = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SaveMeshToAsset();
        }
    }

    public void RemoveVertextList(float time, int count = 1)
    {
        isMoving = true;

        // Tạo sequence
        Sequence seq = DOTween.Sequence();

        for (int segmentIdx = 0; segmentIdx < count; segmentIdx++)
        {
            var lastSegment = segmentIndexRanges[^(segmentIdx + 1)]; // Lấy segment từ cuối
            int pointsInThisSegment = lastSegment.totalPoints;
            float eachDuration = time / pointsInThisSegment;

            // Thêm animation cho từng point trong segment này
            for (int pointIdx = 0; pointIdx < pointsInThisSegment; pointIdx++)
            {
                seq.AppendInterval(eachDuration)
                   .AppendCallback(() =>
                   {
                       Moving(eachDuration);
                   });
            }

            // Sau khi hoàn thành segment này, gọi RemoveLastMaterialAndShowHiddenColor
            seq.AppendCallback(() =>
            {
                segmentIndexRanges.RemoveAt(segmentIndexRanges.Count - 1);
                RemoveLastMaterialAndShowHiddenColor();
            });
        }

        // Khi tất cả hoàn tất
        seq.OnComplete(() =>
        {
            isMoving = false;
        });
    }
    public void RemoveLastMaterialAndShowHiddenColor()
    {
        if (segmentIndexRanges[segmentIndexRanges.Count - 1].colorID < 0)
        {
            List<SegmentIndexRange> segments = new List<SegmentIndexRange>();

            segments.Add(segmentIndexRanges[segmentIndexRanges.Count - 1]);

            if (segmentIndexRanges.Count > 1)
            {
                for (int i = segmentIndexRanges.Count - 2; i >= 0; i--)
                {
                    if (segmentIndexRanges[i].colorID == segments[0].colorID)
                    {
                        segments.Add(segmentIndexRanges[i]);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].colorID = Mathf.Abs(segments[i].colorID);
                materials[segments[i].index].DOColor(colors.colorWithIDs[segments[i].colorID].surfaceColor, 0.2f);
            }

        }
    }

    void OnValidate()
    {
        if (Application.isPlaying && roundedPolylinePipe != null)
        {
            GenerateSegmentedPipe(segmentColorIndexs);
        }
    }

    public List<int> segmentColorIndexs = new List<int>();

    [ContextMenu("Generate Segmented Pipe")]
   
    public void GenerateSegmentedPipe(List<int> segmentColors)
    {
        this.segmentColorIndexs = new List<int>(segmentColors);

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

        // === MỚI: Chia path ban đầu cho 30 màu đầu, thêm điểm cho các màu còn lại ===
        AddPointsForExtraColors();

        // Lưu số lượng điểm ban đầu (sau khi đã thêm điểm)
        initialPointCount = pathPoints.Count;

        // Tính toán và lưu index ranges cho từng segment
        CalculateSegmentIndexRanges();

        CalculateRotationMinimizingFrames();
        CreateSegmentedPipeMesh();
    }

    // === MỚI: Thêm điểm vào đầu path cho các màu vượt quá 30 ===
    void AddPointsForExtraColors()
    {
        if (segmentColorIndexs.Count <= maxInitialColors)
        {
            // Nếu số màu <= 30, dùng toàn bộ path ban đầu
            Debug.Log($"Using all {pathPoints.Count} points for {segmentColorIndexs.Count} colors");
            return;
        }

        // Số màu cần thêm điểm
        int extraColors = segmentColorIndexs.Count - maxInitialColors;
     
        // Tính số điểm cần thêm (giả sử mỗi màu thêm cần ít nhất 10 điểm)
        int pointsPerExtraColor = Mathf.Max(10, pathPoints.Count / maxInitialColors);
        int pointsToAdd = extraColors * pointsPerExtraColor;

        maxRenderPoints = pointsPerExtraColor * maxInitialColors;

        Debug.Log($"Adding {pointsToAdd} points for {extraColors} extra colors (total colors: {segmentColorIndexs.Count})");

        float distance = Vector3.Distance(pathPoints[0], pathPoints[1]);

        // Thêm điểm vào đầu path
        for (int i = 0; i < pointsToAdd; i++)
        {
            Vector3 p0 = pathPoints[0];
            Vector3 newp0 = p0 + new Vector3(0, distance, 0);
            pathPoints.Insert(0, newp0);
            pathTangents.Insert(0, pathTangents[0]);
        }

        Debug.Log($"Total points after adding: {pathPoints.Count}");
    }

    // === MỚI: Phương thức tính toán index ranges ===
    void CalculateSegmentIndexRanges()
    {
        segmentIndexRanges.Clear();

        float pointsPerSegment = (float)initialPointCount / segmentColorIndexs.Count;

        for (int segIdx = 0; segIdx < segmentColorIndexs.Count; segIdx++)
        {
            int startIdx = Mathf.RoundToInt(segIdx * pointsPerSegment);
            int endIdx = Mathf.RoundToInt((segIdx + 1) * pointsPerSegment) - 1;

            // Đảm bảo segment cuối cùng bao gồm tất cả điểm còn lại
            if (segIdx == segmentColorIndexs.Count - 1)
            {
                endIdx = initialPointCount - 1;
            }

            SegmentIndexRange range = new SegmentIndexRange(startIdx, endIdx, segmentColorIndexs[segIdx], segIdx);
            segmentIndexRanges.Add(range);

            Debug.Log($"Segment {segIdx} (Color: {segmentColorIndexs[segIdx]}): Points {startIdx} to {endIdx} ({range.totalPoints} points)");
        }
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
    Material[] materials;
    MeshRenderer meshRenderer;
    void CreateSegmentedPipeMesh()
    {
        GameObject pipeObject = new GameObject("ColoredPipe");
        pipeObject.layer = LayerMask.NameToLayer("Tile");
        pipeObject.transform.SetParent(transform);
        pipeObject.transform.localPosition = Vector3.zero;

        MeshFilter meshFilter = pipeObject.AddComponent<MeshFilter>();
        meshRenderer = pipeObject.AddComponent<MeshRenderer>();

        Mesh pipeMesh = CreateCurvedPipeMesh();
        meshFilter.mesh = pipeMesh;
        currentMeshFilter = meshFilter;

        materials = new Material[segmentColorIndexs.Count];
        for (int i = 0; i < segmentColorIndexs.Count; i++)
        {
            materials[i] = new Material(pipeMaterial != null ? pipeMaterial : new Material(Shader.Find("Standard")));
            if (segmentColorIndexs[i] >= 0)
            {
                materials[i].color = colors.colorWithIDs[segmentColorIndexs[i]].surfaceColor;
            }
            else
            {
                materials[i].color = Color.gray;
            }
            //materials[i].color = segmentColors[i];
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

        if (pathPoints.Count == 0) return;

        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 tangent;

            if (i == 0)
            {
                // Điểm đầu: dùng vector từ điểm 0 đến 1
                tangent = (pathPoints[1] - pathPoints[0]).normalized;
            }
            else if (i == pathPoints.Count - 1)
            {
                // Điểm cuối: dùng vector từ điểm n-2 đến n-1
                tangent = (pathPoints[i] - pathPoints[i - 1]).normalized;
            }
            else
            {
                // Điểm giữa: tính trung bình của hai vector
                Vector3 toPrev = (pathPoints[i] - pathPoints[i - 1]).normalized;
                Vector3 toNext = (pathPoints[i + 1] - pathPoints[i]).normalized;

                // Ưu tiên hướng về phía trước
                tangent = (toPrev + toNext).normalized;

                // Nếu vector gần bằng 0, sử dụng hướng từ điểm trước
                if (tangent.magnitude < 0.001f)
                {
                    tangent = toPrev;
                }
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
        List<int>[] trianglesPerSegment = new List<int>[segmentColorIndexs.Count];
        for (int i = 0; i < segmentColorIndexs.Count; i++)
        {
            trianglesPerSegment[i] = new List<int>();
        }
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        float pointsPerSegment = (float)initialPointCount / segmentColorIndexs.Count;

        // Xác định số điểm cần render (tối đa maxRenderPoints điểm cuối)
        int pointsToRender = Mathf.Min(maxRenderPoints, pathPoints.Count);
        int startIndex = pathPoints.Count - pointsToRender; // Bắt đầu từ điểm thứ (count - maxRenderPoints)

        for (int i = startIndex; i < pathPoints.Count; i++)
        {
            Vector3 point = pathPoints[i];
            Vector3 tangent = pathTangents[i];
            Vector3 normal = pathNormals[i];
            Vector3 binormal = pathBinormals[i];

            // Tính segment index dựa trên vị trí trong mảng ban đầu
            int currentSegmentIndex = Mathf.Min((int)(i / pointsPerSegment), segmentColorIndexs.Count - 1);

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
                float v = (i - startIndex) / (float)(pointsToRender - 1); // Điều chỉnh UV mapping
                uvs.Add(new Vector2(u, v));
            }

            // Chỉ tạo triangles nếu không phải điểm đầu tiên của đoạn render
            if (i > startIndex)
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

        // Tạo start cap cho điểm đầu tiên của đoạn render
        if (pointsToRender > 0)
        {
            CreateStartCap(vertices, trianglesPerSegment[0], normals, uvs, pathPoints[startIndex],
                          pathTangents[startIndex], pathNormals[startIndex], pathBinormals[startIndex]);
        }

        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.subMeshCount = segmentColorIndexs.Count;
        for (int i = 0; i < segmentColorIndexs.Count; i++)
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

        // Khởi tạo frame đầu tiên
        Vector3 tangent0 = pathTangents[0];
        Vector3 normal0, binormal0;

        // Chọn reference vector tránh song song với tangent
        Vector3 reference = Mathf.Abs(Vector3.Dot(tangent0, Vector3.up)) < 0.9f ? Vector3.up : Vector3.forward;

        binormal0 = Vector3.Cross(tangent0, reference).normalized;
        normal0 = Vector3.Cross(binormal0, tangent0).normalized;

        pathNormals.Add(normal0);
        pathBinormals.Add(binormal0);

        // Tính toán các frame tiếp theo sử dụng Parallel Transport
        for (int i = 1; i < pathPoints.Count; i++)
        {
            Vector3 prevTangent = pathTangents[i - 1];
            Vector3 currTangent = pathTangents[i];
            Vector3 prevNormal = pathNormals[i - 1];

            // Sử dụng phép quay để chuyển từ frame trước sang frame hiện tại
            Quaternion rotation = Quaternion.FromToRotation(prevTangent, currTangent);
            Vector3 newNormal = rotation * prevNormal;

            // Đảm bảo tính trực giao
            Vector3 newBinormal = Vector3.Cross(currTangent, newNormal).normalized;
            newNormal = Vector3.Cross(newBinormal, currTangent).normalized;

            pathNormals.Add(newNormal);
            pathBinormals.Add(newBinormal);
        }
    }

    [ContextMenu("Clear Segments")]
    public void ClearAllSegments()
    {
        ClearSegments();
    }
    

    [ContextMenu("Save Mesh to Asset")]
    public void SaveMeshToAsset()
    {
#if UNITY_EDITOR
        if (currentMeshFilter == null || currentMeshFilter.sharedMesh == null)
        {
            Debug.LogWarning("No mesh to save!");
            return;
        }

        // Tạo một mesh mới để lưu vì mesh hiện tại có thể là mesh tạm thời
        Mesh meshToSave = Instantiate(currentMeshFilter.sharedMesh);

        // Đảm bảo thư mục tồn tại
        string folderPath = "Assets/GeneratedMeshes";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedMeshes");
        }

        // Tạo tên file duy nhất
        string meshName = "PipeMesh_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string assetPath = folderPath + "/" + meshName + ".asset";

        // Lưu mesh
        AssetDatabase.CreateAsset(meshToSave, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log("Mesh saved at: " + assetPath);
#else
        Debug.LogWarning("Saving mesh is only supported in the Editor.");
#endif
    }
    [ContextMenu("Clear All Data")]
    public void ClearAllData()
    {
        ClearSegments();

        // Clear all path data
        pathPoints.Clear();
        pathTangents.Clear();
        pathNormals.Clear();
        pathBinormals.Clear();

        // Clear segment index ranges
        segmentIndexRanges.Clear();

        // Clear color indexes
        segmentColorIndexs.Clear();

        // Reset initial point count
        initialPointCount = 0;

        // Clear materials array
        materials = null;

        // Clear mesh filter reference
        currentMeshFilter = null;

        Debug.Log("All pipe data cleared!");
    }

    // Cũng có thể cập nhật hàm ClearSegments hiện tại để xóa mesh hoàn toàn
    [ContextMenu("Clear Segments")]
    public void ClearAll()
    {
        ClearAllData();

        // Additional cleanup for any remaining mesh components
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter mf in meshFilters)
        {
            if (mf.mesh != null)
            {
                if (Application.isPlaying)
                    Destroy(mf.mesh);
                else
                    DestroyImmediate(mf.mesh);
            }
        }

        // Destroy all child objects
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        segmentObjects.Clear();
    }
}