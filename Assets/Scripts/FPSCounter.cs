using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    public TMP_Text fpsText;
    public float updateInterval = 0.5f;

    private float accum = 0f;
    private int frames = 0;
    private float timeleft;

    void Start()
    {
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeleft <= 0.0)
        {
            float fps = accum / frames;
            fpsText.text = $"FPS: {Mathf.RoundToInt(fps)}";
            timeleft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }
}
