using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class CommandCrystalInit : MonoBehaviour
{
    public ComputeShader shader;
    public CrystalConfig runningConfig;

    [SerializeField] int verticesNum = 0;
    [SerializeField] int indicesCount;

    float radius = 5f;


    [Serializable]
    public struct Crystal
    {
        public float radius;
        public float length; // do random calcs on cpu
        public float segments; // do random calcs on cpu
        public float pointiness;
        public Vector3 offset;
        public int verticesCount;
        public int indicesCount;
        public Vector3 debugColor;
        public Vector2 rotXZ;
    };

    public struct Vert
    {
        public Vector3 pos;
        public Vector2 uv;
        public Vector3 normal;
        public Vector3 debugCol;
    };

    public struct Cluster
    {
        public float[] radii;
        public Vector2[] offsets;
        public Vector3[] debugColor;
        public Vector2[] rotXZ;
    };

    public Material _mat;
    public CameraEvent _cameraEvent;
    private Dictionary<Camera, CommandBuffer> _cams = new Dictionary<Camera, CommandBuffer>();

    public List<Crystal> Tests = new List<Crystal>();

    ComputeBuffer verticesBuffer;
    ComputeBuffer uvsBuffer;
    ComputeBuffer indicesBuffer;
    ComputeBuffer crystalInfoBuffer;
    ComputeBuffer vertOutputBuffer;


    // Start is called before the first frame update
    void Start()
    {
        GenCrystalCluster( GenerateClusterInstructions());

    }



    public void OnDisable()
    {
        if ( verticesBuffer != null ) verticesBuffer.Dispose();
        if ( uvsBuffer != null ) uvsBuffer.Dispose();
        if ( indicesBuffer != null ) indicesBuffer.Dispose();
        if ( crystalInfoBuffer != null ) crystalInfoBuffer.Dispose();
        if ( vertOutputBuffer != null ) vertOutputBuffer.Dispose();


        foreach ( var camera in _cams )
        {
            if ( camera.Key ) camera.Key.RemoveCommandBuffer( _cameraEvent, camera.Value );
        }

        _cams.Clear();
    }



    private void OnWillRenderObject()
    {
        bool isActive = gameObject.activeInHierarchy && enabled;
        if ( !isActive )
        {
            OnDisable();
            return;
        }

        var cam = Camera.current;
        if ( !cam ) return;

        // clear cmd buffer
        CommandBuffer cmd = null;

        // if cmd buffer is already registered on camera, return
        if ( _cams.ContainsKey( cam ) ) return;

        cmd = new CommandBuffer();
        cmd.name = "Buffer Test";
        _mat.SetPass( 0 );

        _cams[cam] = cmd;
        cmd.DrawProcedural( transform.localToWorldMatrix, _mat, -1, MeshTopology.Triangles, indicesTotal, 1 );
        //    cmd.DispatchCompute( shader, shader.FindKernel( "CSMain" ), 1, 1, 1 );

        cam.AddCommandBuffer( _cameraEvent, cmd );

        // Execute Render Commands


    }

    Crystal GenCrystal(float radius, Vector2 offset, Vector3 debugCol, Vector2 rotXZ)
    {
        // Setup Params for Crystal Gen
        int length = Random.Range( runningConfig.MinLength, runningConfig.MaxLength + 1 );
        int segments = Random.Range( runningConfig.MinSegments, runningConfig.MaxSegments );
        float pointiness = Random.Range( runningConfig.MinPointiness, runningConfig.MaxPointiness );

        verticesNum = length * segments * 2;
        indicesCount = ( 6 * ( length - 1 ) ) + ( ( length - 1 ) ) * ( 3 * ( ( segments * 2 ) - 2 ) );

        Crystal c = new Crystal();
        c.radius = radius;
        c.length = length;
        c.pointiness = pointiness;
        c.segments = segments;
        c.offset = new Vector3( offset.x, 0, offset.y );
        c.indicesCount = indicesCount;
        c.verticesCount = verticesNum;
        c.rotXZ = rotXZ;
        c.debugColor = debugCol;

        return c;
    }

    int vertTotal = 0, indicesTotal = 0;

    private Cluster GenerateClusterInstructions()
    {
        Cluster cl = new Cluster();

        // Get first X in Quadrant I for 1st Crystal
        int x = runningConfig.ClusterRadius / 2;
        // 1st Crystal r = dist to (0,0) MAX ; Z is MIN r
        float r = Random.Range( x * .75f, x );
        int z = (int)Random.Range( r, r * 1.25f );
        Vector2 A = new Vector2( x, z );

        // get next 2 pos in ~60° triangle shape from 1st crystal with dist to each MAXr * 2
        int x2 = - (int)Random.Range( r * .75f , r );
        int z2 = (int)Random.Range( 0, x * .5f );
        Vector2 B = new Vector2( x2, z2 );
        float r2 = Random.Range( x2 * .85f, x2 );

        float c = Vector2.Distance(A, B) * ( Mathf.Sin( 60 * Mathf.Deg2Rad ) / Mathf.Sin( 60 * Mathf.Deg2Rad ) );
        int x3 = x2 * 2 + (int)( c * Mathf.Cos( 60 * Mathf.Deg2Rad ) );
        int z3 = z2 * 2 + (int)( c * Mathf.Sin( 60 * Mathf.Deg2Rad ) );

        Vector2 C = new Vector2( x3, z3 );
        float r3 = Mathf.Clamp( Random.Range( x * .5f, c * .85f ), x * .5f, r);





        // 2 pass

        // get new points in mid sek direction on all sides
        Vector2 mAB = new Vector2( A.x + ( B.x - A.x ) / 2 , A.y + ( B.y - A.y ) / 2 );
        Vector2 mAC = new Vector2( A.x + ( C.x - A.x ) / 2 , A.y + ( C.y - A.y ) / 2 );
        Vector2 mBC = new Vector2( B.x + ( C.x - B.x ) / 2 , B.y + ( C.y - B.y ) / 2 );

        Vector2 cP = new Vector2(
            ( A.x + B.x + C.x ) / 3,
            ( A.y + B.y + C.y ) / 3
            );


        Vector2 sekAB = mAB + ( new Vector2( Vector3.Normalize(mAB - cP).x, Vector3.Normalize( mAB - cP ).y) * x);
        Vector2 sekAC = mAC + ( new Vector2( Vector3.Normalize(mAC - cP).x, Vector3.Normalize( mAC - cP ).y) * x);
        Vector2 sekBC = mBC + ( new Vector2( Vector3.Normalize(mBC - cP).x, Vector3.Normalize( mBC - cP ).y) * x);

        float[] pass2radii = new float[3];
        Vector2[] rotas = new Vector2[6];
        for ( int i = 0; i < 3; i++ )
        {
            pass2radii[i] = Random.Range( r * .3f, r * .6f );
        }
        for ( int i = 0; i < 6; i++ )
        {
            rotas[i] = new Vector2( Random.Range( -20, 20 ), Random.Range( -20, 20 ) );
        }


        // DEBUG
        Debug.DrawLine( A, B, Color.red, 800f );
        Debug.DrawLine( B, C, Color.blue, 800f );
        Debug.DrawLine( C, A, Color.green, 800f );
        Debug.DrawLine( mBC, sekBC, Color.yellow, 800f );
        Debug.DrawLine( mAB, sekAB, Color.grey, 800f );
        Debug.DrawLine( mAC, sekAC, Color.white, 800f );

        // rotation: all crystals slightly angled away from center on X & Z


        // OUTPUT
        cl.radii = new float[6] { r, r2, r3, pass2radii[0], pass2radii[1], pass2radii[2] };
        cl.offsets = new Vector2[6] { A, B, C, sekAB, sekBC, sekAC };
        cl.rotXZ = rotas;
        cl.debugColor = new Vector3[6]
        {
            new Vector3(1, 0, 0), // red A
            new Vector3(0, 0, 1), // blue B
            new Vector3(0, 1, 0), // green C
            new Vector3(.66f, 0, 0), // red AB
            new Vector3(0, 0, .66f), // blue BC
            new Vector3(0, .66f, 0) // green CA
        };
        return cl;

    }

    void GenCrystalCluster(Cluster cl)
    {
        Crystal[] cluster = new Crystal[cl.radii.Length];
        for ( int i = 0; i < cluster.Length; i++ )
        {
            cluster[i] = GenCrystal( cl.radii[i], cl.offsets[i], cl.debugColor[i], cl.rotXZ[i] );
        }

        Tests.AddRange( cluster ); //DEBUG

        // GET TOTAL VERT / INDICES COUNT
        for ( int i = 0; i < cluster.Length; i++ )
        {
            vertTotal += cluster[i].verticesCount;
            indicesTotal += cluster[i].indicesCount;
        }

        // INIT BUFFERS
        verticesBuffer = new ComputeBuffer( vertTotal, sizeof( float ) * 3, ComputeBufferType.Default );
        uvsBuffer = new ComputeBuffer( vertTotal, sizeof( float ) * 2, ComputeBufferType.Default );
        crystalInfoBuffer = new ComputeBuffer( cluster.Length, Marshal.SizeOf( typeof( Crystal ) ), ComputeBufferType.Default );
        indicesBuffer = new ComputeBuffer( indicesTotal, sizeof( uint ), ComputeBufferType.Default );
        vertOutputBuffer = new ComputeBuffer( indicesTotal, Marshal.SizeOf(typeof (Vert)), ComputeBufferType.Default );

        // Set Mesh Config Data
        crystalInfoBuffer.SetData( cluster );
        crystalInfoBuffer.SetCounterValue( (uint) cluster.Length );
        int kernelHandle = shader.FindKernel( "CSMain" );

        // RUN COMPUTE SHADER
        shader.SetBuffer( kernelHandle, "VerticesBuffer", verticesBuffer );
        shader.SetBuffer( kernelHandle, "UvsBuffer", uvsBuffer );
        shader.SetBuffer( kernelHandle, "CrystalInfoBuffer", crystalInfoBuffer );
        shader.SetBuffer( kernelHandle, "IndicesBuffer", indicesBuffer );
        shader.SetBuffer( kernelHandle, "VertOutputBuffer", vertOutputBuffer );
        shader.SetFloat( "_VertCount", indicesTotal );

        // DISPATCH
        shader.Dispatch( kernelHandle, 1, 1, 1 );


        _mat.SetBuffer( "vertBuffer", vertOutputBuffer );


    }

    void OLDGenCrystalCluster( Cluster cl )
    {
        int count = 2;
        Crystal[] cluster = new Crystal[count];
        for ( int i = 0; i < count; i++ )
        {
            // Setup Params for Crystal Gen
            int length = Random.Range( runningConfig.MinLength, runningConfig.MaxLength + 1 );
            int segments = Random.Range( runningConfig.MinSegments, runningConfig.MaxSegments );
            float pointiness = Random.Range( runningConfig.MinPointiness, runningConfig.MaxPointiness );

            verticesNum = length * segments * 2;
            indicesCount = ( 6 * ( length - 1 ) ) + ( ( length - 1 ) ) * ( 3 * ( ( segments * 2 ) - 2 ) );

            float offXZ = 0;
            if ( i > 0 ) offXZ = Random.Range( radius, radius * 3 );

            // CRYSTAL TESTS
            Crystal testCrystal = new Crystal();
            testCrystal.radius = radius;
            testCrystal.length = length;
            testCrystal.pointiness = pointiness;
            testCrystal.segments = segments;
            testCrystal.offset = new Vector3( offXZ, 0, offXZ );
            // testCrystal.offset = Vector3.zero;
            testCrystal.indicesCount = indicesCount;
            testCrystal.verticesCount = verticesNum;

            cluster[i] = testCrystal;
        }
        Tests.AddRange( cluster );

        // GET TOTAL VERT / INDICES COUNT
        for ( int i = 0; i < cluster.Length; i++ )
        {
            vertTotal += cluster[i].verticesCount;
            indicesTotal += cluster[i].indicesCount;
        }

        // INIT BUFFERS
        verticesBuffer = new ComputeBuffer( vertTotal, sizeof( float ) * 3, ComputeBufferType.Default );
        uvsBuffer = new ComputeBuffer( vertTotal, sizeof( float ) * 2, ComputeBufferType.Default );
        crystalInfoBuffer = new ComputeBuffer( cluster.Length, Marshal.SizeOf( typeof( Crystal ) ), ComputeBufferType.Default );
        indicesBuffer = new ComputeBuffer( indicesTotal, sizeof( uint ), ComputeBufferType.Default );
        vertOutputBuffer = new ComputeBuffer( indicesTotal, Marshal.SizeOf( typeof( Vert ) ), ComputeBufferType.Default );

        // Set Mesh Config Data
        crystalInfoBuffer.SetData( cluster );
        crystalInfoBuffer.SetCounterValue( (uint)cluster.Length );
        int kernelHandle = shader.FindKernel( "CSMain" );

        // RUN COMPUTE SHADER
        shader.SetBuffer( kernelHandle, "VerticesBuffer", verticesBuffer );
        shader.SetBuffer( kernelHandle, "UvsBuffer", uvsBuffer );
        shader.SetBuffer( kernelHandle, "CrystalInfoBuffer", crystalInfoBuffer );
        shader.SetBuffer( kernelHandle, "IndicesBuffer", indicesBuffer );
        shader.SetBuffer( kernelHandle, "VertOutputBuffer", vertOutputBuffer );
        shader.SetFloat( "_VertCount", indicesTotal );

        // DISPATCH
        shader.Dispatch( kernelHandle, 1, 1, 1 );


        _mat.SetBuffer( "vertBuffer", vertOutputBuffer );

        // DEBUG
        Vert[] output = new Vert[indicesTotal];
        vertOutputBuffer.GetData( output );
     //   OutputVert.AddRange( output );

    }

}
