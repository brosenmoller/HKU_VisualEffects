using UnityEngine;
using System.Collections;

// Followed tutorial by brackeys https://www.youtube.com/watch?v=eJEpeUH1EMg

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    [Header("General Settings")]
    public bool autoUpdate;
    [SerializeField] [Range(1, 100)] int xSize;
    [SerializeField] [Range(1, 100)] int zSize;
    [SerializeField] [Range(1, 200)] int xVertices;
    [SerializeField] [Range(1, 200)] int zVertices;

    [Header("Perlin Noise")]
    [SerializeField] [Range(0, 30)] float maxHeigth;
    [SerializeField] [Range(.1f, 20)] float scale;

    [Header("Offset Moving")]
    [SerializeField] [Range(-.1f, .1f)] float scrollSpeed;

    float offsetX, offsetZ;

    MeshFilter meshFilter;

    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    [HideInInspector] public bool isScrolling;

    public void DrawMesh()
    {
        //// PerlinNoise offset for randomnes
        //offsetX = Random.Range(0f, 9999f);
        //offsetZ = Random.Range(0f, 9999f);

        CreateShape();
        CreateUV();
        UpdateMesh();
    }

    public void Scroll()
    {
        isScrolling = !isScrolling;
        if (isScrolling)
        {
            StartCoroutine(Scrolling());
        }
        else
        {
            StopAllCoroutines();
        }
    }

    IEnumerator Scrolling()
    {
        while (true)
        {
            offsetX += scrollSpeed;
            offsetZ += scrollSpeed;

            CreateShape();
            CreateUV();
            UpdateMesh();

            yield return null;
        }
    }

    private void Update()
    {
        if (isScrolling)
        {
            Scroll();
        }
    }

    void CreateShape()
    {
        vertices = new Vector3[(xVertices + 1) * (zVertices + 1)];

        for (int i = 0, z = 0; z <= zVertices; z++)
        {
            for (int x = 0; x <= xVertices; x++)
            {
                float worldZ = (zSize / zVertices) * z;
                float worldX = (xSize / xVertices) * x;

                float y = CalculatePerlinNoise(worldX, worldZ) * maxHeigth;
                vertices[i] = new Vector3(worldX, y, worldZ);
                i++;
            }
        }

        triangles = new int[xVertices * zVertices * 6];


        int vert = 0;
        int tris = 0;

        for (int z = 0; z < zVertices; z++)
        {
            for (int x = 0; x < xVertices; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + xVertices + 1;
                triangles[tris + 2] = vert + 1;

                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + xVertices + 1;
                triangles[tris + 5] = vert + xVertices + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }
    }

    void CreateUV()
    {
        // Generate UV map for overlaying images
        uvs = new Vector2[vertices.Length];

        for (int i = 0, z = 0; z <= zVertices; z++)
        {
            for (int x = 0; x <= xVertices; x++)
            {
                uvs[i] = new Vector2((float)x / xVertices, (float)z / zVertices);

                i++;
            }
        }
    }

    void UpdateMesh()
    {
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles
        };

        if (uvs != null) mesh.uv = uvs;

        mesh.RecalculateNormals();
        
        meshFilter.mesh = mesh;
    }

    float CalculatePerlinNoise(float x, float z)
    {
        // Calculate PerlinNoise coordinates based on scale and offset
        float xCoord = x / xSize * scale + offsetX;
        float zCoord = z / zSize * scale + offsetZ;

        return Mathf.PerlinNoise(xCoord, zCoord);
    }

    //private void OnDrawGizmos()
    //{
    //    if (vertices == null) return;

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], .1f);
    //    }
    //}
}
