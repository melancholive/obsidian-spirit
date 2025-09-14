using System.Collections.Generic;
using UnityEngine;

/// Spawns wall spikes as the camera climbs upward. Walls do NOT move.
/// Place two anchors at the inner edges of the left/right walls so we have X positions to spawn at.
public class SpikesSpawnerScript : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject leftSpikePrefab;     // spike that faces right (mounted on LEFT wall)
    public GameObject rightSpikePrefab;    // spike that faces left (mounted on RIGHT wall)

    [Header("Camera & Anchors")]
    public Camera cam;                     // if null, uses Camera.main
    public Transform leftAnchor;           // empty transform placed at the inner face of left wall
    public Transform rightAnchor;          // empty transform placed at the inner face of right wall
    public float inset = 0.15f;            // small inward offset from the wall (world units)

    [Header("Spawn Logic")]
    public Vector2 gapY = new Vector2(4f, 7f); // vertical spacing between rows [min,max]
    public int prewarmCount = 2;               // spawn a few rows above the top at start
    public float cullMargin = 3f;              // destroy spikes when this far below bottom
    [Range(0f, 1f)] public float pairProbability = 0.25f; // chance to spawn both sides in a row
    public bool alternateSides = false;        // if true: left, right, left, right...

    // runtime
    private readonly List<Transform> spawned = new List<Transform>();
    private float nextSpawnY;
    private bool nextIsRight = false;          // used when alternateSides = true

    void Start()
    {
        if (!cam) cam = Camera.main;
        if (!cam || !leftAnchor || !rightAnchor)
        {
            Debug.LogError("[SpikesSpawner] Camera and both anchors must be assigned.");
            enabled = false;
            return;
        }

        // first target Y just above the top edge
        nextSpawnY = cam.transform.position.y + cam.orthographicSize + gapY.x;

        // prewarm a few rows so the screen starts filled
        for (int i = 0; i < prewarmCount; i++)
            SpawnOneRow();
    }

    void Update()
    {
        float top = cam.transform.position.y + cam.orthographicSize;
        float bottom = cam.transform.position.y - cam.orthographicSize;

        // keep spawning while the next row would be visible soon
        while (nextSpawnY < top + gapY.y)
            SpawnOneRow();

        // cull spikes far below the screen
        for (int i = spawned.Count - 1; i >= 0; --i)
        {
            var t = spawned[i];
            if (t == null) { spawned.RemoveAt(i); continue; }
            if (t.position.y < bottom - cullMargin)
            {
                Destroy(t.gameObject);
                spawned.RemoveAt(i);
            }
        }
    }

    private void SpawnOneRow()
    {
        bool spawnLeft = false, spawnRight = false;

        if (alternateSides)
        {
            spawnLeft = !nextIsRight;
            spawnRight = nextIsRight;
            nextIsRight = !nextIsRight;
        }
        else
        {
            // roll once: both sides, otherwise choose one side at random
            if (Random.value < pairProbability) { spawnLeft = spawnRight = true; }
            else if (Random.value < 0.5f) spawnLeft = true;
            else spawnRight = true;
        }

        float y = nextSpawnY;

        if (spawnLeft && leftSpikePrefab && leftAnchor)
        {
            Vector3 pos = new Vector3(leftAnchor.position.x + inset, y, leftAnchor.position.z);
            // keep the prefab's rotation (make sure it already points to the right)
            var t = Instantiate(leftSpikePrefab, pos, leftSpikePrefab.transform.rotation).transform;
            spawned.Add(t);
        }

        if (spawnRight && rightSpikePrefab && rightAnchor)
        {
            Vector3 pos = new Vector3(rightAnchor.position.x - inset, y, rightAnchor.position.z);
            // keep the prefab's rotation (make sure it already points to the left)
            var t = Instantiate(rightSpikePrefab, pos, rightSpikePrefab.transform.rotation).transform;
            spawned.Add(t);
        }

        // advance next row by a random vertical gap
        nextSpawnY += Random.Range(gapY.x, gapY.y);
    }

    // Visualize anchors in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (leftAnchor) Gizmos.DrawLine(leftAnchor.position + Vector3.down * 50f, leftAnchor.position + Vector3.up * 50f);
        Gizmos.color = Color.cyan;
        if (rightAnchor) Gizmos.DrawLine(rightAnchor.position + Vector3.down * 50f, rightAnchor.position + Vector3.up * 50f);
    }
}
