using UnityEngine;
using UnityEngine.InputSystem;

public class CameraScrollerScript : MonoBehaviour
{
    [Header("Scroll")]
    public float startSpeed = 2f;      // initial upward speed (world units / s)
    public float accel = 0.15f;        // speed increases over time
    public float maxSpeed = 8f;        // safety clamp
    private bool rising = false;
    private InputAction space;  

    public static float CurrentSpeed { get; private set; }

    void Awake()
    {
        space = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/space");
        space.Enable();
    }

    void OnDisable()
    {
        space?.Disable();
    }


    void Start()
    {
        CurrentSpeed = startSpeed;
    }

    void Update()
    {
        if (space != null && space.triggered)
            rising = true;
        if (!rising) return;
        // Increase speed over time (your "tightly coupled" difficulty)
        CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + accel * Time.deltaTime);

        // Move camera straight up
        transform.position += Vector3.up * CurrentSpeed * Time.deltaTime;
    }
}
