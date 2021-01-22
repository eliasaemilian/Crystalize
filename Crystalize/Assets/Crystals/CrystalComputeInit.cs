using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalComputeInit : MonoBehaviour
{
    public ComputeShader shader;
    public Material mat; // geometry shader

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
    ComputeBuffer indicesBuffer;
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
        if (indicesBuffer != null) indicesBuffer.Dispose();
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
        indicesCount = ( 6 * ( length - 1 ) ) + ( ( length - 1 ) ) * ( 3 * ( ( segments * 2 ) - 2 ) );

        // set geo shader vars
        mat.SetFloat( "_VerticesCount", verticesNum );
        mat.SetFloat( "_IndicesCount", indicesCount );


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
        indicesBuffer = new ComputeBuffer( indicesCount, sizeof( uint ), ComputeBufferType.Default );

        // Set Mesh Config Data
        crystalInfoBuffer.SetData( data );
        crystalInfoBuffer.SetCounterValue( 1 );
        int kernelHandle = shader.FindKernel( "CSMain" );

        // RUN COMPUTE SHADER
        shader.SetBuffer( kernelHandle, "VerticesBuffer", verticesBuffer );
        shader.SetBuffer( kernelHandle, "UvsBuffer", uvsBuffer );
        shader.SetBuffer( kernelHandle, "CrystalInfoBuffer", crystalInfoBuffer );
        shader.SetBuffer( kernelHandle, "IndicesBuffer", indicesBuffer );




        // DISPATCH

        shader.Dispatch( kernelHandle, 1, 1, 1 );
        // SET BUFFERS FOR GEO SHADER
        mat.SetBuffer( "VerticesBuffer", verticesBuffer );
        mat.SetBuffer( "IndicesBuffer", indicesBuffer );
        mat.SetBuffer( "UvsBuffer", uvsBuffer );

        //Graphics.DrawProcedural( mat, mesh.bounds, MeshTopology.Points, 128);


        Vector3[] vertOut = new Vector3[verticesNum];
        verticesBuffer.GetData( vertOut );

        Vector2[] uvsOut = new Vector2[verticesNum];
        uvsBuffer.GetData( uvsOut );

        int[] indisOut = new int[indicesCount];
        indicesBuffer.GetData( indisOut );

        ComputeVertices.AddRange( vertOut );


        

        List<Vector3> normals = new List<Vector3>();

        // Set Mesh
        Vector3[] verts = new Vector3[6];
        int[] indis = new int[3];
        mesh.SetVertices( vertOut );
        mesh.SetTriangles( indisOut, 0 );
        //  mesh.SetNormals(normals);
        //   mesh.SetUVs( 0, uvsOut );
        //mesh.RecalculateNormals();

        crystal.GetComponent<MeshFilter>().mesh = mesh;
        //  crystal.GetComponent<MeshRenderer>().material = new Material( GetComponent<MeshRenderer>().material );
        crystal.GetComponent<MeshRenderer>().material = mat;
        return crystal;
    }
}
