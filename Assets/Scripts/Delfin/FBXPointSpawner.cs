using System.Collections.Generic;
using UnityEngine;

public class FBXPointSpawner : MonoBehaviour
{
    public Mesh treeMesh;
    public Material treeMaterial;
    public GameObject pointCloudFBX; // сюда кидаешь импортированный FBX с точками

    private List<Matrix4x4> _matrices = new List<Matrix4x4>();

    void Start()
    {
        if (pointCloudFBX == null)
        {
            Debug.LogError("No FBX assigned!");
            return;
        }

        foreach (Transform child in pointCloudFBX.transform)
        {
            Vector3 pos = child.position;
            Quaternion rot = child.rotation;
            Vector3 scl = child.localScale;

            Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scl);
            _matrices.Add(matrix);
        }

        Debug.Log($"Loaded {_matrices.Count} instances from FBX {pointCloudFBX.name}");
    }

    void Update()
    {
        if (_matrices.Count == 0 || treeMesh == null || treeMaterial == null)
            return;

        for (int i = 0; i < _matrices.Count; i += 1023)
        {
            int count = Mathf.Min(1023, _matrices.Count - i);
            Graphics.DrawMeshInstanced(treeMesh, 0, treeMaterial, _matrices.GetRange(i, count));
        }
    }
}