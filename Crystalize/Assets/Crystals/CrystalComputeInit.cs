using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CrystalComputeInit : MonoBehaviour
{
    public ComputeShader shader;
    public Material mat; // geometry shader
    public Material debugMat;
    public bool DisableDebugGeneration;

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
    ComputeBuffer debugBuffer;
    ComputeBuffer computeOutputBuffer;

    int verticesNum = 0;

    public List<Vector3> ComputeVertices;
    public List<Vector3> GeneratedVertices;

    public List<int> ComputeIndices;
    public List<int> GeneratedIndices;

    public List<uint> DEBUG_I;
    public List<uint> DEBUG_COUNT;
    public List<Vector3> DEBUG_VERT;


    float debugR = 0f; 

    Camera cam;

    public CameraEvent _cameraEvent;
    private Dictionary<Camera, CommandBuffer> _cams = new Dictionary<Camera, CommandBuffer>();

    struct debug
    {
        public uint i;
        public uint count;
        public Vector3 vertex;
    };

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
        if (debugBuffer != null) debugBuffer.Dispose();
        if ( computeOutputBuffer != null) computeOutputBuffer.Dispose();

        foreach ( var camera in _cams )
        {
            if ( camera.Key ) camera.Key.RemoveCommandBuffer( _cameraEvent, camera.Value );
        }
    }

    void OnRenderObject()
    {
        mat.SetPass( 0 );
        Graphics.DrawProceduralNow( MeshTopology.Triangles, verticesNum, 1 );
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
        computeOutputBuffer = new ComputeBuffer( verticesNum, sizeof( float ) * 3, ComputeBufferType.Default );

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
        shader.SetBuffer( kernelHandle, "ComputeOutputBuffer", computeOutputBuffer );
        shader.SetFloat( "_VertCount", verticesNum );

        // DISPATCH

        shader.Dispatch( kernelHandle, 1, 1, 1 );

        // RETRIEVE DATA

        Vector3[] vertOut = new Vector3[verticesNum];
        computeOutputBuffer.GetData( vertOut );

        Vector2[] uvsOut = new Vector2[verticesNum];
        uvsBuffer.GetData( uvsOut );

        int[] indisOut = new int[indicesCount];
        indicesBuffer.GetData( indisOut );

        // DEBUG
        ComputeVertices.AddRange( vertOut );
        ComputeIndices.AddRange( indisOut );


        debugBuffer = new ComputeBuffer( verticesNum, sizeof( uint ) * 2 + sizeof(float) * 3 , ComputeBufferType.Default );

        // CPU SIDE DEBUG
        var ini = InstantiateMesh( out int indicount, radius, length, segments, pointiness );
        if ( DisableDebugGeneration ) ini.gameObject.SetActive( false );

        // SET BUFFERS FOR GEO SHADER
        mat.SetBuffer( "VerticesBuffer", verticesBuffer );
        mat.SetBuffer( "IndicesBuffer", indicesBuffer );
        mat.SetBuffer( "UvsBuffer", uvsBuffer );
        mat.SetBuffer( "ComputeOutputBuffer", computeOutputBuffer );

        debug[] debugOut = new debug[verticesNum];
        debugBuffer.GetData( debugOut );
        for ( int i = 0; i < debugOut.Length; i++ )
        {
            DEBUG_COUNT.Add(debugOut[i].count);
            DEBUG_I.Add(debugOut[i].i);
            DEBUG_VERT.Add(debugOut[i].vertex);
        }

        mat.SetPass( 0 );
        

        return crystal;
            
//Vector3[] verts = new Vector3[verticesNum];
//        int[] indis = new int[indicesCount];
//        mesh.SetVertices( verts );
//        mesh.SetTriangles( indis, 0 );

        //crystal.GetComponent<MeshFilter>().mesh = mesh;
        //crystal.GetComponent<MeshRenderer>().material = mat;
        //return crystal;












  //      MaterialPropertyBlock matProps = new MaterialPropertyBlock();
  //      //matProps.SetVectorArray( "Vert", vert );
  //      //matProps.SetBuffer( "VerticesBuffer", verticesBuffer );
  //      //matProps.SetBuffer( "IndicesBuffer", indicesBuffer );

  //      CommandBuffer cmd = new CommandBuffer();
  //      cmd.DispatchCompute( shader, kernelHandle, 1, 1, 1 );

  //      // SET BUFFERS FOR GEO SHADER
  //      mat.SetBuffer( "VerticesBuffer", verticesBuffer );
  //      mat.SetBuffer( "IndicesBuffer", indicesBuffer );
  //      mat.SetBuffer( "UvsBuffer", uvsBuffer );


  //      Vector3[] vertOut = new Vector3[verticesNum];
  //      verticesBuffer.GetData( vertOut );

  //      Vector2[] uvsOut = new Vector2[verticesNum];
  //      uvsBuffer.GetData( uvsOut );

  //      int[] indisOut = new int[indicesCount];
  //      indicesBuffer.GetData( indisOut );

  //      ComputeVertices.AddRange( vertOut );

  //      Vector4[] vert = new Vector4[vertOut.Length];
  //      for ( int i = 0; i < vertOut.Length; i++ )
  //      {
  //          vert[i] = new Vector4( vertOut[i].x, vertOut[i].y, vertOut[i].z, 1 );
  //          ComputeVertices.Add( vertOut[i] );
  //      }

  //      cam.AddCommandBuffer( CameraEvent.AfterForwardOpaque, cmd );
  //      cmd.DrawProcedural( Matrix4x4.identity, mat, -1, MeshTopology.Triangles, verticesNum, 1, matProps );


  //      List<Vector3> normals = new List<Vector3>();

  //      // Set Mesh
  //      Vector3[] verts = new Vector3[verticesNum];
  //      int[] indis = new int[indicesCount];
  //      mesh.SetVertices( verts );
  //      mesh.SetTriangles( indis, 0 );
  ////      mesh.SetIndices( indis, MeshTopology.Triangles, 0 );
  //      //  mesh.SetNormals(normals);
  //      //   mesh.SetUVs( 0, uvsOut );
  //      //mesh.RecalculateNormals();
  //   //   Graphics.DrawProcedural( mat, mesh.bounds, MeshTopology.Triangles, verticesNum );


  //      crystal.GetComponent<MeshFilter>().mesh = mesh;
  //      //  crystal.GetComponent<MeshRenderer>().material = new Material( GetComponent<MeshRenderer>().material );
  //      crystal.GetComponent<MeshRenderer>().material = mat;
  //      return crystal;
    }


    private GameObject InstantiateMesh( out int indicesCount, float radius, float length, int segments, float pointiness )
    {
        GameObject crystal = new GameObject();
        crystal.transform.SetParent( transform );
        crystal.AddComponent( typeof( MeshRenderer ) );
        crystal.AddComponent( typeof( MeshFilter ) );
        Mesh mesh = new Mesh { name = "CrystalMesh" };


        // VERTICES
        List<Vector3>vertices = new List<Vector3>();
        List<Vector3>normals = new List<Vector3>();
        List<Vector2>uvs = new List<Vector2>();
        for ( int i = 0; i < length; i++ )
        {
            for ( int j = 0; j < segments; j++ )
            {
                Vector3 vertPos;
                if ( i == length - 1 ) // top point for crystal (double row)
                {
                    vertPos = transform.position;
                    vertPos.y += i + pointiness;
                }
                else
                {
                    float t = j / (float)segments;
                    float angRad = t * Mathfs.TAU; // angle in radians   

                    // Get Basic Corpus
                    Vector2 dir = Mathfs.GetUnitVectorByAngle( angRad );

                    vertPos = dir * radius;
                    vertPos = Quaternion.Euler( 90, 0, 0 ) * vertPos; // rotate to point crystal upwards
                    vertPos.y += i; //enlongate on y

                }

                vertices.Add( vertPos );
                vertices.Add( vertPos );
                float v = vertPos.y / ( length + pointiness );
                float u = j / (float)segments;
                uvs.Add( new Vector2( u, v ) );
                uvs.Add( new Vector2( u, v ) );

            }


        }


        List <int> indices = new List<int>();

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

        // DEBUG VALUES
        GeneratedVertices.AddRange( vertices );
        GeneratedIndices.AddRange( indices );

        // Set Mesh
        mesh.SetVertices( vertices );
        mesh.SetTriangles( indices, 0 );
        //  mesh.SetNormals(normals);
        mesh.SetUVs( 0, uvs );
        mesh.RecalculateNormals();

        crystal.GetComponent<MeshFilter>().mesh = mesh;
        crystal.GetComponent<MeshRenderer>().material = debugMat;

        indicesCount = indices.Count;
        return crystal;
    }
}
