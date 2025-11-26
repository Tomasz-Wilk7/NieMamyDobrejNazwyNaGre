using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private InputActionReference move;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float collisionBuffer = 0.05f;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField] private InputActionReference dash; // optional inspector wiring
    [SerializeField] private Camera fallbackCamera;
    [SerializeField] private bool debugDraw;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;

    private Vector2 _moveDirection;

    private PlayerInput _playerInput;
    private InputAction _dashAction;
    private Collider2D[] _selfColliders;
    private readonly RaycastHit2D[] _dashHits = new RaycastHit2D[8];

    // Dash runtime state
    private bool _isDashing;
    private float _dashTimer;
    private float _cooldownTimer;
    private Vector2 _dashStart;
    private Vector2 _dashEnd;
    private Vector2 _savedVelocity;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        // Prefer PlayerInput's action map if available
        if (_playerInput != null)
        {
            try
            {
                var candidate = _playerInput.actions["Dash"];
                if (candidate != null)
                {
                    _dashAction = candidate;
                }
            }
            catch { /* ignore if not present */ }
        }

        // If a serialized InputActionReference was provided, prefer that
        if (dash != null && dash.action != null)
        {
            _dashAction = dash.action;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on PlayerController2D GameObject.");
        }

        if (_selfColliders == null || _selfColliders.Length == 0)
        {
            _selfColliders = GetComponents<Collider2D>();
        }

        if (_dashAction == null)
        {
            Debug.LogWarning("Dash action not wired. Provide a PlayerInput with a 'Dash' action or assign an InputActionReference in the inspector.");
        }
    }

    private void OnEnable()
    {
        if (_dashAction != null)
        {
            _dashAction.performed += OnDash;
            _dashAction.Enable();
        }
    }

    private void OnDisable()
    {
        if (_dashAction != null)
        {
            _dashAction.performed -= OnDash;
            _dashAction.Disable();
        }
    }


    private void OnDash(InputAction.CallbackContext context)    
    {
        Debug.Log("Dash input received");
        // Only respond to performed and when not already dashing or on cooldown
        if (!context.performed) return;
        if (_isDashing) return;
        if (_cooldownTimer > 0f) return;
        if (rb == null) return;

        // Determine dash direction: prefer mouse -> fallback to movement input
        Vector2 dir = Vector2.zero;
        Camera cam = Camera.main ?? fallbackCamera;

        if (Mouse.current != null && cam != null)
        {
            Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPosition = cam.ScreenToWorldPoint(mouseScreenPosition);
            mouseWorldPosition.z = 0f;
            dir = (mouseWorldPosition - rb.transform.position).normalized;
        }

        if (dir.sqrMagnitude < 0.0001f)
        {
            // fallback to current movement input
            if (_moveDirection.sqrMagnitude > 0.0001f)
            {
                dir = _moveDirection.normalized;
            }
        }

        if (dir.sqrMagnitude < 0.0001f)
        {
            // no valid direction -> cancel dash
            if (debugDraw) Debug.Log("Dash cancelled: no direction available.");
            return;
        }

        Vector2 start = rb.position;
        float intendedDistance = dashDistance;

        // Raycast to detect obstacles and clamp the dash endpoint
        Vector2 end;
        if (TryGetDashHit(start, dir, intendedDistance, out RaycastHit2D hit))
        {
            float stopDist = Mathf.Max(0f, hit.distance - collisionBuffer);
            end = start + dir * stopDist;
            if (debugDraw) Debug.DrawLine(start, end, Color.red, 1f);
        }
        else
        {
            end = start + dir * intendedDistance;
            if (debugDraw) Debug.DrawLine(start, end, Color.green, 1f);
        }

        // Start dash
        _isDashing = true;
        _dashTimer = dashDuration;
        _dashStart = start;
        _dashEnd = end;
        _savedVelocity = rb.linearVelocity;
        rb.linearVelocity = Vector2.zero; // take control of movement during dash

        if (debugDraw)
        {
            Debug.Log($"Dash started: start={_dashStart} end={_dashEnd} duration={dashDuration}");
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        if (move != null && move.action != null)
        {
            _moveDirection = move.action.ReadValue<Vector2>();
        }

        // countdown cooldown timer
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer < 0f) _cooldownTimer = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        if (_isDashing)
        {
            // progress between start and end based on remaining dash timer
            float elapsed = dashDuration - _dashTimer;
            float progress = dashDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / dashDuration);
            Vector2 next = Vector2.Lerp(_dashStart, _dashEnd, progress);
            rb.MovePosition(next);

            // decrement timer
            _dashTimer -= Time.fixedDeltaTime;

            if (_dashTimer <= 0f)
            {
                // finish dash
                rb.MovePosition(_dashEnd);
                _isDashing = false;
                _cooldownTimer = dashCooldown;

                // restore saved linear velocity after dash
                rb.linearVelocity = _savedVelocity;

                if (debugDraw)
                {
                    Debug.Log("Dash finished");
                }
            }

            return; // skip regular movement while dashing
        }

        // Normal movement when not dashing
        Vector2 targetVelocity = new Vector2(
            _moveDirection.x * moveSpeed,
            _moveDirection.y * moveSpeed
        );

        // Use linearVelocity for Rigidbody2D
        rb.linearVelocity = targetVelocity;
    }

    private bool TryGetDashHit(Vector2 origin, Vector2 direction, float maxDistance, out RaycastHit2D hit)
    {
        hit = default;
        if (maxDistance <= 0f)
        {
            return false;
        }

        LayerMask mask = obstacleMask;
        if (mask == 0)
        {
            mask = Physics2D.DefaultRaycastLayers;
        }

        var filter = new ContactFilter2D
        {
            useLayerMask = true,
            useTriggers = Physics2D.queriesHitTriggers
        };
        filter.SetLayerMask(mask);

        int hitCount = Physics2D.Raycast(origin, direction, filter, _dashHits, maxDistance);
        for (int i = 0; i < hitCount; i++)
        {
            var candidate = _dashHits[i];
            if (candidate.collider == null)
            {
                continue;
            }

            if (IsSelfCollider(candidate.collider))
            {
                continue;
            }

            hit = candidate;
            return true;
        }

        return false;
    }

    private bool IsSelfCollider(Collider2D candidate)
    {
        if (candidate == null || _selfColliders == null)
        {
            return false;
        }

        for (int i = 0; i < _selfColliders.Length; i++)
        {
            if (_selfColliders[i] == null)
            {
                continue;
            }

            if (_selfColliders[i] == candidate)
            {
                return true;
            }
        }

        return false;
    }
}
