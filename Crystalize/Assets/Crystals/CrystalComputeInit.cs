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
        public Vector3 wPos;
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
        testCrystal.wPos = transform.position;

        Crystal[] data = new Crystal[1];
        data[0] = testCrystal;

        // INIT BUFFERS
        verticesBuffer = new ComputeBuffer( verticesNum, sizeof( float ) * 3, ComputeBufferType.Default );
        uvsBuffer = new ComputeBuffer( verticesNum, sizeof( float ) * 2, ComputeBufferType.Default );
        crystalInfoBuffer = new ComputeBuffer( 1, sizeof( float ) * 4 + sizeof (float) * 3, ComputeBufferType.Default );

        // Set Mesh Config Data
        crystalInfoBuffer.SetData( data );
        crystalInfoBuffer.SetCounterValue( 1 );
        int kernelHandle = shader.FindKernel( "CSMain" );

        // RUN COMPUTE SHADER
        shader.SetBuffer( kernelHandle, "VerticesBuffer", verticesBuffer );
        shader.SetBuffer( kernelHandle, "UvsBuffer", uvsBuffer );
        shader.SetBuffer( kernelHandle, "CrystalInfoBuffer", crystalInfoBuffer );
       // shader.SetBuffer( kernelHandle, "IndicesBuffer", indicesBuffer );




        // DISPATCH

        shader.Dispatch( kernelHandle, 1, 1, 1 );

        Vector3[] vertOut = new Vector3[verticesNum];
        verticesBuffer.GetData( vertOut );

        Vector2[] uvsOut = new Vector2[verticesNum];
        uvsBuffer.GetData( uvsOut );

        ComputeVertices.AddRange( vertOut );
        Debug.Log( "Completed Compute Step" );

        // COMPARISON

        // DEBUG VALUES
        Vector3 gen = Vector3.zero;


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
                    gen = dir;
                    te = t;

                    vertPos = Quaternion.Euler( 90, 0, 0 ) * vertPos; // rotate to point crystal upwards
                    vertPos.y += i; //enlongate on y

                }

                vertices.Add( vertPos );
                vertices.Add( new Vector3( 1000000, 10000000, 10000000 ) );
                float v = vertPos.y / ( length + pointiness );
                float u = j / (float)segments;
                uvs.Add( new Vector2( u, v ) );
                uvs.Add( new Vector2( u, v ) );

            }


        }



        ////////////////
        ///
        GeneratedVertices = vertices;

        Debug.Log( $"Generated X: {vertices[2].x} Y: {vertices[2].y} Z: {vertices[2].z}" );
        Debug.Log( $"Computed X: {ComputeVertices[2].x} Y: {ComputeVertices[2].y} Z: {ComputeVertices[2].z}" );
        Debug.Log( $"Generated Prior: X: {gen.x} Y: {gen.y} Z: {gen.z}" );
        Debug.Log( $"Computed Propr:  X: {ComputeVertices[1].x} Y: {ComputeVertices[1].y} Z: {ComputeVertices[1].z}" );

        for ( int i = 0; i < vertices.Count; i+= 2 )
        {
            Debug.Log( $"Computed X: {ComputeVertices[i].x} Generated X:{vertices[i].x} " );
        }
        /////////////////////////


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
        mesh.SetVertices( ComputeVertices );
        mesh.SetTriangles( indices, 0 );
        //  mesh.SetNormals(normals);
        mesh.SetUVs( 0, uvsOut );
        mesh.RecalculateNormals();

        crystal.GetComponent<MeshFilter>().mesh = mesh;
        //  crystal.GetComponent<MeshRenderer>().material = new Material( GetComponent<MeshRenderer>().material );
        crystal.GetComponent<MeshRenderer>().material = mat;
        indicesCount = indices.Count;
        return crystal;
    }
}
