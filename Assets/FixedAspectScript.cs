using UnityEngine;

public class FixedAspectScript : MonoBehaviour
{
    public Vector2 targetAspect = new Vector2(9, 16);
    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateViewport();
    }

    // Update is called once per frame
    void Update()
    {
        if (Screen.width != lastW || Screen.height != lastH)
        {
            UpdateViewport();
        }
    }

    int lastW, lastH;

    void UpdateViewport()
    {
        lastW = Screen.width;
        lastH = Screen.height;

        float target = targetAspect.x / targetAspect.y;   // 9/16
        float window = (float)Screen.width / Screen.height;

        if (window > target)
        {
            // 屏幕太宽 → 左右加黑边
            float w = target / window;
            cam.rect = new Rect((1f - w) * 0.5f, 0, w, 1f);
        }
        else
        {
            // 屏幕太高 → 上下加黑边
            float h = window / target;
            cam.rect = new Rect(0, (1f - h) * 0.5f, 1, h);
        }
    }
}
