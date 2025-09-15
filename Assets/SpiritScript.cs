using UnityEngine;

public class SpiritScript : MonoBehaviour
{
    public LayerMask wallLayer;          // assign your wall layer
    public float wallCheckDistance = 0.5f;
    public Camera cam;
    public LogicScript logic;

    [HideInInspector] public bool isTouchingWall = false;
    [HideInInspector] public bool isOnRightWall = false;

    void Start()
    {
        logic = GameObject.FindGameObjectWithTag("Logic").GetComponent<LogicScript>();
    }

    void Update()
    {
        isTouchingWall = false;

        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        RaycastHit2D hitDown = Physics2D.Raycast(transform.position, Vector2.down, wallCheckDistance, wallLayer);

        Debug.Log($"Raycast Right: {(hitRight.collider != null ? hitRight.collider.gameObject.name : "None")}");
        Debug.Log($"Raycast Left: {(hitLeft.collider != null ? hitLeft.collider.gameObject.name : "None")}");
        Debug.Log($"Raycast Down: {(hitDown.collider != null ? hitDown.collider.gameObject.name : "None")}");

        if (hitRight.collider != null)
        {
            isTouchingWall = true;
            isOnRightWall = true;
        }
        else if (hitLeft.collider != null)
        {
            isTouchingWall = true;
            isOnRightWall = false;
        }
        else if (hitDown.collider != null)
        {
            isTouchingWall = true;
            // isOnRightWall remains unchanged
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        // Remove wall sticking logic to allow projectile motion

        // Debug: visualize rays
        Debug.DrawRay(transform.position, Vector2.right * wallCheckDistance, Color.red);
        Debug.DrawRay(transform.position, Vector2.left * wallCheckDistance, Color.red);
        Debug.DrawRay(transform.position, Vector2.down * wallCheckDistance, Color.blue);

        // Game Over when the ball falls
        if (!cam) cam = Camera.main;
        float bottom = cam.transform.position.y - cam.orthographicSize; 
        if (transform.position.y < bottom)
        {
            logic.gameOver();
        }

    }

    // Game over when collision
    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     logic.gameOver();
    // }
}

