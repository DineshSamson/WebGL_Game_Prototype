using UnityEngine;
using TMPro;

public class FPSCalculater : MonoBehaviour
{
    [SerializeField] TMP_Text lable;
    [SerializeField, Min(0.1f)] float updateInterval = 0.5f;

    int frames;
    float timer;
    float minFrameTime = float.MaxValue;
    float maxFrameTime;

    private void Awake()
    {
        if(lable == null)
        {
            lable = GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        frames++;
        timer += dt;

        if(dt < minFrameTime)
            minFrameTime = dt;  
        if(dt > maxFrameTime)
            maxFrameTime = dt;
        if (timer < updateInterval)
            return;

        float avgFps = frames / timer;

        lable.SetText("{0:0} FPS", avgFps);

        frames = 0;
        timer = 0;
        minFrameTime = float.MaxValue;
        maxFrameTime = 0f;

    }
}
