using UnityEngine;

public class TriFlyerSpawnerScript : MonoBehaviour
{
    public GameObject triFlyerPrefab;
    public Camera cam;
    public float spawnGapY = 6f;     // vertical spacing between spawns (world units)
    public Vector2 xRange = new Vector2(-2.5f, 2.5f); // spawn X range between walls
    public int prewarmCount = 2;     // spawn a few off-screen at start
    public float cullMargin = 3f;    // destroy when far below bottom

    private float nextSpawnY;

    void Start()
    {
        if (!cam) cam = Camera.main;
        nextSpawnY = cam.transform.position.y + cam.orthographicSize + spawnGapY;

        // Prewarm a few above the screen
        for (int i = 0; i < prewarmCount; i++)
            SpawnOne();
    }

    void Update()
    {
        float top = cam.transform.position.y + cam.orthographicSize;
        float bottom = cam.transform.position.y - cam.orthographicSize;

        // Keep spawning as the camera climbs
        while (nextSpawnY < top + spawnGapY * 1.5f)
            SpawnOne();

        // Cull old ones
        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (e.transform.position.y < bottom - cullMargin)
                Destroy(e);
        }
    }

    void SpawnOne()
    {
        float x = Random.Range(xRange.x, xRange.y);
        Vector3 pos = new Vector3(x, nextSpawnY, 0f);
        var go = Instantiate(triFlyerPrefab, pos, Quaternion.identity);
        go.tag = "Enemy";
        nextSpawnY += spawnGapY;
    }
}
