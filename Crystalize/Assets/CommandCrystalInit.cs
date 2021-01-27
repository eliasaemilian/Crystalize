using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class CommandCrystalInit : MonoBehaviour
{
    public ComputeShader shader;
    public CrystalConfig runningConfig;

    [SerializeField] int verticesNum = 0;
    [SerializeField] int indicesCount;

    float radius = 5f;


    public struct Point
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector4 tangent;
        public Vector2 uv;
    }

    public struct Vert
    {
        public Vector3 pos;
        public Vector2 uv;
        public Vector3 normal;
        //  public Vector4 tangent;
    }

    struct Crystal
    {
        public float radius;
        public float length; // do random calcs on cpu
        public float segments; // do random calcs on cpu
        public float pointiness;
        public Vector3 wPos;
    };

    public Material _mat;
    public CameraEvent _cameraEvent;
    private Dictionary<Camera, CommandBuffer> _cams = new Dictionary<Camera, CommandBuffer>();


    ComputeBuffer verticesBuffer;
    ComputeBuffer uvsBuffer;
    ComputeBuffer indicesBuffer;
    ComputeBuffer crystalInfoBuffer;
    ComputeBuffer vertOutputBuffer;

    // Start is called before the first frame update
    void Start()
    {

        GenerateCrystal();

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
        cmd.DrawProcedural( transform.localToWorldMatrix, _mat, -1, MeshTopology.Triangles, indicesCount, 1 );
        //    cmd.DispatchCompute( shader, shader.FindKernel( "CSMain" ), 1, 1, 1 );

        cam.AddCommandBuffer( _cameraEvent, cmd );

        // Execute Render Commands


    }


    void GenerateCrystal()
    {
        // Setup Params for Crystal Gen

        int length = Random.Range( runningConfig.MinLength, runningConfig.MaxLength + 1 );
        int segments = Random.Range( runningConfig.MinSegments, runningConfig.MaxSegments );
        float pointiness = Random.Range( runningConfig.MinPointiness, runningConfig.MaxPointiness );

        verticesNum = length * segments * 2;
        indicesCount = ( 6 * ( length - 1 ) ) + ( ( length - 1 ) ) * ( 3 * ( ( segments * 2 ) - 2 ) );

        // Command Shader

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
        crystalInfoBuffer = new ComputeBuffer( 1, sizeof( float ) * 4 + sizeof( float ) * 3, ComputeBufferType.Default );
        indicesBuffer = new ComputeBuffer( indicesCount, sizeof( uint ), ComputeBufferType.Default );
        vertOutputBuffer = new ComputeBuffer( indicesCount, Marshal.SizeOf(typeof (Vert)), ComputeBufferType.Default );

        // Set Mesh Config Data
        crystalInfoBuffer.SetData( data );
        crystalInfoBuffer.SetCounterValue( 1 );
        int kernelHandle = shader.FindKernel( "CSMain" );

        // RUN COMPUTE SHADER
        shader.SetBuffer( kernelHandle, "VerticesBuffer", verticesBuffer );
        shader.SetBuffer( kernelHandle, "UvsBuffer", uvsBuffer );
        shader.SetBuffer( kernelHandle, "CrystalInfoBuffer", crystalInfoBuffer );
        shader.SetBuffer( kernelHandle, "IndicesBuffer", indicesBuffer );
        shader.SetBuffer( kernelHandle, "VertOutputBuffer", vertOutputBuffer );
        shader.SetFloat( "_VertCount", indicesCount );

        // DISPATCH
        shader.Dispatch( kernelHandle, 1, 1, 1 );


        _mat.SetBuffer( "vertBuffer", vertOutputBuffer );



    }
}
