using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// STALKER-based character controller
/// </summary>
public class Actor_Movement : MonoBehaviour
{
    #region Drag Drop
    [Header("Components")]
    [SerializeField]
    private CharacterController _unityCharacterController = null;

    // Collision will not happend with these layers
    [SerializeField]
    private LayerMask _excludedLayers = 0;

    [SerializeField]
    private bool _debugInfo = false;

    [SerializeField]
    private List<Transform> _groundedRayPositions = null;
    #endregion

    #region Movement Parameters
    [Header("Movement parameters")]
    // Ad-hoc approach to make the controller accelerate faster
    [SerializeField]
    private float _groundAccelerationCoeff = 500.0f;

    // How fast the controller accelerates while it's not grounded
    [SerializeField]
    private float _airAccelCoeff = 1f;

    // Air deceleration occurs when the player gives an input that's not aligned with the current velocity
    [SerializeField]
    private float _airDecelCoeff = 1.5f;

    // Along a dimension, we can't go faster than this
    [SerializeField]
    private float _maxSpeedAlongOneDimension = 8f;

    // How fast the controller decelerates on the grounded
    [SerializeField]
    private float _friction = 15;

    // Stop if under this speed
    [SerializeField]
    private float _frictionSpeedThreshold = 0.5f;

    // Push force given when jumping
    [SerializeField]
    private float _jumpStrength = 8f;

    // Gravity amount
    [SerializeField]
    private float _gravityAmount = 24f;

    // How precise the controller can change direction while not grounded 
    [SerializeField]
    private float _airControlPrecision = 16f;

    // When moving only forward, increase air control dramatically
    [SerializeField]
    private float _airControlAdditionForward = 8f;

    // Keyboard input are enabled
    [SerializeField]
    private bool _canControl = true;

    [SerializeField]
    private bool _jumpEnabled = true;
    #endregion

    #region Fields
    // The real velocity of this controller
    private Vector3 _velocity;

    // Raw input taken with GetAxisRaw()
    private Vector3 _moveInput;

    // Caching...
    private readonly Collider[] _overlappingColliders = new Collider[10];
    private Transform _ghostJumpRayPosition;

    // Some information to persist
    private bool _isGroundedInPrevFrame;
    private bool _isGonnaJump;
    private Vector3 _wishDirDebug;
    #endregion

    private void Start()
    {
        if (_unityCharacterController == null)
            _unityCharacterController = GetComponent<CharacterController>();

        if (_groundedRayPositions != null && _groundedRayPositions.Count > 0)
            _ghostJumpRayPosition = _groundedRayPositions[_groundedRayPositions.Count - 1];
    }

    // Only for debug drawing
    private void OnGUI()
    {
        if (!_debugInfo)
        {
            return;
        }

        // Print current horizontal speed
        Vector3 ups = _velocity;
        ups.y = 0;
        GUI.Box(new Rect(Screen.width / 2f - 50, Screen.height / 2f + 50, 100, 40),
            (Mathf.Round(ups.magnitude * 100) / 100).ToString());
    }

    private void Update()
    {
        float dt = Time.deltaTime;

        if (_canControl)
        {
            // We use GetAxisRaw, since we need it to feel as responsive as possible
            _moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

            if (_jumpEnabled && Input.GetKeyDown(KeyCode.Space) && !_isGonnaJump)
            {
                _isGonnaJump = true;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                _isGonnaJump = false;
            }
        }

        // MOVEMENT
        Vector3 wishDir = transform.TransformDirectionHorizontal(_moveInput); // We want to go in this direction
        _wishDirDebug = wishDir.ToHorizontal();

        Vector3 groundNormal;
        bool isGrounded = IsGrounded(out groundNormal);

        if (isGrounded) // Ground move
        {
            // Don't apply friction if just landed or about to jump
            if (_isGroundedInPrevFrame && !_isGonnaJump)
            {
                ApplyFriction(ref _velocity, dt);
            }

            Accelerate(ref _velocity, wishDir, _groundAccelerationCoeff, dt);

            // Crop up horizontal velocity component
            _velocity = Vector3.ProjectOnPlane(_velocity, groundNormal);
            if (_isGonnaJump)
            {
                // Jump away
                _velocity.y = _jumpStrength;
            }
        }
        else // Air move
        {
            // If the input doesn't have the same facing with the current velocity
            // then slow down instead of speeding up
            float coeff = Vector3.Dot(_velocity, wishDir) > 0 ? _airAccelCoeff : _airDecelCoeff;

            Accelerate(ref _velocity, wishDir, coeff, dt);

            if (Mathf.Abs(_moveInput.z) > 0.0001) // Pure side velocity doesn't allow air control
            {
                ApplyAirControl(ref _velocity, wishDir, dt);
            }

            _velocity.y -= _gravityAmount * dt;
        }

        // Use CharacterController for movement
        Vector3 displacement = _velocity * dt;
        if (_unityCharacterController != null)
        {
            _unityCharacterController.Move(displacement);
        }
        else
        {
            transform.position += displacement;
        }

        _isGroundedInPrevFrame = isGrounded;
    }

