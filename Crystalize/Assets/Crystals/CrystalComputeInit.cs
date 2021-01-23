using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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

    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
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

     //   GraphicsBuffer debugBuffer = new GraphicsBuffer()

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

     //   shader.Dispatch( kernelHandle, 1, 1, 1 );
        // SET BUFFERS FOR GEO SHADER
        mat.SetBuffer( "VerticesBuffer", verticesBuffer );
        mat.SetBuffer( "IndicesBuffer", indicesBuffer );
        mat.SetBuffer( "UvsBuffer", uvsBuffer );



        Vector3[] vertOut = new Vector3[verticesNum];
        verticesBuffer.GetData( vertOut );

        Vector2[] uvsOut = new Vector2[verticesNum];
        uvsBuffer.GetData( uvsOut );

        int[] indisOut = new int[indicesCount];
        indicesBuffer.GetData( indisOut );

        ComputeVertices.AddRange( vertOut );

        Vector4[] vert = new Vector4[vertOut.Length];
        for ( int i = 0; i < vertOut.Length; i++ ) vert[i] = new Vector4( vertOut[i].x, vertOut[i].y, vertOut[i].z, 0 );


        MaterialPropertyBlock matProps = new MaterialPropertyBlock();
        matProps.SetVectorArray( "Vert", vert );
        matProps.SetBuffer( "VerticesBuffer", verticesBuffer );
        matProps.SetBuffer( "IndicesBuffer", indicesBuffer );

        CommandBuffer cmd = new CommandBuffer();
        cmd.DispatchCompute( shader, kernelHandle, 1, 1, 1 );
        
        cam.AddCommandBuffer( CameraEvent.AfterForwardOpaque, cmd );
        cmd.DrawProcedural( Matrix4x4.identity, mat, -1, MeshTopology.Triangles, verticesNum, 1, matProps );


        List<Vector3> normals = new List<Vector3>();

        // Set Mesh
        Vector3[] verts = new Vector3[verticesNum];
        int[] indis = new int[3];
        mesh.SetVertices( verts );
        mesh.SetTriangles( indis, 0 );
  //      mesh.SetIndices( indis, MeshTopology.Triangles, 0 );
        //  mesh.SetNormals(normals);
        //   mesh.SetUVs( 0, uvsOut );
        //mesh.RecalculateNormals();
     //   Graphics.DrawProcedural( mat, mesh.bounds, MeshTopology.Triangles, verticesNum );


        crystal.GetComponent<MeshFilter>().mesh = mesh;
        //  crystal.GetComponent<MeshRenderer>().material = new Material( GetComponent<MeshRenderer>().material );
        crystal.GetComponent<MeshRenderer>().material = mat;
        return crystal;
    }
}
