using UnityEngine;

/// <summary>
/// Controls Elias Cole — movement, camera, and interaction.
///
/// Mobile input: virtual joystick (left thumb zone) for movement,
/// tap anywhere on the right half of screen to interact.
///
/// Requires:
///   - CharacterController component on the same GameObject
///   - A child GameObject named "CameraTarget" for the camera to follow
///   - Main Camera in scene (Camera.main)
///
/// Reacts to GameState via EventBus — freezes input during
/// Dialogue, Cutscene, Inventory, Map, and Paused states.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────────────────

    [Header("Movement")]
    [SerializeField] private float _walkSpeed   = 2.8f;
    [SerializeField] private float _jogSpeed    = 5.2f;
    [SerializeField] private float _sprintSpeed = 8.0f;
    [SerializeField] private float _gravity     = -18f;
    [SerializeField] private float _turnSpeed   = 12f;   // degrees/sec smoothing

    [Header("Camera")]
    [SerializeField] private Transform _cameraTarget;    // child empty GameObject
    [SerializeField] private float _camDistance  = 4.5f;
    [SerializeField] private float _camHeight    = 1.8f;
    [SerializeField] private float _camSensitivity = 1.8f;
    [SerializeField] private float _camMinPitch  = -20f;
    [SerializeField] private float _camMaxPitch  =  45f;

    [Header("Interaction")]
    [SerializeField] private float _interactRange = 2.2f;
    [SerializeField] private LayerMask _interactLayers;  // set in Inspector

    [Header("Mobile Input")]
    [SerializeField] private float _joystickDeadzone = 0.12f;
    [SerializeField] private float _joystickRadius   = 80f;   // pixels

    // ── Private state ────────────────────────────────────────────────────

    private CharacterController _cc;
    private Camera              _cam;

    // Movement
    private Vector3  _velocity;          // includes gravity
    private Vector3  _moveDir;
    private float    _currentSpeed;
    private bool     _isSprinting;
    private bool     _isCrouching;

    // Camera
    private float    _camYaw;
    private float    _camPitch;
    private Vector3  _camVelocity;       // for SmoothDamp

    // Input state
    private bool     _inputEnabled = true;

    // Mobile touch tracking
    private int      _moveTouchId   = -1;
    private Vector2  _joystickOrigin;
    private Vector2  _joystickDelta;     // normalised -1..1

    private int      _camTouchId    = -1;
    private Vector2  _camLastPos;

    // Interaction
    private IInteractable _highlightedTarget;

    // ── Unity lifecycle ──────────────────────────────────────────────────

    private void Awake()
    {
        _cc  = GetComponent<CharacterController>();
        _cam = Camera.main;

        if (_cameraTarget == null)
        {
            // Auto-create if not assigned in Inspector
            var go = new GameObject("CameraTarget");
            go.transform.SetParent(transform);
            go.transform.localPosition = new Vector3(0, 1.6f, 0);
            _cameraTarget = go.transform;
        }

        _camYaw = transform.eulerAngles.y;
    }

    private void OnEnable()
    {
        EventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        EventBus.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
    }

    private void Update()
    {
        if (!_inputEnabled) return;

        HandleTouchInput();
        ApplyMovement();
        CheckInteractable();
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }

    // ── Movement ─────────────────────────────────────────────────────────

    private void ApplyMovement()
    {
        // Determine move direction relative to camera yaw
        var camForward = Quaternion.Euler(0, _camYaw, 0) * Vector3.forward;
        var camRight   = Quaternion.Euler(0, _camYaw, 0) * Vector3.right;

        var inputDir = (camForward * _joystickDelta.y + camRight * _joystickDelta.x);
        var inputMag = Mathf.Clamp01(inputDir.magnitude);

        if (inputMag > _joystickDeadzone)
        {
            _moveDir   = inputDir.normalized;
            _isSprinting = inputMag > 0.85f;
            _currentSpeed = _isSprinting ? _jogSpeed : _walkSpeed;

            // Smoothly rotate Elias to face movement direction
            var targetRot = Quaternion.LookRotation(_moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
        }
        else
        {
            _currentSpeed = 0f;
        }

        // Gravity
        if (_cc.isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
        _velocity.y += _gravity * Time.deltaTime;

        var move = _moveDir * (_currentSpeed * Time.deltaTime)
                 + Vector3.up * (_velocity.y * Time.deltaTime);

        _cc.Move(move);
    }

    // ── Camera ───────────────────────────────────────────────────────────

    private void UpdateCamera()
    {
        if (_cam == null) return;

        var targetPos    = _cameraTarget.position;
        var rotation     = Quaternion.Euler(_camPitch, _camYaw, 0);
        var desiredPos   = targetPos - rotation * Vector3.forward * _camDistance
                         + Vector3.up * _camHeight;

        // Collision — pull camera in if geometry is in the way
        if (Physics.Linecast(targetPos, desiredPos, out var hit,
            ~LayerMask.GetMask("Player"), QueryTriggerInteraction.Ignore))
        {
            desiredPos = hit.point + hit.normal * 0.2f;
        }

        _cam.transform.position = Vector3.SmoothDamp(
            _cam.transform.position, desiredPos, ref _camVelocity, 0.08f);

        _cam.transform.LookAt(targetPos + Vector3.up * 0.2f);
    }

    // ── Touch input ──────────────────────────────────────────────────────

    private void HandleTouchInput()
    {
#if UNITY_EDITOR
        HandleEditorInput();
        return;
#endif
        foreach (Touch t in Input.touches)
        {
            switch (t.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegan(t);
                    break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnTouchMoved(t);
                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnTouchEnded(t);
                    break;
            }
        }
    }

    private void OnTouchBegan(Touch t)
    {
        bool isLeftSide = t.position.x < Screen.width * 0.45f;

        if (isLeftSide && _moveTouchId == -1)
        {
            _moveTouchId   = t.fingerId;
            _joystickOrigin = t.position;
            _joystickDelta  = Vector2.zero;
        }
        else if (!isLeftSide && _camTouchId == -1)
        {
            _camTouchId  = t.fingerId;
            _camLastPos  = t.position;
        }
    }

    private void OnTouchMoved(Touch t)
    {
        if (t.fingerId == _moveTouchId)
        {
            var raw = (t.position - _joystickOrigin) / _joystickRadius;
            _joystickDelta = Vector2.ClampMagnitude(raw, 1f);
        }
        else if (t.fingerId == _camTouchId)
        {
            var delta    = t.position - _camLastPos;
            _camYaw     += delta.x * _camSensitivity * 0.1f;
            _camPitch    = Mathf.Clamp(
                _camPitch - delta.y * _camSensitivity * 0.1f,
                _camMinPitch, _camMaxPitch);
            _camLastPos  = t.position;
        }
    }

    private void OnTouchEnded(Touch t)
    {
        if (t.fingerId == _moveTouchId)
        {
            _moveTouchId   = -1;
            _joystickDelta = Vector2.zero;
            _moveDir       = Vector3.zero;

            // Check for tap (tiny movement = tap, not drag)
            var rawDelta = (t.position - _joystickOrigin).magnitude;
            if (rawDelta < 15f)
                TryInteract(t.position);
        }
        else if (t.fingerId == _camTouchId)
        {
            _camTouchId = -1;

            // Right-side tap = interact check too
            var rawDelta = t.deltaPosition.magnitude;
            if (rawDelta < 15f)
                TryInteract(t.position);
        }
    }

    // ── Editor input (WASD + mouse for testing in Play mode) ─────────────

    private void HandleEditorInput()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        _joystickDelta = new Vector2(h, v);

        // Right mouse held = rotate camera
        if (Input.GetMouseButton(1))
        {
            _camYaw   += Input.GetAxis("Mouse X") * 3f;
            _camPitch  = Mathf.Clamp(
                _camPitch - Input.GetAxis("Mouse Y") * 3f,
                _camMinPitch, _camMaxPitch);
        }

        // Left mouse click = interact
        if (Input.GetMouseButtonDown(0))
            TryInteract(Input.mousePosition);

        // Crouch toggle
        if (Input.GetKeyDown(KeyCode.C))
            ToggleCrouch();
    }

    // ── Interaction ──────────────────────────────────────────────────────

    /// <summary>
    /// Called every frame — finds the closest interactable in range
    /// and highlights it. The player tapping triggers TryInteract().
    /// </summary>
    private void CheckInteractable()
    {
        IInteractable nearest = null;
        float nearestDist = _interactRange;

        var cols = Physics.OverlapSphere(
            transform.position, _interactRange, _interactLayers);

        foreach (var col in cols)
        {
            if (!col.TryGetComponent<IInteractable>(out var interactable)) continue;
            if (!interactable.CanInteract()) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = interactable;
            }
        }

        // Update highlight
        if (nearest != _highlightedTarget)
        {
            _highlightedTarget?.OnHighlightExit();
            nearest?.OnHighlightEnter();
            _highlightedTarget = nearest;
        }
    }

    private void TryInteract(Vector2 screenPos)
    {
        // If something is already highlighted and in range, interact with it
        if (_highlightedTarget != null)
        {
            _highlightedTarget.OnInteract(gameObject);
            return;
        }

        // Fallback: raycast from tap position into world
        var ray = _cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out var hit, _interactRange * 3f, _interactLayers))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var target)
                && target.CanInteract())
            {
                target.OnInteract(gameObject);
            }
        }
    }

    // ── Crouch ───────────────────────────────────────────────────────────

    private void ToggleCrouch()
    {
        _isCrouching = !_isCrouching;
        _cc.height  = _isCrouching ? 1.0f : 1.8f;
        _cc.center  = Vector3.up * (_cc.height * 0.5f);
    }

    // ── Event handlers ───────────────────────────────────────────────────

    private void OnGameStateChanged(GameStateChangedEvent e)
    {
        _inputEnabled = e.Next == GameState.Playing;

        if (!_inputEnabled)
        {
            _joystickDelta = Vector2.zero;
            _moveDir       = Vector3.zero;
            _moveTouchId   = -1;
            _camTouchId    = -1;
        }
    }

    private void OnDialogueEnded(DialogueEndedEvent e)
    {
        _inputEnabled = true;
    }

    // ── Public API ───────────────────────────────────────────────────────

    public bool  IsMoving    => _currentSpeed > 0.1f;
    public bool  IsSprinting => _isSprinting;
    public bool  IsCrouching => _isCrouching;
    public float CurrentSpeed => _currentSpeed;
}

// ─── Interactable interface ───────────────────────────────────────────────────

/// <summary>
/// Implement this on any GameObject that Elias can interact with:
/// NPCs, clue objects, doors, containers, environmental details.
/// </summary>
public interface IInteractable
{
    /// <summary>Returns true if interaction is currently possible.</summary>
    bool CanInteract();

    /// <summary>Called when Elias enters interaction range — show prompt.</summary>
    void OnHighlightEnter();

    /// <summary>Called when Elias leaves interaction range — hide prompt.</summary>
    void OnHighlightExit();

    /// <summary>Called when the player taps this interactable.</summary>
    void OnInteract(GameObject interactor);
}
