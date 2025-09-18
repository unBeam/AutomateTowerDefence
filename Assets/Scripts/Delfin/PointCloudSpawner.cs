using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class PointCloudSpawner : MonoBehaviour
{
    public Mesh treeMesh;
    public Material treeMaterial;
    public string csvFileName = "test_rot.csv";

    [Header("Transform Options")]
    public Transform parent;
    public bool useParentAsOffset = false;
    public float positionScale = 0.01f;
    public Vector3 rotationOffsetEuler;
    public bool flipX = false;
    public bool flipZ = false;

    private List<Matrix4x4> _matrices = new List<Matrix4x4>();

    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, csvFileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"CSV file not found at path: {path}");
            return;
        }

        string[] lines = File.ReadAllLines(path);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] cols = lines[i].Split(',');
            if (cols.Length < 8) // posX, posY, posZ, scale, rotX, rotY, rotZ, rotW
            {
                Debug.LogWarning($"Skipping invalid line {i}: {lines[i]}");
                continue;
            }

            float posX = float.Parse(cols[0], CultureInfo.InvariantCulture);
            float posY = float.Parse(cols[1], CultureInfo.InvariantCulture);
            float posZ = float.Parse(cols[2], CultureInfo.InvariantCulture);

            float scale = float.Parse(cols[3], CultureInfo.InvariantCulture);

            float rotX = float.Parse(cols[4], CultureInfo.InvariantCulture);
            float rotY = float.Parse(cols[5], CultureInfo.InvariantCulture);
            float rotZ = float.Parse(cols[6], CultureInfo.InvariantCulture);
            float rotW = float.Parse(cols[7], CultureInfo.InvariantCulture);

            // Позиция
            Vector3 pos = new Vector3(-posX, posY, posZ) * positionScale;


            if (flipX) pos.x = -pos.x;
            if (flipZ) pos.z = -pos.z;

            if (useParentAsOffset && parent != null)
                pos = parent.TransformPoint(pos);

            // Кватернион из Houdini → Unity
            Quaternion rot = new Quaternion(-rotX, rotY, rotZ, -rotW);
            rot.Normalize();

            // Доп. смещение (ручной оффсет)
            // Масштаб
            Vector3 scl = new Vector3(scale, scale, scale);

            Matrix4x4 matrix = Matrix4x4.TRS(pos, rot, scl);
            _matrices.Add(matrix);

            Debug.Log($"[PointCloudSpawner] Instance {i}: pos={pos}, quat=({rotX},{rotY},{rotZ},{rotW}), unityRot={rot.eulerAngles}, scale={scl}");
        }

        Debug.Log($"[PointCloudSpawner] Loaded {_matrices.Count} instances from {csvFileName}");
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
