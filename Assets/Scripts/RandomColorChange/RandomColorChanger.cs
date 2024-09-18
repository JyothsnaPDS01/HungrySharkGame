using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomColorChanger : MonoBehaviour
{
    #region Private Variables
    [Header("BG transition Duration Parameters")]
    [SerializeField] private float interval = 2.0f; // Time interval in seconds
    [SerializeField] private float transitionDuration = 1.0f; // Duration of color transition in seconds
    [Header("Plane Material")]
    [SerializeField] private Material planeMaterial; // Reference to the plane's material
    [Header("UnderWater Material")]
    [SerializeField] private Material UnderwaterMaterial;
    [Header("BG Colors")]
    [SerializeField] private List<Color> BgColors = new List<Color>();

    #endregion

    void Start()
    {
        StartCoroutine(ChangeColorOverTime()); // Start the coroutine
    }

    IEnumerator ChangeColorOverTime()
    {
        while (true)
        {
            // Generate a random color
            Color newColor = BgColors[Random.Range(0, BgColors.Count)];

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
            planeMaterial.color = new Color(color.r, color.g, color.b, .05f);
            UnderwaterMaterial.color = new Color(color.r, color.g, color.b, .05f);
        }
    }
}
