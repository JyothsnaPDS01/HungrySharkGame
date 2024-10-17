using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
    public TextMeshProUGUI fpsText; // Optional: A UI Text element to display FPS

    private float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        if (fpsText != null)
        {
            fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log($"FPS: {Mathf.Ceil(fps)}");
#endif
        }
    }
}