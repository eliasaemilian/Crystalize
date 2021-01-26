using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class CommandBufferInit : MonoBehaviour
{
    public struct Point
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector4 tangent;
        public Vector2 uv;
    }

    public Mesh mesh;
    public Material _mat;
    public CameraEvent _cameraEvent;
    private Dictionary<Camera, CommandBuffer> _cams = new Dictionary<Camera, CommandBuffer>();
    private int n = 0;

    private ComputeBuffer computebuffer;

    // Start is called before the first frame update
    void Start()
    {
        // TODO:
        /*
         * 1 write out step list
         * 2 work through steps
         * 3 test
         */


        // Set Example Vertices and Indices for Mesh
        n = mesh.triangles.Length;

        Point[] points = new Point[n];
        for ( int i = 0; i < n; ++i )
        {
            points[i].vertex = mesh.vertices[mesh.triangles[i]];
            points[i].normal = mesh.normals[mesh.triangles[i]];
            points[i].tangent = mesh.tangents[mesh.triangles[i]];
            points[i].uv = mesh.uv[mesh.triangles[i]];
        }


        // Instantiate Command Buffer
        computebuffer = new ComputeBuffer( n, Marshal.SizeOf( typeof( Point ) ), ComputeBufferType.Default );
        computebuffer.SetData( points );
        _mat.SetBuffer( "points", computebuffer );


        // Call DrawProcedural
    }

    public void OnDisable()
    {
        foreach (var camera in _cams)
        {
            if ( camera.Key ) camera.Key.RemoveCommandBuffer( _cameraEvent, camera.Value );
        }
    }

    void OnRenderObject()
    {
        _mat.SetPass( 0 );
        Graphics.DrawProceduralNow( MeshTopology.Triangles, n, 1 );
    }

    void OnDestroy()
    {
        if (computebuffer != null) computebuffer.Release();
    }

    public void STOPnWillRenderObject()
    {
        bool isActive = gameObject.activeInHierarchy && enabled;
        if (!isActive)
        {
            OnDisable();
            return;
        }

        var cam = Camera.current;
        if ( !cam ) return;

        CommandBuffer cmd = null;
        if (_cams.ContainsKey(cam))
        {
            cmd = _cams[cam];
            cmd.Clear();
        }
        else
        {
            cmd = new CommandBuffer();
            cmd.name = "Buffer Test";
            _cams[cam] = cmd;

            cam.AddCommandBuffer( _cameraEvent, cmd );
        }


        // Execute Render Commands
        cmd.SetRenderTarget( tag );
        cmd.DrawMesh( mesh, Matrix4x4.identity, _mat );

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
