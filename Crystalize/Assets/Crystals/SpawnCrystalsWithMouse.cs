using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class SpawnCrystalsWithMouse : MonoBehaviour
{
    private class bridge
    {
        public Vector3 start;
        public Vector3 end;
        public Vector3[] hashedPos;
        public Vector3[] bezierPos;
    }

    // Draw Crystals
    public GameObject crystalTest;
    public float AreaRadius = 3f;
    public float ClusterRadius = 3f;
    private Camera cam;

    public Material CrystalBridgeMat;

    // Bridge Mode
    //public int bridgeWidth = 10;


    float startTick = .05f, tick, count = 0f, mouseDist;
    int savedPosNum;
    bool drawing; // true when a line is drawn
    Vector3 newPos;
    [SerializeField] List<Vector3> mousePositions;

    ObjectPooler pooler;

    public bool bridgeMode;

    public CrystalConfig defaultCrystalConfig;

    // Start is called before the first frame update
    void Start()
    {
        tick = startTick;
        cam = Camera.main;
        pooler = ObjectPooler.Instance;
        mousePositions = new List<Vector3>();
        // GetSpawnArea();
    }

    // Update is called once per frame
    void Update()
    {

        // **** TEST CRYSTAL HALFMOON *****
        if (Input.GetButtonUp("Jump"))
        {
            CrystalHalfmoon(transform.position, transform.forward, 60);
        }

   //     Debug.DrawLine(transform.position, transform.forward * 50f, Color.red);

        // ****** END ******

        if (Input.GetMouseButtonDown(0))
        {

            drawing = true;
            if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition)))
            {
                mousePositions.Clear();
                if (!bridgeMode) GetSpawnArea();
                else mousePositions.Add(GetWorldPosFromMouse()); //add start
            }

        }

        if (Input.GetMouseButton(0))
        {
            drawing = true;
            //Debug.Log("Registering Mouse");
            // if new position registered then, new elongated area
            if (mousePositions.Count > savedPosNum && mousePositions.Count > 2)
            {
                //  Debug.Log("Elongating");
                if (!bridgeMode) GetElongatedArea();
                savedPosNum = mousePositions.Count;

            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            drawing = false;

            if (!bridgeMode)
            {
                GetEndArea();
            }
            else
            {
                mousePositions.Add(GetWorldPosFromMouse()); // add end

                bridge newBridge = new bridge();
                newBridge.start = mousePositions[0];

                // Check here if end Position is a valid Position
                newBridge.end = mousePositions[mousePositions.Count - 1];
                newBridge.hashedPos = mousePositions.ToArray();
                ConstructBezier(newBridge);
            }

            savedPosNum = 0;
            mousePositions.Clear();
            //   Debug.Log("Ending Line");
        }


    }

    private void FixedUpdate()
    {

        // Collect Path Points from mouse
        count += Time.deltaTime;
        if (count > tick && drawing)
        {
            count = 0;
            //float dist = ClusterRadius;
            newPos = GetWorldPosFromMouse();
            //if (!bridgeMode) newPos = GetWorldPosFromMouse();
            //else newPos = GetWorldPosFromMouse();

            if (newPos != Vector3.zero)
            {
                mouseDist = 1f; //needs to be bigger than check

                if (mousePositions.Count > 2)
                {
                    mouseDist = Vector3.Distance(newPos, mousePositions[mousePositions.Count - 1]);

                    if (mouseDist > (ClusterRadius * .5)) tick /= (mouseDist / ClusterRadius);
                    else tick = startTick;
                }

                if (mouseDist > .5f)
                {
                    if (!bridgeMode) mousePositions.Add(GetWorldPosFromMouse());
                    else mousePositions.Add(GetWorldPosFreeFromMouse());

                    //   mousePositions.Add(newPos);
                }

            }

        }

    }

    Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = t * 2 - 1;
        if (Mathf.Abs(start.y - end.y) < 0.1f)
        {
            //start and end are roughly level, pretend they are - simpler solution with less steps
            Vector3 travelDirection = end - start;
            Vector3 result = start + t * travelDirection;
            result.y += (-parabolicT * parabolicT + 1) * height;
            return result;
        }
        else
        {
            //start and end are not level, gets more complicated
            Vector3 travelDirection = end - start;
            Vector3 levelDirecteion = end - new Vector3(start.x, end.y, start.z);
            Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
            Vector3 up = Vector3.Cross(right, travelDirection);
            if (end.y > start.y) up = -up;
            Vector3 result = start + t * travelDirection;
            result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
            return result;
        }
    }



    public void CrystalCircle(Vector3 center, int radius) // also give config here? mayhaps
    {
        // Circle radius
        // get middle point, generate Circle of Clusters for circle radius
        int rows = (int)(radius / ClusterRadius);
        for (int i = 0; i < rows; i++)
        {
            float r = ClusterRadius * i;
            float U = Mathfs.TAU * r;
            int columns = (int)(U / ClusterRadius);
            for (int j = 0; j < (columns); j++)
            {
                SpawnCrystalFromPool(j, columns, r, center);
            }

        }
    }

    public void CrystalHalfmoon(Vector3 center, Vector3 direction, int radius)
    {
        // get width of Halfmoon
        // get radians for end, start point
        // spawn crystals (with offset?)
        int rows = (int)(radius / ClusterRadius);
        float r = ClusterRadius * rows;
        float U = Mathfs.TAU * r;
        int columns = (int)(U / ClusterRadius);

        
        for (int j = 0; j < columns * 0.33f; j++)
        {
           // int j2 = j * ;
            SpawnCrystalFromPool(j, columns, r, center, direction);
        }

    }

    private void ConstructBezier(bridge bridge)
    {
        bool drawnBackwards = false;
        // Find Angle And Intersection to approximate Bezier Height
        float angle = 45f;
        float angleS = angle, angleE = (360 - angle);
        Vector3 dirStart, dirEnd;

        if ((Mathf.Abs(bridge.start.z - bridge.end.z)) > ((Mathf.Abs(bridge.start.x - bridge.end.x))))
        {
            if (bridge.start.z < bridge.end.z) // Reverse angle if going in reverse axis direction
            {
                angleS = (360 - angle);
                angleE = angle;
                drawnBackwards = true;
            }

            dirStart = Quaternion.Euler(angleS, 0, 0) * (bridge.end - bridge.start);
            dirEnd = Quaternion.Euler(angleE, 0, 0) * (bridge.start - bridge.end);
            //   Debug.Log("Rotate in X");
        }
        else
        {
            if (bridge.start.x > bridge.end.x) // Reverse angle if going in reverse axis direction
            {
                angleS = (360 - angle);
                angleE = angle;
                drawnBackwards = true;
            }
            //  Debug.Log("Rotate in Z");
            dirStart = Quaternion.Euler(0, 0, angleS) * (bridge.end - bridge.start);
            dirEnd = Quaternion.Euler(0, 0, angleE) * (bridge.start - bridge.end);
        }

        float t, totalDist = Vector3.Distance(bridge.start, bridge.end);
        int bezierPoints = (int)(totalDist / ClusterRadius);
        bridge.bezierPos = new Vector3[bezierPoints];
        if (Mathfs.ClosestPointsOnTwoLines(out Vector3 i1, out Vector3 i2, bridge.start, dirStart, bridge.end, dirEnd))
        {
            // Calculate Bezier using the new P1
            for (int i = 0; i < bridge.bezierPos.Length; i++)
            {
                // If valid mouse positions you can use this but if not use below
                // t = Vector3.Distance(bridge.hashedPos[i], bridge.end) / totalDist; // get percentual how far on line between start and end                                                                          
                t = i / (float)bridge.bezierPos.Length;

                bridge.bezierPos[i] = Mathfs.GetBezierPointWithOneCP(bridge.start, i1, bridge.end, t);
                //DebugSpawn(bridge.bezierPos[i], Color.white);
            }

        }

        // ELSE THIS WILL BE NO BEZIER


        // ----------------------------------------------------------------------
        // Construct Bezier Mesh


        // USING SURFACE NORMAL
        Vector3 elevStart = new Vector3(bridge.start.x, bridge.start.y + 5f, bridge.start.z);
        //if (Physics.Raycast(elevStart, Vector3.down, out RaycastHit hit, 30f))
        //{
        //    Debug.Log("Hit " + hit.collider.gameObject.name);
        //    Vector3 di = Vector3.ProjectOnPlane(bridge.start, hit.normal);
        //    Debug.DrawRay(bridge.start, di.normalized * 60f, Color.magenta, 20f);
        //  //  Vector3 po = bridge.start + (di.normalized * 6f);
        //  //  DebugSpawn(po, Color.magenta);
        //}
        //Vector3 d = Vector3.ProjectOnPlane(bridge.start, (bridge.end - bridge.start));
        //Debug.DrawRay(bridge.start, d, Color.blue, 20f);
        //  Vector3 pos = bridge.start + (d.normalized * 6f);
        // DebugSpawn(pos, Color.green);

        // Vertices Positions
        float bridgeWidth = (int)(totalDist / 8);
        float bridgeHeight = bridgeWidth / 4;
        List<Vector3> vertices = new List<Vector3>();
        Vector3 dir = Vector3.ProjectOnPlane(bridge.start, (bridge.end - bridge.start).normalized).normalized;

        for (int i = 0; i < bridge.bezierPos.Length; i++)
        {

            Vector3 p = bridge.bezierPos[i] + (dir * (bridgeWidth / 2));
            Vector3 pY = new Vector3(p.x, p.y - bridgeHeight, p.z);
            Vector3 p2 = bridge.bezierPos[i] + (dir * (-bridgeWidth / 2));
            Vector3 p2Y = new Vector3(p2.x, p2.y - bridgeHeight, p2.z);


            // Spawn Crystal in P
            GameObject c1 = pooler.SpawnFromPool("BridgeCrystal", p, Quaternion.identity);
            GameObject c2 = pooler.SpawnFromPool("BridgeCrystal", pY, Quaternion.identity);
            //c2.transform.Rotate(0, 0, 180);
            GameObject c3 = pooler.SpawnFromPool("BridgeCrystal", p2, Quaternion.identity);
            GameObject c4 = pooler.SpawnFromPool("BridgeCrystal", p2Y, Quaternion.identity);

            c1.GetComponent<ClusterHandler>().ignoreLifetime = true; // Move to Config and have config for bridge Crystals
            c2.GetComponent<ClusterHandler>().ignoreLifetime = true;
            c3.GetComponent<ClusterHandler>().ignoreLifetime = true;
            c4.GetComponent<ClusterHandler>().ignoreLifetime = true;


            p = transform.InverseTransformPoint(p);
            pY = transform.InverseTransformPoint(pY);
            p2 = transform.InverseTransformPoint(p2);
            p2Y = transform.InverseTransformPoint(p2Y);


            vertices.Add(p);
            vertices.Add(p2);
            vertices.Add(pY);
            vertices.Add(p2Y);

        }

        // INDICE
        List<int> indices = new List<int>();
        for (int i = 0; i < vertices.Count - 4; i += 4)
        {
            if (drawnBackwards)
            {
                // Top Face
                //i + 1 p2, i + 1 p, p2
                indices.Add(i + 1);
                indices.Add(i + 4);
                indices.Add(i + 4 + 1);

                // i+1 p, p, p2
                indices.Add(i + 1);
                indices.Add(i);
                indices.Add(i + 4);


                // Left Face
                // i+1 p2, p2, i+1 p2y
                indices.Add(i + 4 + 3);
                indices.Add(i + 1);
                indices.Add(i + 4 + 1);
                // p2, p2y, i+1 p2y
                indices.Add(i + 4 + 3);
                indices.Add(i + 3);
                indices.Add(i + 1);


                // Right Face
                // p, i+1 p, py
                indices.Add(i + 2);
                indices.Add(i + 4);
                indices.Add(i);
                // i+1 p, i+1 py, py
                indices.Add(i + 2);
                indices.Add(i + 4 + 2);
                indices.Add(i + 4);

                // Bottom Face
                // p2y, py, i+1 p2y
                indices.Add(i + 4 + 3);
                indices.Add(i + 2);
                indices.Add(i + 3);
                // py, i+1 py, p2y i+1
                indices.Add(i + 4 + 3);
                indices.Add(i + 4 + 2);
                indices.Add(i + 2);
            }
            else
            {
                // Top Face
                //i + 1 p2, i + 1 p, p2
                indices.Add(i + 4 + 1);
                indices.Add(i + 4);
                indices.Add(i + 1);
                // i+1 p, p, p2
                indices.Add(i + 4);
                indices.Add(i);
                indices.Add(i + 1);


                // Left Face
                // i+1 p2, p2, i+1 p2y
                indices.Add(i + 4 + 1);
                indices.Add(i + 1);
                indices.Add(i + 4 + 3);
                // p2, p2y, i+1 p2y
                indices.Add(i + 1);
                indices.Add(i + 3);
                indices.Add(i + 4 + 3);


                // Right Face
                // p, i+1 p, py
                indices.Add(i);
                indices.Add(i + 4);
                indices.Add(i + 2);
                // i+1 p, i+1 py, py
                indices.Add(i + 4);
                indices.Add(i + 4 + 2);
                indices.Add(i + 2);

                // Bottom Face
                // p2y, py, i+1 p2y
                indices.Add(i + 3);
                indices.Add(i + 2);
                indices.Add(i + 4 + 3);
                // py, i+1 py, p2y i+1
                indices.Add(i + 2);
                indices.Add(i + 4 + 2);
                indices.Add(i + 4 + 3);
            }

        }

        // Close off Front and Back
        indices.Add(1);
        indices.Add(2);
        indices.Add(3);

        indices.Add(1);
        indices.Add(0);
        indices.Add(2);

        // Back
        indices.Add(vertices.Count - 1);
        indices.Add(vertices.Count - 2);
        indices.Add(vertices.Count - 3);


        indices.Add(vertices.Count - 2);
        indices.Add(vertices.Count - 4);
        indices.Add(vertices.Count - 3);



        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.name = "BridgeMesh";


        GameObject br = new GameObject();
        br.transform.position = transform.TransformPoint(transform.position);
        br.AddComponent<MeshFilter>().mesh = mesh;
        br.AddComponent<MeshRenderer>().material = new Material(CrystalBridgeMat);
        br.AddComponent<MeshCollider>();

        BridgeHandler brH = br.AddComponent<BridgeHandler>();

        brH.OnSpawn();
    }

    private void DebugSpawn(Vector3 pos, Color color)
    {
        GameObject go = Instantiate(crystalTest);
        go.transform.position = transform.position;
        go.GetComponent<MeshRenderer>().material.color = color;
        go.transform.position = pos;
    }

    private void ConstructBezierÔLD(bridge bridge)
    {

        // Calculate Bezier Control Point P1 using the MidPoint
        // Any Point projected on line for Bezier P = (1 - t)P0 + (t)P1
        //Vector3 p = ((1 - .5f) * bridge.start) + (.5f * bridge.end);
        //// Therefore Point on Bezier P1 = 2P(t)− tP0 − tP2
        //Vector3 p1 = (2 * p) - (.5f * bridge.start) - (.5f * bridge.end);

        ////TEST FOR TWO POINTS
        //Vector3 p2 = (2 * p) - (.33f * bridge.start) - (.33f * bridge.end);
        //Vector3 p3 = (2 * p) - (.66f * bridge.start) - (.66f * bridge.end);
        //GameObject cr2 = Instantiate(crystalTest);
        //cr2.GetComponent<MeshRenderer>().material.color = Color.red;
        //cr2.transform.position = p1;

        //DebugSpawn(p2, Color.red);
        //DebugSpawn(p3, Color.red);

        // Get Mid Point of Bezier
        // Mid Point as bezier defined as MousePos with the max perpendicular Vector length to the direct line between start and end
        //Vector3 maxPoint = Vector3.zero;
        //float maxDist = 0f;
        //for (int i = 0; i < bridge.hashedPos.Length; i++)
        //{
        //    Vector3 point = Mathfs.NearestPointOnFiniteLine(out float perpDist, bridge.start, bridge.end, bridge.hashedPos[i]);

        //    if (perpDist > maxDist)
        //    {
        //        maxDist = perpDist;
        //        maxPoint = point; // <- MIDPOINT OF BEZIER
        //    }

        //}

        //GameObject cr1 = Instantiate(crystalTest);
        //cr1.GetComponent<MeshRenderer>().material.color = Color.green;
        //cr1.transform.position = new Vector3(maxPoint.x, maxPoint.y + maxDist, maxPoint.z);

        // Run after Mouse Up
        // StartPos, EndPos

        // get Mouse Pos Average
        Vector3 average = Vector3.zero;
        //  Vector3 average2 = Vector3.zero;
        //for (int i = 0; i < bridge.hashedPos.Length; i++)
        //{
        //    if (i < bridge.hashedPos.Length * .5)
        //    average += bridge.hashedPos[i];
        //    else average2 += bridge.hashedPos[i];
        //}
        //average /= bridge.hashedPos.Length * .5f;
        //average2 /= bridge.hashedPos.Length * .5f;

        for (int i = 0; i < bridge.hashedPos.Length; i++)
        {
            average += bridge.hashedPos[i];
        }
        average /= bridge.hashedPos.Length;

        Vector3 middle = (bridge.start + bridge.end) / 2;
        float newY = Vector3.Distance(middle, average);
        // float newY2 = Vector3.Distance(middle, average2);
        Vector3 newAverage = new Vector3(average.x, average.y + newY, average.z); //AHA!
                                                                                  // Vector3 newAverage2 = new Vector3(average2.x, average2.y + newY2, average2.z); //AHA!

        // get new average Ys
        float t = 0f;
        float totalDist = Vector3.Distance(bridge.start, bridge.end);

        Vector3 dir = (bridge.end - bridge.start).normalized, maxPoint = Vector3.zero;
        float maxDist = 0f;





        //NEW
        for (int i = 0; i < bridge.hashedPos.Length; i++)
        {
            Vector3 point = Mathfs.NearestPointOnFiniteLine(out float perpDist, bridge.start, bridge.end, bridge.hashedPos[i]);
            GameObject cr2 = Instantiate(crystalTest);
            cr2.GetComponent<MeshRenderer>().material.color = Color.green;
            cr2.transform.position = point;

            //  float perpDist = ()
            // distance = ((Vector2)v0 - cut).magnitude;
            //  Vector3 perp = dir - bridge.hashedPos[i];
            //  float perpDist = Vector3.Dot(dir, bridge.hashedPos[i]);
            //Vector3 pointVec = bridge.hashedPos[i] - bridge.start;
            //float perpDist = Vector3.Dot(pointVec, dir);
            //get vector from point on line to point in space
            //Vector3 linePointToPoint = point - linePoint;

            //float t = Vector3.Dot(linePointToPoint, lineVec);


            //return linePoint + lineVec * t;
            //  float distance = Vector3.Cross(ray.direction, point - ray.origin).magnitude;
            if (perpDist > maxDist)
            {
                maxDist = perpDist;
                maxPoint = bridge.hashedPos[i]; // <- MIDPOINT OF BEZIER
            }
            Debug.Log("Dist: " + perpDist);
        }

        Debug.Log($"Fount max Dist at {maxDist}");
        GameObject cr1 = Instantiate(crystalTest);
        cr1.GetComponent<MeshRenderer>().material.color = Color.green;
        cr1.transform.position = maxPoint;
        // NEW END


        for (int i = 0; i < bridge.hashedPos.Length; i++)
        {
            t = Vector3.Distance(bridge.hashedPos[i], bridge.end) / totalDist; // get percentual how far away from point average
                                                                               // Vector3 pos = Mathfs.GetBezierPointWithOneCP(bridge.start, newAverage, bridge.end, t);
            Vector3 pos;



            pos = Mathfs.GetBezierPointWithOneCP(bridge.start, newAverage, bridge.end, t);


            //if (Vector3.Distance(average, average2) < totalDist * .25f)
            //{
            //    newAverage += newAverage2;
            //    pos = Mathfs.GetBezierPointWithOneCP(bridge.start, newAverage, bridge.end, t);
            //    Debug.Log("Drawing with one point");
            //}
            //else pos = Mathfs.GetBezierPointWithTwoCP(bridge.start, newAverage, newAverage2, bridge.end, t);


            // Any Point projected on line for Bezier P = (1 - t)P0 + (t)P1
            //Vector3 p = ((1 - t) * bridge.start) + (t * bridge.end);
            // Therefore Point on Bezier P1 = (P - (1 - t)P0) / t
            //Vector3 p1 = (p - (1 - t) * bridge.start) / t;
            //Vector3 pos = Mathfs.GetBezierPointWithTwoCP(bridge.start, newAverage, newAverage, bridge.end, t);

            //Vector3 newPos = SampleParabola(bridge.start, bridge.end, newY, i);
            // bridge.hashedPos[i].y = Mathf.Lerp(bridge.start.y, newAverage.y, t); // Get new y value using the new average y
            //Debug.Log($"New t is {t} and y is {bridge.hashedPos[i].y}");
            GameObject cr = Instantiate(crystalTest);
            cr.GetComponent<MeshRenderer>().material.color = Color.black;
            cr.transform.position = pos;
        }

        // DEBUGGING -------------------------------------------------------
        // Debug.Log($"Average: {average}, Start: {bridge.start}, End: {bridge.end}");
        //GameObject crys = Instantiate(crystalTest);
        //crys.GetComponent<MeshRenderer>().material.color = Color.white;
        //crys.transform.position = average;
        // iterate: for each if > average*2
        // add bezier point
        GameObject crys2 = Instantiate(crystalTest);
        crys2.GetComponent<MeshRenderer>().material.color = Color.green;
        crys2.transform.position = bridge.start;

        GameObject crys3 = Instantiate(crystalTest);
        crys3.GetComponent<MeshRenderer>().material.color = Color.red;
        crys3.transform.position = bridge.end;

        GameObject crys4 = Instantiate(crystalTest);
        crys4.GetComponent<MeshRenderer>().material.color = Color.yellow;
        crys4.transform.position = newAverage;
        // construct bezier

    }

    private void GetSpawnArea()
    {
        //! Rotation to face towards needs to be found by first 2 mouse Pos

        // Get Circle Spawn
        int rows = (int)(AreaRadius / ClusterRadius);
        //Get Front Half
        for (int i = 0; i < rows; i++)
        {
            float r = ClusterRadius * i;
            float U = 2 * Mathfs.PI * r;
            int columns = (int)(U / ClusterRadius);
            for (int j = 0; j < (columns * .5); j++)
            {
                SpawnCrystalFromPool(j, columns, r);

            }

        }

    }

    private void GetEndArea()
    {
        // Get Circle Spawn
        // float area = Mathfs.PI * Radius * Radius;
        int rows = (int)(AreaRadius / ClusterRadius);
        //Get Front Half
        for (int i = 0; i < rows; i++)
        {
            float r = ClusterRadius * i;
            float U = 2 * Mathfs.PI * r;
            int columns = (int)(U / ClusterRadius);
            for (int j = (int)(columns * .5); j < columns; j++)
            {
                SpawnCrystalFromPool(j, columns, r);


            }

        }
    }

    private void GetElongatedArea()
    {
        int rows = (int)(AreaRadius / ClusterRadius);
        //Get Front Half
        for (int i = 0; i < rows; i++)
        {
            float r = ClusterRadius * i;

            float dist = Vector3.Distance(mousePositions[mousePositions.Count - 2], mousePositions[mousePositions.Count - 1]);

            int columns = (int)(dist / ClusterRadius);
            //  Debug.Log(columns);
            if (columns < 2) columns = 2;
            // if (columns < 2) return;

            for (int j = 0; j < (columns * .5); j++)
            {
                SpawnCrystalFromPool(j, columns, r);
                // Debug.Log("instantiating");
            }

        }
    }


    private void SpawnCrystalFromPool(int j, int columns, float r) // With Mouse Position
    {
        float t = j / (float)columns;
        float angRad = t * Mathfs.TAU; // angle in radians   
        Vector2 dir = Mathfs.GetUnitVectorByAngle(angRad);
        Vector3 pos = dir * r;
        pos = Quaternion.Euler(90, 0, 0) * pos;
        Vector3 wPos = GetWorldPosFromMouse() + pos;

        pooler.SpawnFromPool("Crystal", wPos, Quaternion.identity);
    }

    private void SpawnCrystalFromPool(int j, int columns, float r, Vector3 wPos) // With fixed world Pos
    {
        float t = j / (float)columns;
        float angRad = t * Mathfs.TAU; // angle in radians   
        Vector2 dir = Mathfs.GetUnitVectorByAngle(angRad);
        Vector3 pos = dir * r;
        pos = Quaternion.Euler(90, 0, 0) * pos;
        wPos += pos;

        pooler.SpawnFromPool("Crystal", wPos, Quaternion.identity);
    }

    private void SpawnCrystalFromPool(int j, int columns, float r, Vector3 wPos, Vector3 direction) // With fixed world Pos and Direction
    {
        // get angle between local forward and world forward
        float angle = Vector3.SignedAngle(Vector3.forward, direction, Vector3.up);
        // convert to unit circle
        angle = 360f - angle;
        angle *= Mathf.Deg2Rad;

        float t = (j / (float)columns);
        float angRad = t * Mathfs.TAU; // angle in radians   
        angRad += (Mathfs.PI * 0.5f);

        float maxRad = (columns * 0.33f / columns) * Mathfs.TAU;
      
        // remap to have direction middlepoint in crystals spawned
        float newRad = Mathfs.Remap(angRad, 0f, (columns * 0.33f / columns) * Mathfs.TAU, angle - (maxRad * 0.5f), angle + (maxRad * 0.5f));
        Vector2 dir = Mathfs.GetUnitVectorByAngle(newRad);

        Vector3 pos = dir * r;
        pos = Quaternion.Euler(90, 0, 0) * pos;

        wPos += pos;
        pooler.SpawnFromPool("Crystal", wPos, Quaternion.identity);
    }


    private Vector3 GetWorldPosFromMouse()
    {
        Ray raycast = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 returnVector = Vector3.zero;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hitInfo))
        {
            returnVector = hitInfo.point;
        }

        return returnVector;
    }

    private Vector3 GetWorldPosFreeFromMouse()
    {
        Plane plane = new Plane(Vector3.up, 0);

        Vector3 worldPos = Vector3.zero;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
        }
        // return cam.ScreenToWorldPoint(Input.mousePosition);


        return worldPos;
    }


}
