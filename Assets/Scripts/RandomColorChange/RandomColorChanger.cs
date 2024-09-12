using UnityEngine;
using System.Collections;

public class RandomColorChanger : MonoBehaviour
{
    public float interval = 2.0f; // Time interval in seconds
    public float transitionDuration = 1.0f; // Duration of color transition in seconds
    public Material planeMaterial; // Reference to the plane's material

    void Start()
    {
        StartCoroutine(ChangeColorOverTime()); // Start the coroutine
    }

    IEnumerator ChangeColorOverTime()
    {
        while (true)
        {
            // Generate a random color
            Color newColor = new Color(Random.value, Random.value, Random.value);

            // Smoothly transition to the new color
            yield return StartCoroutine(TransitionColor(newColor));

            // Wait for the specified interval before changing the color again
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator TransitionColor(Color targetColor)
    {
        float elapsedTime = 0f;
        Color initialColor = WaterSettings.sunExtinction;

        while (elapsedTime < transitionDuration)
        {
            // Interpolate between the initial color and the target color
            Color lerpedColor = Color.Lerp(initialColor, targetColor, elapsedTime / transitionDuration);
            WaterSettings.SetSunExtinctionColor(lerpedColor);
            UpdatePlaneMaterial(lerpedColor); // Update the plane material with the current lerped color

            elapsedTime += Time.deltaTime;
            yield return null; // Wait until the next frame
        }

        // Ensure the final color is set exactly
        WaterSettings.SetSunExtinctionColor(targetColor);
        UpdatePlaneMaterial(targetColor); // Update the plane material with the final color
    }

    void UpdatePlaneMaterial(Color color)
    {
        if (planeMaterial != null)
        {
            planeMaterial.color = color;
        }
    }
}
