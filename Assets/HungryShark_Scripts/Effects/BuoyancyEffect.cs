using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SharkGame
{
    public class BuoyancyEffect : MonoBehaviour
    {
        public float waterLevel = 0f;            // The Y position of the water surface
        public float buoyancyStrength = 10f;     // The strength of the buoyant force
        public float objectDensity = 1f;         // Density of the object (relative to water)
        public float damping = 0.1f;             // Damping factor to reduce oscillation

        private Rigidbody rb;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            // Calculate how much of the object is submerged
            float submergedRatio = Mathf.Clamp01((waterLevel - transform.position.y) / transform.localScale.y);

            // Apply the buoyant force proportional to the submerged volume
            Vector3 buoyantForce = Vector3.up * buoyancyStrength * submergedRatio * objectDensity;

            // Apply the buoyant force to the object's rigidbody
            rb.AddForce(buoyantForce, ForceMode.Acceleration);

            // Apply damping to reduce the oscillation effect
            rb.velocity *= (1f - damping * Time.fixedDeltaTime);
        }

    }
}
