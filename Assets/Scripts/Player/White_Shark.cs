using System.Collections;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class White_Shark : MonoBehaviour
    {
        #region Private Variables
        [SerializeField] private float _sharkSpeed = 2f;
        [SerializeField] private float rotationSpeed = 5f; // Speed of rotation
        [SerializeField] private Animator _sharkAnimator;
        [SerializeField] private Rigidbody _sharkRB;
        [SerializeField] private Transform _sharkMouthPosition;
        [SerializeField] private Transform _sharkHeadPosition;
        [SerializeField] private GameObject _bloodEffectObject;

        [SerializeField] private Color _fogColor;
        [Header("VFX Effects")]
        [SerializeField] private GameObject _waterSplashEffect; // Reference to the splash effect
        [SerializeField] private GameObject _waterExplosionEffect;

        private bool isSplashing = false; // Flag to track if splashing


        private Quaternion targetRotation;  // The target rotation based on input
        private bool isMoving;              // Whether the shark is currently moving
        private bool isTransitioning = false; // To manage transition state
        private bool transitionStarted = false;

        private SharkGameDataModel.SharkDirection _currentSharkDirection;
        private SharkGameDataModel.SharkDirection _previousSharkDirection;
        private bool initialMovementCompleted = false;  // Track if initial movement is completed
        private float spawnCooldown = 0.5f; // Time interval between consecutive spawns
        private float lastSpawnTime = 0f;  // Time of last fish spawn

        private Vector3 transitionTargetPosition; // Target position for transition
        private Quaternion transitionTargetRotation = Quaternion.Euler(0, -90, 0); // Target rotation for transition
        private float transitionDuration = 1f; // Duration of the transition

        private float horizontalInput;
        private float verticalInput;
        #endregion

        #region MonoBehaviour Methods
        void Start()
        {
            // Start the initial shark movement sequence
            StartCoroutine(InitialSharkMovement());

            // Reset Rigidbody velocities to prevent unintended movement
            _sharkRB.velocity = Vector3.zero;
            _sharkRB.angularVelocity = Vector3.zero;

            // Ensure Rigidbody is not influenced by gravity
            _sharkRB.useGravity = false;

            // Optional: Freeze unnecessary axes if needed
            _sharkRB.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

        [SerializeField] private float smoothTime = 0.3f; // Duration for smoothing transition
        [SerializeField] private float maxSpeed = 10f; // Maximum speed
        private Vector3 velocity = Vector3.zero; // Current velocity
        [SerializeField] private float smoothSpeed = 2f; // Speed of smoothing
        private bool isParabolicJumping = false; // Flag to indicate if a parabolic jump is in progress


        private bool transitionCompleted = false; // Flag to track if the transition is completed
        void FixedUpdate()
        {
            if (initialMovementCompleted)
            {
                // Detect all input
                horizontalInput = Input.GetAxis("Horizontal");
                verticalInput = Input.GetAxis("Vertical");

                // Determine if "up" input is given
                bool upKeyPressed = verticalInput > 0 && Mathf.Approximately(horizontalInput, 0); // Only "up" input, no horizontal input
                bool noInput = Mathf.Approximately(horizontalInput, 0) && Mathf.Approximately(verticalInput, 0); // No input is given


                // If no input is given, move the shark forward by a small distance
                if (noInput)
                {
                    Debug.LogError("No Input detected. Moving shark forward.");
                    MoveSharkForward();
                }

                // Debugging input and position
                Debug.Log($"Horizontal Input: {horizontalInput}, Vertical Input: {verticalInput}");
                Debug.Log($"Up Key Pressed: {upKeyPressed}, No Input: {noInput}");
                Debug.Log($"Y Position: {_sharkRB.position.y}, Transitioning: {isTransitioning}, Transition Started: {transitionStarted}, Transition Completed: {transitionCompleted}");

                // Check if parabolic jump is in progress
                if (isParabolicJumping)
                {
                    Debug.Log("Parabolic jump in progress. Skipping input handling.");
                    return; // Skip input handling during the parabolic jump
                }

                // Check if transition is in progress
                if (isTransitioning)
                {
                    Debug.Log("Transition is in progress. Skipping input handling.");
                    return; // If transitioning, do nothing and let the coroutine handle the movement
                }


                const float smallThreshold = 0.001f; // Adjusted to accommodate fluctuations
                const float yTargetValue = 0.335f; // Target Y value for surface interactions

                // Check if the shark's Y position is close to the target Y value
                bool isCloseToSurface = Mathf.Abs(_sharkRB.position.y - yTargetValue) < smallThreshold || Mathf.Approximately(_sharkRB.position.y, yTargetValue);

                // Check if we are in the transition range
                bool isInTransitionRange = _sharkRB.position.y >= -1f && _sharkRB.position.y <= 0f;

                if (isCloseToSurface && upKeyPressed)
                {
                    Debug.LogError("Starting parabolic jump.");
                    isParabolicJumping = true; // Set the flag to indicate a parabolic jump is in progress
                    StartCoroutine(HandleParabolicJump());
                    return; // Exit to prevent further handling until jump is complete
                }

                // Handle automatic movement only if the shark is not on the surface
                if (!isCloseToSurface && _sharkRB.position.y > -1f)
                {
                    if (isInTransitionRange)
                    {
                        Debug.Log("Condition met for transition range");

                        // Start the smooth transition if no input or "up" key is pressed and within Y range
                        if ((noInput || upKeyPressed) && !transitionCompleted)
                        {
                            if (!transitionStarted)
                            {
                                Debug.Log("Starting smooth transition");
                                StartSmoothTransition();
                            }
                        }
                    }
                }
                else
                {
                    // Reset transition flags if outside of the transition range
                    transitionCompleted = false; // Allow transition to start again if conditions are met in the future
                }

                // Handle input and movement after transition is complete
                if (transitionCompleted || !isTransitioning)
                {
                    Debug.Log("Handling movement and rotation after transition.");

                    // Handle movement and rotation based on input
                    HandleMovement();
                    HandleRotation();
                   
                }

               
            }
        }


        // Method to move the shark forward
        void MoveSharkForward()
        {
            float forwardSpeed = 30f; // Adjust this value to control the forward movement speed
            Vector3 forwardMovement = transform.forward * forwardSpeed * Time.deltaTime;
            _sharkRB.MovePosition(_sharkRB.position + forwardMovement);
        }

        // Assuming rotationSpeed is defined somewhere in your class
        IEnumerator RotateToTargetRotation(Quaternion targetRotation)
        {
            while (Quaternion.Angle(_sharkRB.transform.rotation, targetRotation) > 0.01f)
            {
                _sharkRB.transform.rotation = Quaternion.RotateTowards(
                    _sharkRB.transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.fixedDeltaTime
                );
                yield return null; // Wait for the next frame
            }
            // Ensure the final rotation is set
            _sharkRB.transform.rotation = targetRotation;
        }

        private IEnumerator HandleParabolicJump()
        {
            float elapsedTime = 0f;
            float duration = 1f; // Duration for the parabolic jump

            Vector3 initialPosition = _sharkRB.position;
            Quaternion initialRotation = _sharkRB.rotation; // Save the initial rotation

            // Define the peak height of the parabolic jump
            float peakHeight = 3f; // Adjust this value to control the height of the jump

            // Define the target position for the end of the jump
            Vector3 targetPosition = new Vector3(_sharkRB.position.x, 0.33f, _sharkRB.position.z); // The landing point at water level

            // Define the target rotation for the end of the jump
            Quaternion targetRotation = Quaternion.Euler(-90, 0, -180);

            while (elapsedTime < duration)
            {
                // Normalize elapsed time to a value between 0 and 1
                float t = elapsedTime / duration;

                // Calculate the parabolic Y position using the quadratic equation
                float yPosition = Mathf.Lerp(initialPosition.y, targetPosition.y, t) + (4 * peakHeight * t * (1 - t));

                // Combine the new Y position with linearly interpolated X and Z positions
                Vector3 newPosition = new Vector3(
                    Mathf.Lerp(initialPosition.x, targetPosition.x, t),
                    yPosition,
                    Mathf.Lerp(initialPosition.z, targetPosition.z, t)
                );

                // Smoothly rotate the shark
                Quaternion newRotation = Quaternion.Slerp(initialRotation, targetRotation, t);

                // Move and rotate the shark smoothly to the new position and rotation
                _sharkRB.MovePosition(newPosition);
                _sharkRB.MoveRotation(newRotation);

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Ensure the shark reaches the final target position and rotation
            _sharkRB.MovePosition(targetPosition);
            _sharkRB.MoveRotation(targetRotation);

            if (_waterExplosionEffect != null)
            {
                _waterExplosionEffect.SetActive(true);
                yield return new WaitForSeconds(1f); // Wait for 2 seconds
                _waterExplosionEffect.SetActive(false);
            }

            // Stop any residual velocity
            _sharkRB.velocity = Vector3.zero;

            // Reset the flag to allow input after the jump
            isParabolicJumping = false;
        }

        private void StartSmoothTransition()
        {
            // Ensure we only start the transition if it's not already in progress
            if (isTransitioning || transitionStarted) return;

            isTransitioning = true;
            transitionStarted = true; // Mark the transition as started
            transitionCompleted = false; // Reset the completion flag for new transitions

            StartCoroutine(HandleSmoothTransition());
        }

        private IEnumerator HandleSmoothTransition()
        {
            float elapsedTime = 0f;
            float duration = 0.5f; // Duration for the smooth transition

            Vector3 initialPosition = _sharkRB.position;
            transitionTargetPosition = new Vector3(_sharkRB.position.x, 0.33f, _sharkRB.position.z);

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;

                // Move the shark smoothly to the target position
                _sharkRB.MovePosition(Vector3.Lerp(initialPosition, transitionTargetPosition, t));

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure the shark reaches the final target position
            _sharkRB.MovePosition(transitionTargetPosition);

            // Correct any floating-point precision errors
            Vector3 correctedPosition = _sharkRB.position;

            _sharkRB.MovePosition(correctedPosition);

            // Stop the transition movement
            _sharkRB.velocity = Vector3.zero;

            // Start the smooth rotation after the transition
            yield return StartCoroutine(HandleSmoothRotation());

            // Reset transition flags
            isTransitioning = false;
            transitionStarted = false; // Allow future transitions
            transitionCompleted = true; // Mark the transition as completed
        }


        private IEnumerator HandleSmoothRotation()
        {
            Quaternion _targetRotation = Quaternion.Euler(0, 90, 0);
            Quaternion initialRotation = _sharkRB.rotation;

            float elapsedTime = 0f;
            float duration = 1f; // Duration for the smooth rotation

            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;

                // Smoothly rotate the shark
                _sharkRB.MoveRotation(Quaternion.Slerp(initialRotation, _targetRotation, t));

                elapsedTime += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            // Ensure final rotation is set correctly
            _sharkRB.MoveRotation(_targetRotation);

            // Correct any floating-point precision errors in position
            Vector3 correctedPosition = _sharkRB.position;

            _sharkRB.MovePosition(correctedPosition);

            // Stop the transition movement
            _sharkRB.velocity = Vector3.zero;
        }

        private IEnumerator InitialSharkMovement()
        {
            float elapsedTime = 0f;
            Quaternion initialRotation = _sharkRB.rotation;
            Quaternion _targetRotation = Quaternion.Euler(90, 90, -90);

            Vector3 initialPosition = _sharkRB.position;
            Vector3 targetPosition = initialPosition + new Vector3(0, -3, 0);

            while (elapsedTime < 1f)
            {
                float t = elapsedTime / 1f;

                // Move and rotate the shark smoothly
                _sharkRB.MovePosition(Vector3.Lerp(initialPosition, targetPosition, t));
                _sharkRB.MoveRotation(Quaternion.Slerp(initialRotation, _targetRotation, t));

                elapsedTime += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }

            // Ensure final position and rotation are set correctly
            _sharkRB.MovePosition(targetPosition);
            _sharkRB.MoveRotation(_targetRotation);

            // Mark the initial movement as completed
            initialMovementCompleted = true;
        }

        private void HandleMovement()
        {
            // Read input values
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            const float smallThreshold = 0.05f; // Adjusted to accommodate fluctuations
            const float yTargetValue = 0.335f; // Target Y value for surface interactions
            bool isMovingOnWater = (Mathf.Abs(_sharkRB.position.y - yTargetValue) < smallThreshold && (Input.GetAxis("Horizontal") != 0));

            if (isMovingOnWater && !isSplashing)
            {
                // Enable splash effect
                EnableSplashEffect();
            }
            else if (!isMovingOnWater && isSplashing)
            {
                // Disable splash effect
                DisableSplashEffect();
            }

          


            if (horizontalInput != 0 || verticalInput != 0)
            {
                Vector3 inputDirection = new Vector3(horizontalInput, verticalInput, 0).normalized;
                Vector3 targetPosition = _sharkRB.position + (inputDirection * _sharkSpeed * Time.fixedDeltaTime);

                // Maintain the current Z position
                targetPosition.z = _sharkRB.position.z;

                // Clamp the Y position within specified bounds
                targetPosition.y = Mathf.Clamp(targetPosition.y, -55f, 0);

                // Move the shark smoothly to the new position
                _sharkRB.MovePosition(Vector3.Lerp(_sharkRB.position, targetPosition, 0.1f));

                isMoving = true; // Set isMoving only when input is detected
            }
            else
            {
                // Stop the shark's movement if no input is detected
                _sharkRB.velocity = Vector3.zero;
                isMoving = false;
            }

            // Handle fog based on the Y position
            if (transform.position.y >= -0.5f)
            {
                RenderSettings.fog = false;
            }
            else if (transform.position.y <= -0.5f)
            {
                EnableFog();
            }
        }


        private void EnableSplashEffect()
        {
            if (_waterSplashEffect != null)
            {
                _waterSplashEffect.SetActive(true); // Activate the splash effect

                isSplashing = true; // Set splashing flag to true
            }
        }

        private void DisableSplashEffect()
        {
            if (_waterSplashEffect != null)
            {
                _waterSplashEffect.SetActive(false); // Deactivate the splash effect
                isSplashing = false; // Reset splashing flag
            }
        }

        private void EnableFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.005f;
            RenderSettings.fogColor = _fogColor;
        }

        private void HandleRotation()
        {
            if (!isMoving)
                return;

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            // Define rotation based on input direction
            if (verticalInput > 0 && horizontalInput == 0)  // Up
            {
                targetRotation = Quaternion.Euler(-90, 0, -180);
                rotationSpeed = 5f;
                _currentSharkDirection = SharkGameDataModel.SharkDirection.Up;
            }
            else if (verticalInput < 0 && horizontalInput == 0)  // Down
            {
                targetRotation = Quaternion.Euler(90, 90, -90);
                rotationSpeed = 5f;
                _currentSharkDirection = SharkGameDataModel.SharkDirection.Down;
            }
            else if (horizontalInput < 0 && verticalInput == 0)  // Left
            {
                targetRotation = Quaternion.Euler(0, -90, 0);
                rotationSpeed = 5f;
                _currentSharkDirection = SharkGameDataModel.SharkDirection.Left;
            }
            else if (horizontalInput > 0 && verticalInput == 0)  // Right
            {
                targetRotation = Quaternion.Euler(0, 90, 0);
                rotationSpeed = 5f;
                _currentSharkDirection = SharkGameDataModel.SharkDirection.Right;
            }
            else if (horizontalInput != 0 && verticalInput != 0)
            {
                if (horizontalInput > 0 && verticalInput > 0)  // Up-Right Diagonal
                {
                    targetRotation = Quaternion.Euler(-25, 110, 50);
                    _currentSharkDirection = SharkGameDataModel.SharkDirection.UpRight;
                }
                else if (horizontalInput > 0 && verticalInput < 0)  // Down-Right Diagonal
                {
                    targetRotation = Quaternion.Euler(60, -250, -70);
                    _currentSharkDirection = SharkGameDataModel.SharkDirection.DownRight;
                }
                else if (horizontalInput < 0 && verticalInput > 0)  // Up-Left Diagonal
                {
                    targetRotation = Quaternion.Euler(230, 30, 130);
                    _currentSharkDirection = SharkGameDataModel.SharkDirection.UpLeft;
                }
                else if (horizontalInput < 0 && verticalInput < 0)  // Down-Left Diagonal
                {
                    targetRotation = Quaternion.Euler(50, -90, 80);
                    _currentSharkDirection = SharkGameDataModel.SharkDirection.DownLeft;
                }
                rotationSpeed = 2.5f;
            }

            // Smooth rotation
            _sharkRB.MoveRotation(Quaternion.Slerp(_sharkRB.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            // Check and update direction if it has changed
            //if (_currentSharkDirection != _previousSharkDirection)
            //{
            //    if (Time.time - lastSpawnTime > spawnCooldown) // Check cooldown
            //    {
            //        SpawnManager._instance.SpawnFishes(_currentSharkDirection);
            //        _previousSharkDirection = _currentSharkDirection;
            //        lastSpawnTime = Time.time; // Update the last spawn time
            //    }
            //}
        }
        #endregion

        #region Collision 

        private bool isGrounded = false;
        private float groundCollisionCooldown = 1f; // Cooldown time to prevent repeated collisions
        private float lastGroundCollisionTime = -1f;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "SmallFish")
            {
                collision.gameObject.transform.SetParent(_sharkHeadPosition);
                StartCoroutine(DeactiveSmallFishAndPushBackToPool(collision.gameObject));
            }
            if (collision.gameObject.tag == "Ground")
            {
                // Stop the shark's movement
                _sharkRB.velocity = Vector3.zero;

                // Check if enough time has passed since the last collision
                if (Time.time - lastGroundCollisionTime > groundCollisionCooldown)
                {
                    // Log the collision
                    Debug.LogError("Collision with Ground");

                    // Update last collision time
                    lastGroundCollisionTime = Time.time;

                    // Set the target rotation
                    targetRotation = Quaternion.Euler(0, 90, 0);

                    // Start rotation coroutine
                    StartCoroutine(RotateToTargetRotation(targetRotation));
                }

                // Set the grounded flag
                isGrounded = true;

                // Disable movement and rotation until the shark is in the air or another condition is met
                StopMovementAndRotation();
            }
        }


        void StopMovementAndRotation()
        {
            // Disable input handling
            horizontalInput = 0f;
            verticalInput = 0f;

            // Optionally, set flags to prevent movement and rotation
            isTransitioning = false; // Prevent smooth transitions if needed
            isParabolicJumping = false; // Ensure no jump is active
        }

        void OnCollisionExit(Collision collision)
        {
            if (collision.gameObject.tag == "Ground")
            {
                // Reset grounded flag when leaving the ground
                isGrounded = false;
            }
        }


        private IEnumerator DeactiveSmallFishAndPushBackToPool(GameObject _fishObject)
        {
            float duration = .1f;
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                _fishObject.transform.localPosition = Vector3.Lerp(_fishObject.transform.localPosition, _sharkMouthPosition.localPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _sharkAnimator.SetBool("attack", true);

            yield return new WaitForSeconds(.25f);

            EnableBloodEffect();

            SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType; // Assuming your Fish class has a FishType property

            _fishObject.transform.parent = null;

          //  ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

            yield return new WaitForSeconds(.25f);

            _sharkAnimator.SetBool("attack", false);

            DisableBloodEffect();
        }

        private void EnableBloodEffect()
        {
            _bloodEffectObject.SetActive(true);
            _bloodEffectObject.GetComponent<ParticleSystem>().Play();
        }
        private void DisableBloodEffect()
        {
            _bloodEffectObject.SetActive(false);
        }
        #endregion
    }
}
