using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshCombine : MonoBehaviour
{
    public int maxVerticesPerMesh = 65534;

    const string Combined = "Combined";

    struct SubInfo
    {
        public int subMeshIndex;
        public Material material;
    }

    class MeshEntry
    {
        public Mesh mesh;
        public Transform transform;
        public int vertexCount;
        public List<SubInfo> subs = new List<SubInfo>();
    }
    public void CombineNow()
    {
        List<MeshEntry> entries = new List<MeshEntry>();
        GetChildsMesh(out entries);
        SplitMesh(entries);

        WallTile[] wallTiles = GetComponentsInChildren<WallTile>();
        for (int i = 0; i < wallTiles.Length; i++)
        {
            if (wallTiles[i].row == -1 || wallTiles[i].col == -1)
            {
                Destroy(wallTiles[i].gameObject);
            }
            else
            {
                wallTiles[i].gameObject.gameObject.SetActive(false);
            }
        }
    }
    void GetChildsMesh(out List<MeshEntry> entries)
    {
        var meshFilters = GetComponentsInChildren<MeshFilter>();
        entries = new List<MeshEntry>();

        foreach (var render in meshFilters)
        {
            if (render.sharedMesh == null) continue;
            if (render.gameObject == this.gameObject) continue;
            if (render.transform.name.Contains(Combined)) continue;

            var mr = render.GetComponent<MeshRenderer>();
            if (mr == null) continue;
            if (!mr.enabled || !render.gameObject.activeInHierarchy) continue;

            var mesh = render.sharedMesh;
            var mats = mr.sharedMaterials;
            int subCount = mesh.subMeshCount;

            if (mats == null || mats.Length < subCount)
            {
                var fixedMats = new Material[subCount];
                for (int i = 0; i < subCount; i++)
                    fixedMats[i] = (mats != null && i < mats.Length) ? mats[i] : null;
                mats = fixedMats;
            }

            var entry = new MeshEntry
            {
                mesh = mesh,
                transform = render.transform,
                vertexCount = mesh.vertexCount
            };

            for (int s = 0; s < mesh.subMeshCount; s++)
            {
                entry.subs.Add(new SubInfo
                {
                    subMeshIndex = s,
                    material = mats[s]
                });
            }

            entries.Add(entry);
        }

    }
    void SplitMesh(List<MeshEntry> entries)
    {
        var worldToLocal = transform.worldToLocalMatrix;
        var groups = new List<List<MeshEntry>>();
        var current = new List<MeshEntry>();
        int sum = 0;

        foreach (var e in entries)
        {
            if (current.Count > 0 && sum + e.vertexCount > maxVerticesPerMesh)
            {
                groups.Add(current);
                current = new List<MeshEntry>();
                sum = 0;
            }
            current.Add(e);
            sum += e.vertexCount;
        }
        if (current.Count > 0) groups.Add(current);

        var createdNames = new HashSet<string>();
        for (int i = 0; i < groups.Count; i++)
        {
            string name = "Combined_" + i;
            CreateCombined(name, groups[i], worldToLocal);
            createdNames.Add(name);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            var obj = transform.GetChild(i);
            if (createdNames.Contains(obj.name)) continue;
            if(obj.GetComponent<WallTile>() == null || !obj.gameObject.activeSelf ) Destroy(obj.gameObject);
            //obj.gameObject.SetActive(false);
        }
    }

    void CreateCombined(string groupName, List<MeshEntry> group, Matrix4x4 parentWorldToLocal)
    {
        var combineListByMaterial = new Dictionary<Material, List<CombineInstance>>();

        foreach (var entry in group)
        {
            var localMatrix = parentWorldToLocal * entry.transform.localToWorldMatrix;

            foreach (var sub in entry.subs)
            {
                var cb = new CombineInstance
                {
                    mesh = entry.mesh,
                    subMeshIndex = sub.subMeshIndex,
                    transform = localMatrix
                };

                if (!combineListByMaterial.TryGetValue(sub.material, out var list))
                {
                    list = new List<CombineInstance>();
                    combineListByMaterial[sub.material] = list;
                }
                list.Add(cb);
            }
        }

        var tempMeshes = new List<Mesh>();
        var materials = new List<Material>();
        foreach (var m in combineListByMaterial)
        {
            var temp = new Mesh();
            temp.CombineMeshes(m.Value.ToArray(), true, true, false);
            tempMeshes.Add(temp);
            materials.Add(m.Key);
        }

        var finalCombiners = new List<CombineInstance>();
        foreach (var tm in tempMeshes)
        {
            finalCombiners.Add(new CombineInstance
            {
                mesh = tm,
                subMeshIndex = 0,
                transform = Matrix4x4.identity
            });
        }

        var go = new GameObject(groupName);
        go.layer = LayerMask.NameToLayer("Tile");
        go.transform.SetParent(transform, worldPositionStays: true);
        go.transform.SetAsFirstSibling();
        go.transform.localPosition = Vector3.zero;
        go.transform.localEulerAngles = Vector3.zero;

        var mfOut = go.AddComponent<MeshFilter>();
        var mrOut = go.AddComponent<MeshRenderer>();

        var finalMesh = new Mesh();
        finalMesh.CombineMeshes(finalCombiners.ToArray(), false, true, false);

        mfOut.sharedMesh = finalMesh;
        mrOut.sharedMaterials = materials.ToArray();

        //go.AddComponent<BoxCollider>();

        foreach (var tm in tempMeshes)
        {
            Destroy(tm);
        }
    }

}