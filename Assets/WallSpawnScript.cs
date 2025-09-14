using UnityEngine;
using System.Collections.Generic;

public class WallSpawnScript : MonoBehaviour
{
    [Header("Prefab & Camera")]
    public GameObject wallPrefab;         // your wall piece
    public Camera cam;                    // if null, uses Camera.main

    [Header("Tiling & Cleanup")]
    public int piecesAbove = 2;           // how many pieces kept above the top edge
    public float cullMargin = 2f;         // destroy pieces this far below the bottom edge
    public float seamOverlap = 0.01f;     // small overlap to hide seams (world units)

    [Header("Height")]
    public bool useRendererBounds = true; // measure height from Renderer.bounds
    public float wallHeight = 1f;         // manual fallback if no bounds found

    // runtime
    private float halfH;
    private float nextSpawnY;             // world Y for the next piece
    private readonly Queue<Transform> active = new Queue<Transform>();

    void Start()
    {
        if (!cam) cam = Camera.main;
        if (!cam || !wallPrefab)
        {
            Debug.LogError("[WallSpawnScript] Missing camera or wall prefab.");
            enabled = false;
            return;
        }

        // Spawn the first piece at this spawner's position
        var first = Instantiate(wallPrefab, transform.position, transform.rotation).transform;
        active.Enqueue(first);

        // Measure world-space height from the instance (includes scaling)
        float h = wallHeight;
        if (useRendererBounds)
        {
            var rend = first.GetComponentInChildren<Renderer>();
            if (rend) h = rend.bounds.size.y;
        }
        else
        {
            var col = first.GetComponentInChildren<Collider2D>();
            if (col) h = col.bounds.size.y;
        }
        wallHeight = Mathf.Max(0.001f, h);
        halfH = wallHeight * 0.5f;

        // Next piece goes exactly one height above (minus tiny overlap)
        nextSpawnY = first.position.y + wallHeight - seamOverlap;

        // Initial fill so the top of the screen is already covered
        PrewarmFill();
    }

    void Update()
    {
        if (!cam) return;

        float topEdge = cam.transform.position.y + cam.orthographicSize;
        float bottomEdge = cam.transform.position.y - cam.orthographicSize;

        // Always keep enough pieces above the camera's top edge
        while (nextSpawnY - halfH < topEdge + piecesAbove * wallHeight)
            SpawnNext();

        // Reclaim pieces that are far below the bottom edge
        while (active.Count > 0 &&
               active.Peek().position.y + halfH < bottomEdge - cullMargin)
        {
            var t = active.Dequeue();
            if (t) Destroy(t.gameObject);
        }
    }

    private void SpawnNext()
    {
        Vector3 pos = new Vector3(transform.position.x, nextSpawnY, transform.position.z);
        var piece = Instantiate(wallPrefab, pos, transform.rotation).transform;
        active.Enqueue(piece);

        // Advance for the next piece; subtract a tiny overlap to avoid visual gaps
        nextSpawnY += (wallHeight - seamOverlap);
    }

    private void PrewarmFill()
    {
        float topEdge = cam.transform.position.y + cam.orthographicSize;
        while (nextSpawnY - halfH < topEdge + piecesAbove * wallHeight)
            SpawnNext();
    }
}
