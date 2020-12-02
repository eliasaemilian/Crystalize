using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicPlayerController : MonoBehaviour
{
    public LayerMask GroundedLayerMask;                     // Includes all Layers considered Ground

    [SerializeField] float _speed = 0.0f;                   // Player Speed
    [SerializeField] float _speedRun = 0.0f;                // Player Speed Running
    [SerializeField] float _timeToSpeedUp = 0.0f;           // How Long Player needs to press Movement Key to start running
    [SerializeField] float _rotationSpeed = 0.0f;           // Player Rotation Speed
    [SerializeField] float _jumpForce = 0.0f;               // Player Jump Velocity
    [SerializeField] float _jumpForceRun = 0.0f;            // Player Running Jump Velocity
    [SerializeField] private Transform _feet = null;        // Player Feet Position for Ground Check
    [SerializeField] private float _groundAdjust = 0.0f;    // Adjust size of PhysicsRaycast Radius for appropriate Ground Check

    [SerializeField] private bool _isJumping;

    private Rigidbody _rb = null;
    private Collider _col = null;

    private bool _isGrounded;                       // true if Player is considered Grounded
    private bool _freezePlayer;                     // true sets Player Movement & Input as restricted
    private float _currentJumpForce;
    private float _currentSpeed;
    private float _runTimer;


    private float _velocity;
    private Vector3 _startingPosition;

    public bool FreezeJump;

    private Camera _cam;
    public float CameraSpeed;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponentInChildren<CapsuleCollider>();
        _startingPosition = transform.position;
        _rb.isKinematic = false;
        _cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        CheckGrounded();

    }

    // Movement and Rigidbody Updates
    private void FixedUpdate()
    {
        if (!_freezePlayer) Move();
    }

    private void CheckGrounded()
    {
        if (Physics.CheckSphere(_feet.position, _col.bounds.extents.y - _groundAdjust, GroundedLayerMask, QueryTriggerInteraction.Ignore))
        {
            _isGrounded = true;
        }
        else _isGrounded = false;

        //  Debug.Log($"Is Grounded is {_isGrounded}");

    }

    private void Move()
    {
        // On Input change, start timer
        if (Input.GetAxis("Vertical") > 0) _runTimer += Time.deltaTime;
        else _runTimer = 0;

        // default: standing still
        // walk if player is going backwards or slowly walking ahead
        if (_runTimer != 0 && _runTimer < _timeToSpeedUp || Input.GetAxis("Vertical") < 0)
        { // Walk
            _currentSpeed = _speed;
        }
        else // Run
        {
            _currentSpeed = _speedRun;
        }

        // Move Rigidbody
        transform.Rotate(0, Input.GetAxis("Horizontal") * Time.deltaTime * _rotationSpeed, 0);
        transform.Translate(0, 0, Input.GetAxis("Vertical") * Time.deltaTime * _currentSpeed);

        _velocity = Mathf.Lerp(_velocity, Input.GetAxis("Vertical"), Time.deltaTime * 10f);

        //JUMP
        if (Input.GetAxis("Jump") != 0 && _isGrounded && !_isJumping && !FreezeJump)
        {

            _isJumping = true;

            // change to stronger jump if player is running
            _currentJumpForce = _jumpForce;

            _rb.AddForce(Vector3.up * _currentJumpForce, ForceMode.Impulse);
        }
        else if (Input.GetAxis("Jump") == 0) _isJumping = false;



    }
}
