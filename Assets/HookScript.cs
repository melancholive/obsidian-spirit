using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HookScript : MonoBehaviour
{
    // --- Inspector Fields ---
    [Header("Arrow Settings")]
    public Transform arrow;               // assign arrow sprite
    public float sweepSpeed = 250f;       // degrees per second

    [Header("Force Settings")]
    public float minForce = 0.05f;
    public float maxForce = 0.5f;

    [Header("References")]
    public Rigidbody2D rb;                
    public SpiritScript wallCheck;        

    // --- Private State ---
    private Collider2D col;
    private float currentAngle = 0f;
    private bool goingUp = true;
    private bool locked = false;
    private bool charging = false;
    private float chargeForce = 0f;
    private InputAction spaceAction;

    // --- Unity Event Methods ---
    void OnEnable()
    {
        col = GetComponent<Collider2D>();
        spaceAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
        spaceAction.Enable();
    }

    void OnDisable()
    {
        if (spaceAction != null)
            spaceAction.Disable();
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

        // Apply horizontal drag to slow down ball's horizontal speed after launch
        if (!wallCheck.isTouchingWall && Mathf.Abs(rb.linearVelocity.x) > 0.01f)
        {
            float horizontalDrag = 0.5f; // adjust for more/less drag
            rb.linearVelocity = new Vector2(rb.linearVelocity.x * (1f - horizontalDrag * Time.deltaTime), rb.linearVelocity.y);
        }
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
        if (!locked)
        {
            currentAngle += (goingUp ? sweepSpeed : -sweepSpeed) * Time.deltaTime;
            if (currentAngle >= 360f) currentAngle -= 360f;
            if (currentAngle < 0f) currentAngle += 360f;
        }

        float angleRad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * orbitRadius;
        arrow.position = transform.position + offset;
        arrow.rotation = Quaternion.Euler(0, 0, currentAngle);
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
            chargeForce += Time.deltaTime * (2.5f / 3f); // even slower charge rate
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

