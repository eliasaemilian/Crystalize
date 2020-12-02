using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CrystalMesh : MonoBehaviour
{
    public float radius = 3f;
    public int length = 8;
    public int segments = 6;
    public int pointiness = 2;
    public List<Vector3> vertices;
    public List<int> indices;
    public GameObject DebugSphere;

    private List<Vector3> normals;
    private List<Vector2> uvs;

    private Mesh mesh;

    private void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Crystal";

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        InstantiateMesh();
        ApplyToMesh();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void InstantiateMesh()
    {
        // VERTICES
        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                Vector3 vertPos;
                if (i == length - 1) // top point for crystal 
                {
                  //  vertPos = transform.position;
                  //  vertPos.y += i + pointiness;

                    float t = j / (float)segments;
                    float angRad = t * Mathfs.TAU; // angle in radians   

                    // Get Basic Corpus
                    Vector2 dir = Mathfs.GetUnitVectorByAngle(angRad);
                    vertPos = dir * .5f; //0 = closed tip <- TIP FACTOR
                    vertPos = Quaternion.Euler(90, 0, 0) * vertPos; // rotate to point crystal upwards
                    vertPos.y += i + pointiness;
                }
                else
                {
                    float t = j / (float)segments;
                    float angRad = t * Mathfs.TAU; // angle in radians   

                    // Get Basic Corpus
                    Vector2 dir = Mathfs.GetUnitVectorByAngle(angRad);
                    vertPos = dir * radius;
                    vertPos = Quaternion.Euler(90, 0, 0) * vertPos; // rotate to point crystal upwards
                    vertPos.y += i;
                  //  if (i != length - 2) vertPos.y += i; //enlongate on y
                  //  if (i == length - 2 && (j == 1 || j == 2)) vertPos.y -= Random.Range(1, 2);

                }

                vertices.Add(vertPos);
                vertices.Add(vertPos);
                float v = vertPos.y / (length + pointiness);
                float u = j / (float)segments;
                uvs.Add(new Vector2(u, v));
                uvs.Add(new Vector2(u, v));

            }


        }


        indices = new List<int>();

        // if tip factor > 0, if tip = 0 then this is redundant
        int addon = (length - 1) * (segments * 2);
        //close tip
        for (int j = 0; j < (segments * 2) - 2; j += 2)
        {
            indices.Add(addon + j);
            indices.Add(addon + (segments * 2) - 2);
            indices.Add(addon + j + 2);

        }


        // INDICES
        for (int i = 0; i < length - 1; i++)
        {
            for (int j = 0; j < (segments * 2) - 2; j += 2)
            {
                int root = (i * ((segments * 2)) + j);
                int rootNext = (i * ((segments * 2))) + j + (segments * 2);

                // 1
                indices.Add(rootNext);
                indices.Add(root + 2);
                indices.Add(root);

                // 2
                indices.Add(rootNext);
                indices.Add(rootNext + 2);
                indices.Add(root + 2);

                //  Debug.Log($"root is {root} and root + 1 is {root + 1} and rootNext is {rootNext} and rootNext + 1 in {rootNext + 1}");
            }

            // close off crystal between first and last row in indices in this column

            //1
            indices.Add(((i + 1) * (segments * 2)) + (segments * 2) - 2);
            indices.Add((i * (segments * 2)) + (segments * 2));
            indices.Add((i * (segments * 2)));

            // 2
            indices.Add(((i + 1) * (segments * 2)) + (segments * 2) - 2);
            indices.Add((i * (segments * 2)));
            indices.Add((i * (segments * 2)) + (segments * 2) - 2);

        }
    }


    private void ShapeVariations()
    {

    }

    private void Normals()
    {

    }

    private void Uvs()
    {
        float u = 0f;
        float v = 0f;
    }

    private void ApplyToMesh()
    {
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        //  mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

}
