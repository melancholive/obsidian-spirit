using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HookScript : MonoBehaviour
{
    // --- Inspector Fields ---

    public SpiritScript spirit;      // drag your player here (or auto-find)
    
    [Header("Arrow Settings")]
    public Transform arrow;               // assign arrow sprite
    public float sweepSpeed = 250f;       // degrees per second
    public float orbitRadius = 0.5f;
    public float edgeMarginDeg = 8f; // keep away from hard edges
    public float spriteAngleOffset = 0f; // if your arrow sprite faces up, set -90
    bool? lastOnRight = null;

    [Header("Force Settings")]
    public float minForce = 20f;
    public float maxForce = 80f;

    [Header("References")]
    public Rigidbody2D rb;                
    public SpiritScript wallCheck;        

    // --- Private State ---
    private Collider2D col;
    private float currentAngle = 0f;
    private bool prevTouching = false;
    private bool goingUp = true;
    private bool locked = false;
    private bool charging = false;
    private float chargeForce = 0f;
    private InputAction spaceAction;
    public AudioSource sfxSource;
    public AudioClip  jumpSfx;

    // --- Unity Event Methods ---
    void OnEnable()
    {
        col = GetComponent<Collider2D>();
        spaceAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
        spaceAction.canceled  += _ => {              // 松开那一刻（真正发射）
            if (charging){
                Launch();
                sfxSource?.PlayOneShot(jumpSfx);     // ← 在发射瞬间播放
                locked=false; charging=false;
            }
        };
        spaceAction.Enable();
    }

    void OnDisable()
    {
        if (spaceAction != null)
            spaceAction.Disable();
    }

    void Awake()
    {
        if (!spirit) spirit = GetComponent<SpiritScript>(); // same GameObject
    }

    void Update()
    {
        Debug.Log($"Update: wallCheck.isTouchingWall = {wallCheck.isTouchingWall}");
        if (arrow == null)
        {
            Debug.LogError("Arrow reference is not assigned!");
            return;
        }
        if (wallCheck.isTouchingWall)
        {
            if (!prevTouching)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                // rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            Debug.Log("Arrow should be visible and rotating.");
            rb.gravityScale = 0f;
            RotateArrow();
            HandleInput();
        }
        else
        {
            rb.gravityScale = 1f;
            Debug.Log("Arrow hidden.");
            arrow.gameObject.SetActive(false);
        }

        prevTouching = wallCheck.isTouchingWall;

        // Apply horizontal drag to slow down ball's horizontal speed after launch
        if (!wallCheck.isTouchingWall && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
        {
            float horizontalDrag = 0.5f; // adjust for more/less drag
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * (1f - horizontalDrag * Time.deltaTime), rb.linearVelocity.y);
        }

        //Reset arrow to mid when touching the wall
        if (wallCheck.isTouchingWall)
        {
            bool nowRight = wallCheck.isOnRightWall;
            if (lastOnRight.HasValue && nowRight != lastOnRight.Value)
            {
                // jump to the middle of the new range to avoid snapping on the edge
                currentAngle = nowRight ? 135f : 45f; // middles of [90,180] and [0,90]
                goingUp = true;
            }
            lastOnRight = nowRight;
        }
        else lastOnRight = null;
    }

    // --- Public Methods ---
    // (none)

    // --- Private Helper Methods ---
    void EnableCollider()
    {
        if (col != null)
            col.enabled = true;
    }

    void RotateArrow()
    {
        Debug.Log("RotateArrow called");
        arrow.gameObject.SetActive(true);
        float orbitRadius = 0.5f; // distance from ball center

        // 1) Decide allowed angle range
        float minDeg, maxDeg;
        if (wallCheck.isTouchingWall)
        {
            if (wallCheck.isOnRightWall)
            {
                // touching RIGHT wall: aim only to the upper-left quadrant [90�,180�]
                minDeg = 90f + edgeMarginDeg;
                maxDeg = 180f - edgeMarginDeg;
            }
            else
            {
                // touching LEFT wall: aim only to the upper-right quadrant [0�,90�]
                minDeg = 0f + edgeMarginDeg;
                maxDeg = 90f - edgeMarginDeg;
            }
        }
        else
        {
            // mid-air fallback: any upper-half angle
            minDeg = 30f;
            maxDeg = 150f;
        }

        // 2) Sweep inside [min,max] and bounce on the bounds
        if (!locked)
        {
            float delta = sweepSpeed * Time.deltaTime;
            currentAngle += goingUp ? delta : -delta;

            if (currentAngle > maxDeg) { currentAngle = maxDeg; goingUp = false; }
            if (currentAngle < minDeg) { currentAngle = minDeg; goingUp = true; }
        }

        // 3) Place arrow on an orbit and rotate to face currentAngle
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        arrow.position = transform.position + offset;
        arrow.rotation = Quaternion.Euler(0f, 0f, currentAngle + spriteAngleOffset);
    }

    void HandleInput()
    {
        if (spaceAction.triggered && !charging)
        {
            locked = true;
            charging = true;
            chargeForce = minForce;
        }

        if (spaceAction.ReadValue<float>() > 0 && charging)
        {
            chargeForce += Time.deltaTime * (5.5f / 3f); // even slower charge rate
            chargeForce = Mathf.Clamp(chargeForce, minForce, maxForce);
            // Stretch the arrow's x scale based on chargeForce
            float stretch = 0.17f + (chargeForce - minForce) / (maxForce - minForce) * 0.5f; // 0.5 to 1.0
            stretch = Mathf.Clamp(stretch, 0.17f, 1.0f);
            arrow.localScale = new Vector3(stretch, arrow.localScale.y, arrow.localScale.z);
        }

        if (!spaceAction.ReadValue<float>().Equals(1f) && charging)
        {
            Launch();
            locked = false;
            charging = false;
        }
    }

    void Launch()
    {
        Vector3 pos = transform.position;
        // Unpin so the impulse can move the body
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        rb.gravityScale = 1f;
        float angle = arrow.rotation.eulerAngles.z;
        float angleRad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        Debug.Log($"Launch angle: {angle} degrees, direction vector: {dir}, chargeForce: {chargeForce}");
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(dir.normalized * chargeForce, ForceMode2D.Impulse);
    // Reset arrow scale after launch
    arrow.localScale = new Vector3(0.17f, arrow.localScale.y, arrow.localScale.z);
    }
}

