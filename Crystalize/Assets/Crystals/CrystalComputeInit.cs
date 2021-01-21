using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalComputeInit : MonoBehaviour
{
    public ComputeShader shader;
    public Material mat;

    struct Crystal
    {
        public float radius;
        public float length; // do random calcs on cpu
        public float segments; // do random calcs on cpu
        public float pointiness;
    };

    public CrystalConfig runningConfig;
    float radius = 5f;

    int indicesCount;

    ComputeBuffer verticesBuffer;
    ComputeBuffer uvsBuffer;
    ComputeBuffer crystalInfoBuffer;

    int verticesNum = 0;

    public List<Vector3> ComputeVertices;
    public List<Vector3> GeneratedVertices;

    // Start is called before the first frame update
    void Start()
    {
        computeCrystal();
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDisable()
    {
        if (verticesBuffer != null) verticesBuffer.Dispose();
        if (uvsBuffer != null) uvsBuffer.Dispose();
        if (crystalInfoBuffer != null) crystalInfoBuffer.Dispose();
    }



    GameObject computeCrystal()
    {
        GameObject crystal = new GameObject();
        crystal.transform.SetParent( transform );
        crystal.AddComponent( typeof( MeshRenderer ) );
        crystal.AddComponent( typeof( MeshFilter ) );
        Mesh mesh = new Mesh { name = "CrystalMesh" };

        int length = Random.Range( runningConfig.MinLength, runningConfig.MaxLength + 1 );
        int segments = Random.Range( runningConfig.MinSegments, runningConfig.MaxSegments );
        float pointiness = Random.Range( runningConfig.MinPointiness, runningConfig.MaxPointiness );

        verticesNum = length * segments * 2;

        Crystal testCrystal = new Crystal();
        testCrystal.radius = radius;
        testCrystal.length = length;
        testCrystal.pointiness = pointiness;
        testCrystal.segments = segments;

        Crystal[] data = new Crystal[1];
        data[0] = testCrystal;

        // INIT BUFFERS
        verticesBuffer = new ComputeBuffer( verticesNum, sizeof( float ) * 3, ComputeBufferType.Default );
        uvsBuffer = new ComputeBuffer( verticesNum, sizeof( float ) * 2, ComputeBufferType.Default );
        crystalInfoBuffer = new ComputeBuffer( 1, sizeof( float ) * 4, ComputeBufferType.Default );

        // Set Mesh Config Data
        crystalInfoBuffer.SetData( data );
        crystalInfoBuffer.SetCounterValue( 1 );
        int kernelHandle = shader.FindKernel( "CSMain" );

        // RUN COMPUTE SHADER
        shader.SetBuffer( kernelHandle, "VerticesBuffer", verticesBuffer );
        shader.SetBuffer( kernelHandle, "UvsBuffer", uvsBuffer );
        shader.SetBuffer( kernelHandle, "CrystalInfoBuffer", crystalInfoBuffer );
       // shader.SetBuffer( kernelHandle, "IndicesBuffer", indicesBuffer );
        shader.Dispatch( kernelHandle, 8, 8, 1 );

        Vector3[] vertOut = new Vector3[verticesNum];
        verticesBuffer.GetData( vertOut );

        ComputeVertices.AddRange( vertOut );
        Debug.Log( "Completed Compute Step" );

        // COMPARISON

        // VERTICES
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        for ( int i = 0; i < length; i++ )
        {
            for ( int j = 0; j < segments; j++ )
            {
                Vector3 vertPos;
                float te;
                if ( i == length - 1 ) // top point for crystal (double row)
                {
                    vertPos = transform.position;
                    vertPos.y += i + pointiness;
                    te = 10;
                }
                else
                {
                    float t = j / (float)segments;
                    float angRad = t * Mathfs.TAU; // angle in radians   

                    // Get Basic Corpus
                    Vector2 dir = Mathfs.GetUnitVectorByAngle( angRad );

                    vertPos = dir * radius;
                    te = t;

                    vertPos = Quaternion.Euler( 90, 0, 0 ) * vertPos; // rotate to point crystal upwards
                    //  vertPos.y += i; //enlongate on y

                }

                vertices.Add( vertPos );
                vertices.Add( new Vector3( te, te, te ) );
                float v = vertPos.y / ( length + pointiness );
                float u = j / (float)segments;
                uvs.Add( new Vector2( u, v ) );
                uvs.Add( new Vector2( u, v ) );

            }


        }


        ////////////////
        ///
        GeneratedVertices = vertices;





        List<int> indices = new List<int>();

        // INDICES
        for ( int i = 0; i < length - 1; i++ )
        {
            for ( int j = 0; j < ( segments * 2 ) - 2; j += 2 )
            {
                int root = ( i * ( ( segments * 2 ) ) + j );
                int rootNext = ( i * ( ( segments * 2 ) ) ) + j + ( segments * 2 );

                // 1
                indices.Add( rootNext );
                indices.Add( root + 2 );
                indices.Add( root );

                // 2
                indices.Add( rootNext );
                indices.Add( rootNext + 2 );
                indices.Add( root + 2 );

                //  Debug.Log($"root is {root} and root + 1 is {root + 1} and rootNext is {rootNext} and rootNext + 1 in {rootNext + 1}");
            }

            // close off crystal between first and last row in indices in this column

            //1
            indices.Add( ( ( i + 1 ) * ( segments * 2 ) ) + ( segments * 2 ) - 2 );
            indices.Add( ( i * ( segments * 2 ) ) + ( segments * 2 ) );
            indices.Add( ( i * ( segments * 2 ) ) );

            // 2
            indices.Add( ( ( i + 1 ) * ( segments * 2 ) ) + ( segments * 2 ) - 2 );
            indices.Add( ( i * ( segments * 2 ) ) );
            indices.Add( ( i * ( segments * 2 ) ) + ( segments * 2 ) - 2 );

        }



        // Set Mesh
        mesh.SetVertices( vertices );
        mesh.SetTriangles( indices, 0 );
        //  mesh.SetNormals(normals);
        mesh.SetUVs( 0, uvs );
        mesh.RecalculateNormals();

        crystal.GetComponent<MeshFilter>().mesh = mesh;
        //  crystal.GetComponent<MeshRenderer>().material = new Material( GetComponent<MeshRenderer>().material );
        crystal.GetComponent<MeshRenderer>().material = mat;
        indicesCount = indices.Count;
        return crystal;
    }
}
