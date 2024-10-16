using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SharkGame
{
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

        public void UpdatePlaneMaterial()
        {
            if (planeMaterial != null)
            {
                Color color = BgColors[SharkGameManager.Instance.CurrentLevel % BgColors.Count];
                planeMaterial.color = new Color(color.r, color.g, color.b, .05f);
                UnderwaterMaterial.color = new Color(color.r, color.g, color.b, .05f);
            }
        }
    }
}
