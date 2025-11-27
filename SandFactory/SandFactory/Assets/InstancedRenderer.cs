using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class InstancedRenderer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _autoCollectOnStart = true;
    [SerializeField] private bool _includeInactiveChildren = false;
    [SerializeField] private int _maxInstancesPerBatch = 1023;

    private Dictionary<BatchKey, BatchData> _batches = new Dictionary<BatchKey, BatchData>();
    private List<Renderer> _originalRenderers = new List<Renderer>();

    // Struct để làm key cho dictionary
    private struct BatchKey
    {
        public Mesh Mesh;
        public Material Material;

        public BatchKey(Mesh mesh, Material material)
        {
            Mesh = mesh;
            Material = material;
        }
    }

    // Class để lưu trữ batch data
    private class BatchData
    {
        public List<Matrix4x4> matrices = new List<Matrix4x4>();
        public List<MaterialPropertyBlock> propertyBlocks = new List<MaterialPropertyBlock>();
    }

    void Start()
    {
        if (_autoCollectOnStart)
        {
            CollectAndBatchObjects();
        }
    }

    void Update()
    {
        RenderBatches();
    }

    [ContextMenu("Collect and Batch Objects")]
    public void CollectAndBatchObjects()
    {
        ClearBatches();
        CollectObjects();
        CreateBatches();
        DisableOriginalRenderers();
    }

    [ContextMenu("Clear Batching")]
    public void ClearBatching()
    {
        ClearBatches();
        EnableOriginalRenderers();
    }

    private void CollectObjects()
    {
        _originalRenderers.Clear();

        // Tìm tất cả renderers theo cài đặt
        Renderer[] renderers = GetComponentsInChildren<Renderer>(_includeInactiveChildren);

        foreach (Renderer renderer in renderers)
        {
            // Bỏ qua nếu renderer không active và không bao gồm inactive
            if (!renderer.gameObject.activeInHierarchy && !_includeInactiveChildren)
                continue;

            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                continue;

            if (renderer.sharedMaterial == null)
                continue;

            _originalRenderers.Add(renderer);
        }
    }

    private void CreateBatches()
    {
        foreach (Renderer renderer in _originalRenderers)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null)
                continue;

            Material material = renderer.sharedMaterial;
            if (material == null)
                continue;

            // Kiểm tra tên material: bỏ qua nếu tên có chứa "Liquid" (không phân biệt hoa thường)
            if (material.name.ToLower().Contains("liquid"))
                continue;

            // Kiểm tra material có enable GPU instancing không
            if (!material.enableInstancing)
                continue;

            Mesh mesh = meshFilter.sharedMesh;
            var key = new BatchKey(mesh, material);

            if (!_batches.ContainsKey(key))
                _batches[key] = new BatchData();

            _batches[key].matrices.Add(renderer.localToWorldMatrix);

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            _batches[key].propertyBlocks.Add(block);
        }

        Debug.Log($"Created {_batches.Count} batches from {_originalRenderers.Count} objects");
    }

    private void RenderBatches()
    {
        foreach (var batch in _batches)
        {
            Mesh mesh = batch.Key.Mesh;
            Material material = batch.Key.Material;
            BatchData batchData = batch.Value;

            int totalInstances = batchData.matrices.Count;

            for (int i = 0; i < totalInstances; i += _maxInstancesPerBatch)
            {
                int count = Mathf.Min(_maxInstancesPerBatch, totalInstances - i);

                // Lấy chunk matrices
                Matrix4x4[] matricesChunk = new Matrix4x4[count];
                for (int j = 0; j < count; j++)
                {
                    matricesChunk[j] = batchData.matrices[i + j];
                }

                // Lấy property block tương ứng (nếu có)
                MaterialPropertyBlock propertyBlock = null;
                if (batchData.propertyBlocks.Count > i)
                {
                    propertyBlock = batchData.propertyBlocks[i];
                }

                // Vẽ instanced
                Graphics.DrawMeshInstanced(
                    mesh: mesh,
                    submeshIndex: 0,
                    material: material,
                    matrices: matricesChunk,
                    count: count,
                    properties: propertyBlock,
                    castShadows: UnityEngine.Rendering.ShadowCastingMode.On,
                    receiveShadows: true,
                    layer: gameObject.layer,
                    camera: null,
                    lightProbeUsage: UnityEngine.Rendering.LightProbeUsage.BlendProbes
                );
            }
        }
    }

    private void DisableOriginalRenderers()
    {
        foreach (Renderer renderer in _originalRenderers)
        {
            renderer.enabled = false;
        }
    }

    private void EnableOriginalRenderers()
    {
        foreach (Renderer renderer in _originalRenderers)
        {
            if (renderer != null)
                renderer.enabled = true;
        }
    }

    private void ClearBatches()
    {
        _batches.Clear();
        _originalRenderers.Clear();
    }

    void OnDestroy()
    {
        EnableOriginalRenderers();
    }

    // Debug information
    void OnGUI()
    {
        if (!Application.isPlaying) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Instanced Rendering Debug");
        GUILayout.Label($"Batches: {_batches.Count}");
        GUILayout.Label($"Total Objects: {_originalRenderers.Count}");

        int totalInstances = 0;
        foreach (var batch in _batches)
        {
            totalInstances += batch.Value.matrices.Count;
        }
        GUILayout.Label($"Total Instances: {totalInstances}");

        GUILayout.EndArea();
    }
}