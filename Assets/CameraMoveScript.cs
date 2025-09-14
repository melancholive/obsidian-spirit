using UnityEngine;

public class CameraScrollerScript : MonoBehaviour
{
    [Header("Scroll")]
    public float startSpeed = 2f;      // initial upward speed (world units / s)
    public float accel = 0.15f;        // speed increases over time
    public float maxSpeed = 8f;        // safety clamp

    public static float CurrentSpeed { get; private set; }

    void Start()
    {
        CurrentSpeed = startSpeed;
    }

    void Update()
    {
        // Increase speed over time (your "tightly coupled" difficulty)
        CurrentSpeed = Mathf.Min(maxSpeed, CurrentSpeed + accel * Time.deltaTime);

        // Move camera straight up
        transform.position += Vector3.up * CurrentSpeed * Time.deltaTime;
    }
}
