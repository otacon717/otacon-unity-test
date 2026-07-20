using UnityEngine;

/// <summary>
/// Procedurally generated finite curved surface with varying heights,
/// built from a layered Perlin-noise height field.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class CurvedSurface : MonoBehaviour
{
    [Header("Dimensions")]
    [SerializeField] private float width = 24f;
    [SerializeField] private float length = 24f;
    [SerializeField, Range(8, 128)] private int resolution = 64;

    [Header("Height Field")]
    [SerializeField] private float heightScale = 2.6f;
    [SerializeField] private float noiseScale = 0.085f;
    [SerializeField] private int seed = 20260720;

    private void Awake()
    {
        Rebuild();
    }

    public void Rebuild()
    {
        Mesh mesh = BuildMesh();
        GetComponent<MeshFilter>().sharedMesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public float SampleHeight(float worldX, float worldZ)
    {
        // Deterministic offsets derived from the seed so the collider,
        // visuals and any queries always agree.
        Random.State prev = Random.state;
        Random.InitState(seed);
        float ox = Random.Range(-1000f, 1000f);
        float oz = Random.Range(-1000f, 1000f);
        Random.state = prev;

        float nx = (worldX + ox) * noiseScale;
        float nz = (worldZ + oz) * noiseScale;

        float h = Mathf.PerlinNoise(nx, nz);                                  // base rolling hills
        h += 0.38f * Mathf.PerlinNoise(nx * 2.7f + 13.1f, nz * 2.7f + 7.9f); // finer detail
        h -= 0.69f;                                                           // roughly centre around zero
        return h * heightScale;
    }

    private Mesh BuildMesh()
    {
        int vertsPerSide = resolution + 1;
        var vertices = new Vector3[vertsPerSide * vertsPerSide];
        var uvs = new Vector2[vertices.Length];
        var triangles = new int[resolution * resolution * 6];

        for (int z = 0; z < vertsPerSide; z++)
        {
            for (int x = 0; x < vertsPerSide; x++)
            {
                float px = (x / (float)resolution - 0.5f) * width;
                float pz = (z / (float)resolution - 0.5f) * length;
                int i = z * vertsPerSide + x;
                vertices[i] = new Vector3(px, SampleHeight(px, pz), pz);
                uvs[i] = new Vector2(x / (float)resolution, z / (float)resolution);
            }
        }

        int t = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * vertsPerSide + x;
                triangles[t++] = i;
                triangles[t++] = i + vertsPerSide;
                triangles[t++] = i + 1;
                triangles[t++] = i + 1;
                triangles[t++] = i + vertsPerSide;
                triangles[t++] = i + vertsPerSide + 1;
            }
        }

        var mesh = new Mesh
        {
            name = "CurvedSurface",
            vertices = vertices,
            uv = uvs,
            triangles = triangles
        };
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