    private void Accelerate(ref Vector3 playerVelocity, Vector3 accelDir, float accelCoeff, float dt)
    {
        // How much speed we already have in the direction we want to speed up
        float projSpeed = Vector3.Dot(playerVelocity, accelDir);

        // How much speed we need to add (in that direction) to reach max speed
        float addSpeed = _maxSpeedAlongOneDimension - projSpeed;
        if (addSpeed <= 0)
        {
            return;
        }

        // How much we are gonna increase our speed
        float accelAmount = accelCoeff * _maxSpeedAlongOneDimension * dt;

        // If we are accelerating more than in a way that we exceed maxSpeedInOneDimension, crop it to max
        if (accelAmount > addSpeed)
        {
            accelAmount = addSpeed;
        }

        playerVelocity += accelDir * accelAmount;
    }

    private void ApplyFriction(ref Vector3 playerVelocity, float dt)
    {
        Vector3 horizontalVelocity = new Vector3(playerVelocity.x, 0, playerVelocity.z);
        float speed = horizontalVelocity.magnitude;
        if (speed <= 0.00001f)
        {
            return;
        }

        float downLimit = Mathf.Max(speed, _frictionSpeedThreshold); // Don't drop below treshold
        float dropAmount = speed - (downLimit * _friction * dt);
        if (dropAmount < 0)
        {
            dropAmount = 0;
        }

        playerVelocity.x *= dropAmount / speed;
        playerVelocity.z *= dropAmount / speed;
    }

    private void ApplyAirControl(ref Vector3 playerVelocity, Vector3 accelDir, float dt)
    {
        // This only happens in the horizontal plane
        Vector3 playerDirHorz = playerVelocity.ToHorizontal().normalized;
        float playerSpeedHorz = playerVelocity.ToHorizontal().magnitude;

        float dot = Vector3.Dot(playerDirHorz, accelDir);
        if (dot > 0)
        {
            float k = _airControlPrecision * dot * dot * dt;

            // CPMA thingy:
            // If we want pure forward movement, we have much more air control
            bool isPureForward = Mathf.Abs(_moveInput.x) < 0.0001f && Mathf.Abs(_moveInput.z) > 0;
            if (isPureForward)
            {
                k *= _airControlAdditionForward;
            }

            // A little bit closer to accelDir
            playerDirHorz = playerDirHorz * playerSpeedHorz + accelDir * k;
            playerDirHorz.Normalize();

            // Assign new direction, without touching the vertical speed
            playerVelocity = (playerDirHorz * playerSpeedHorz).ToHorizontal() + Vector3.up * playerVelocity.y;
        }
    }

    // If one of the rays hit, we're considered to be grounded
    private bool IsGrounded(out Vector3 groundNormal)
    {
        groundNormal = Vector3.up;

        bool isGrounded = false;

        // Check raycasts first
        if (_groundedRayPositions != null)
        {
            foreach (Transform t in _groundedRayPositions)
            {
                // The last one is reserved for ghost jumps
                // Don't check that one if already on the ground
                if (t == _ghostJumpRayPosition && isGrounded)
                {
                    continue;
                }

                RaycastHit hit;
                if (Physics.Raycast(t.position, Vector3.down, out hit, 0.51f, ~_excludedLayers))
                {
                    groundNormal = hit.normal;
                    isGrounded = true;
                }
            }
        }

        // Also check if CharacterController is grounded
        if (_unityCharacterController != null && _unityCharacterController.isGrounded)
        {
            isGrounded = true;
        }

        return isGrounded;
    }

    // Handy when testing
    public void ResetAt(Transform t)
    {
        transform.position = t.position + Vector3.up * 0.5f;
        _velocity = t.TransformDirection(Vector3.forward);
    }
}

public static class Q3CharacterControllerExtensions
{
    public static Vector3 ToHorizontal(this Vector3 v)
    {
        return new Vector3(v.x, 0, v.z);
    }

    public static Vector3 TransformDirectionHorizontal(this Transform t, Vector3 v)
    {
        return t.TransformDirection(v).ToHorizontal().normalized;
    }

    public static Vector3 InverseTransformDirectionHorizontal(this Transform t, Vector3 v)
    {
        return t.InverseTransformDirection(v).ToHorizontal().normalized;
    }
}