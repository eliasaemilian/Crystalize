using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CrystalCluster : MonoBehaviour
{
    public List<Vector3> vertices;
    public List<int> indices;
    public float[] radii;
    private Vector3[] randomAngles;
    public GameObject DebugSphere;

    private List<Vector3> normals;
    private List<Vector2> uvs;


    private Vector3[] clusterPositions;

    public CrystalConfig Config;
    private CrystalConfig runningConfig;
    private CrystalConfig addConfig;

    private int count;

    private List<GameObject> crystals;

    private void Awake()
    {
      //  GenerateConfig();
      //  crystals = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
      //  GenerateCluster();
    }

    public void OnInstantiate()
    {
        GenerateConfig();
        crystals = new List<GameObject>();
        GenerateCluster();
        CombineMeshes();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void GenerateConfig()
    {
        runningConfig = Config;
        addConfig = Config.AdditionalConfig();

    }
    private void GenerateCluster()
    {
        if (crystals.Count > 0) for (int i = 0; i < crystals.Count; i++) Destroy(crystals[i]);


        int debugCount = 0;
        int state = 0, pass = 0;
        count = 0;
        radii = new float[addConfig.Amount];
        clusterPositions = new Vector3[addConfig.Amount];
        randomAngles = new Vector3[addConfig.Amount];

        for (; pass < 2; pass++)
        {
            if (pass > 0)
            {
                count = runningConfig.Amount;
                runningConfig = addConfig;
               // Debug.Log("Switching Config on new pass");
            }

            for (int i = count; i < runningConfig.Amount;)
            {
                switch (state)
                {
                    case 0:
                        {
                            if (debugCount > 500)
                            {
                               // Debug.Log("Hard Break at Position");
                                return;
                            }
                            debugCount++;

                            // Get position and radius
                            if (GetPositionAndRadius(out Vector3 pos, out float radius, i))
                            {
                                clusterPositions[i] = pos;
                                radii[i] = radius;
                                state++;
                            }
                            else
                            {
                                // Debug.Log("No further valid Positions avaliable, rebuilding new Formation");
                                GenerateCluster();
                                return;
                            }
                            break;
                        }
                    case 1:
                        {
                            if (debugCount > 500)
                            {
                                // Debug.Log("Hard Break at Rotation, rebuilding new Formation");
                                GenerateCluster();
                                return;
                            }
                            debugCount++;

                            // Test for valid rotation, if true apply, else find new position
                            if (GetRotation(out Vector3 angle, i))
                            {
                                randomAngles[i] = angle;
                                state++;
                            }
                            else state = 0;
                            break;
                        }
                    case 2:
                        {
                            state = 0;
                            i++;
                            break;
                        }
                    default:
                        state = 0;
                        break;
                }

            }

            debugCount = 0;


            // Instantiate Crystals with Parameters
            for (int i = count; i < runningConfig.Amount; i++)
            {

                GameObject crystal = InstantiateMesh(out int indicesCount, radii[i]);
                if (indicesCount == 0)
                {
                    Destroy(crystal);
                    // Debug.Log("Destroyed invalid crystal");

                }
                else
                {
                    crystal.name = i + " Crystal";
                    crystal.transform.position = clusterPositions[i];
                    crystal.transform.Rotate(randomAngles[i], Space.Self);
                    crystals.Add(crystal);
                    //  Debug.Log($"Instantiate Crystal {i} with r {radii[i]}, pos {clusterPositions[i]}, rot {randomAngles[i]}");
                }

            }
        }


        // merge meshes together



    }

    private bool GetPositionAndRadius(out Vector3 position, out float radius, int i)
    {
        float offsetMin = (Config.ClusterRadius / 100f) * runningConfig.MinOffset;
        float offsetMax = (Config.ClusterRadius / 100f) * runningConfig.MaxOffset;


        int debug = 0;
        float r = 0, x = 0, z = 0;
        Vector3 rPos = Vector3.zero;

        // ---------------- Find Crystal Positions
        while (r == 0 && debug < 800)
        {
            if (debug > 600)
            {
                position = Vector3.zero;
                radius = 0f;
              //  Debug.Log("Exited with no position found");
                return false;
            }

            debug++;
            rPos = Vector3.zero;
            float newR = 0, offset;
            int debug2 = 0, quadrantSearch = 0;

            while (runningConfig.MinRadius > newR || newR > runningConfig.MaxRadius && debug2 < 800)
            {
                debug2++;

                if (debug2 > 600)
                {
                    position = Vector3.zero;
                    radius = 0f;
                   // Debug.Log("Exited with no position found, inner Loop");
                    return false;
                }
                x = transform.position.x + Random.Range(-Config.ClusterRadius, (float)Config.ClusterRadius);
                z = transform.position.z + Random.Range(-Config.ClusterRadius, (float)Config.ClusterRadius);
                // Generate Random Pos on first crystal
                //if (i == 0)
                //{
                //    x = transform.position.x + Random.Range(-Config.ClusterRadius, (float)Config.ClusterRadius);
                //    z = transform.position.z + Random.Range(-Config.ClusterRadius, (float)Config.ClusterRadius);
                //}
                //else //search in different quadrant from previous 
                //{
                //    if (quadrantSearch % 2 == 0)
                //    {
                //        if (clusterPositions[i - 1].x > 0) x = Random.Range((float)-Config.ClusterRadius, 0);
                //        else x = Random.Range((float)Config.ClusterRadius, 0);
                //    }
                //    else
                //    {
                //        if (clusterPositions[i - 1].z > 0) x = Random.Range((float)-Config.ClusterRadius, 0);
                //        else z = Random.Range((float)Config.ClusterRadius, 0);
                //    }
                //    quadrantSearch++;
                //}

                rPos = new Vector3(x, transform.position.y, z);

                offset = Random.Range(offsetMin, offsetMax);
                newR = Vector3.Distance(transform.position, rPos) - offset;
            }

            // new Random

            if (radii[count] != 0)
            {
                for (int j = 0; j < clusterPositions.Length; j++)
                {
                    if (clusterPositions[j] == Vector3.zero || i == j) continue; //ignore if j not set yet

                    // Debug.Log($"R found was {newR} and distance is {Vector3.Distance(clusterPositions[j], rPos)} < Radii {radii[j] + newR}");

                    if (Vector3.Distance(clusterPositions[j], rPos) < radii[j] + newR)
                    {
                        //Debug.Log("Crystals too close");
                        r = 0;
                        break;
                    }
                    else
                    {
                        r = newR;
                        // Debug.Log($"Found new r {r}");
                    }


                }
            }
            else r = newR;

        }

        //  Debug.Log($"new R found was {r} for {i} and debug is {debug}");

        radius = r;
        position = rPos;
        return true;

    }

    private bool GetRotation(out Vector3 angle, int i)
    {
        int debug = 0, incrementCount = 0;
        float newX = 0f, newZ = 0f;
        Vector3 searchAngle = Vector3.zero;

        while (searchAngle == Vector3.zero)
        {
            if (debug > 360)
            {
              //  Debug.Log("Found no valid rotation for this crystal");
                angle = Vector3.zero;
                return false;

            }
            else debug++;

            if (newX == 0f && newZ == 0f)
            {
                // Generate Random Angle
                newX = Random.Range(-Config.MaxRotationAngle, Config.MaxRotationAngle);
                newZ = Random.Range(-Config.MaxRotationAngle, Config.MaxRotationAngle);
            }
            else
            {
                // Increment
                if (newX + 20 > Config.MaxRotationAngle) newX -= 20;
                else newX += 20;
                if (newZ + 20 > Config.MaxRotationAngle) newZ -= 20;
                else newZ += 20;

                if (incrementCount < 32) incrementCount++;
                else
                {
                 //   Debug.Log("Leaving Rotaion for no angle found");
                    angle = Vector3.zero;
                    return false;
                }
            }

            searchAngle = new Vector3(newX, 0, newZ);
            Vector3 testRot = new Vector3(newX, 0, newZ);

            // for each crystal in cluster check against i
            for (int j = 0; j < randomAngles.Length; j++)
            {
                if (randomAngles[j] == Vector3.zero) continue; //ignore if j not set yet

                Vector3 direction = Quaternion.Euler(testRot) * Vector3.up;
                Vector3 dirOther = Quaternion.Euler(randomAngles[j]) * Vector3.up;

                //Debug.DrawRay(clusterPositions[i], direction, Color.red, 50f, false);
                //Debug.DrawRay(clusterPositions[j], dirOther, Color.blue, 50f, false);

                if (Mathfs.ClosestPointsOnTwoLines(out Vector3 point1, out Vector3 point2, clusterPositions[i], direction, clusterPositions[j], dirOther))
                {
                    if (Vector3.Distance(point1, point2) < radii[i] + radii[j] && Vector3.Distance(clusterPositions[i], point1) < runningConfig.MaxLength)
                    {
                        // Debug.Log("Hit other Crystal");
                        searchAngle = Vector3.zero;
                        break;
                    }
                    else
                    {
                      //  Debug.Log($"Valid: Tested for Collision true with Distance {Vector3.Distance(point1, point2)} and radii {radii[i]} and {radii[j]}");
                    }
                }

            }

        }

        angle = searchAngle;
        return true;
    }


    private GameObject InstantiateMesh(out int indicesCount, float radius)
    {
        GameObject crystal = new GameObject();
        crystal.transform.SetParent(transform);
        crystal.AddComponent(typeof(MeshRenderer));
        crystal.AddComponent(typeof(MeshFilter));
        Mesh mesh = new Mesh { name = "CrystalMesh" };

        int length = Random.Range(runningConfig.MinLength, runningConfig.MaxLength + 1);       
        int segments = Random.Range(runningConfig.MinSegments, runningConfig.MaxSegments);
        float pointiness = Random.Range(runningConfig.MinPointiness, runningConfig.MaxPointiness);

        // VERTICES
        vertices = new List<Vector3>();
        normals = new List<Vector3>();
        uvs = new List<Vector2>();
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < segments; j++)
            {
                Vector3 vertPos;
                if (i == length - 1) // top point for crystal (double row)
                {
                    vertPos = transform.position;
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
                    vertPos.y += i; //enlongate on y

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



        // Set Mesh
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        //  mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        crystal.GetComponent<MeshFilter>().mesh = mesh;
        crystal.GetComponent<MeshRenderer>().material = new Material(GetComponent<MeshRenderer>().material);

        indicesCount = indices.Count;
        return crystal;
    }

    private void CombineMeshes()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[meshFilters.Length - 1];

        int i = 1; // 0 is parent mesh
        while (i < meshFilters.Length)
        {
            
            combine[i -1].mesh = meshFilters[i].sharedMesh;
            combine[i - 1].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            i++;
        }
        transform.GetComponent<MeshFilter>().mesh = new Mesh();
        transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine, true);

        foreach (Transform child in transform) Destroy(child.gameObject);
    }


}
