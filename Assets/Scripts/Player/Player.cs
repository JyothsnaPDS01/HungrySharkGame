using System.Collections;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class Player : MonoBehaviour
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
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");

                // Determine if "up" input is given
                bool upKeyPressed = verticalInput > 0 && Mathf.Approximately(horizontalInput, 0); // Only "up" input, no horizontal input
                bool noInput = Mathf.Approximately(horizontalInput, 0) && Mathf.Approximately(verticalInput, 0); // No input is given

                // Debugging input and position
                Debug.Log($"Horizontal Input: {horizontalInput}, Vertical Input: {verticalInput}");
                Debug.Log($"Up Key Pressed: {upKeyPressed}, No Input: {noInput}");
                Debug.Log($"Y Position: {transform.position.y}, Transitioning: {isTransitioning}, Transition Started: {transitionStarted}, Transition Completed: {transitionCompleted}");

                // Check if parabolic jump is in progress
                if (isParabolicJumping)
                {
                    // Skip input handling during the parabolic jump
                    return;
                }

                // Check if transition is in progress
                if (isTransitioning)
                {
                    // If transitioning, do nothing and let the coroutine handle the movement
                    return;
                }

                const float smallThreshold = 0.001f; // A small threshold value to consider "close to zero"

                if (Mathf.Abs(_sharkRB.position.y) < smallThreshold && upKeyPressed)
                {
                    // Start the parabolic arc if the shark is effectively at the surface (Y near zero) and "Up" key is pressed
                    Debug.LogError("Starting parabolic jump.");
                    isParabolicJumping = true; // Set the flag to indicate a parabolic jump is in progress
                    StartCoroutine(HandleParabolicJump());
                    return; // Exit to prevent further handling until jump is complete
                }

                // Check if we are in the transition range
                if (_sharkRB.position.y >= -1f && _sharkRB.position.y <= 0f)
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

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    // Prevent processing upward input if jumping
                    if (!isParabolicJumping)
                    {
                        // Handle upward input here if necessary
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
                    // Handle movement and rotation based on input
                    HandleMovement();
                    HandleRotation();
                }
            }
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
            Vector3 targetPosition = new Vector3(_sharkRB.position.x, 0, _sharkRB.position.z); // The landing point at water level

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
            transitionTargetPosition = new Vector3(_sharkRB.position.x, 0, _sharkRB.position.z);

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

            // If Y position is very close to zero, set it explicitly to 0
            if (Mathf.Abs(correctedPosition.y) < Mathf.Epsilon)
            {
                correctedPosition.y = 0;
            }

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
                yield return null;
            }

            // Ensure final rotation is set correctly
            _sharkRB.MoveRotation(_targetRotation);

            // Correct any floating-point precision errors in position
            Vector3 correctedPosition = _sharkRB.position;

            // If Y position is very close to zero, set it explicitly to 0
            if (Mathf.Abs(correctedPosition.y) < Mathf.Epsilon)
            {
                correctedPosition.y = 0;
            }

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
                yield return null;
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

            //// Define target positions based on input and current position
            //float targetYPosition = transform.position.y; // Default to current Y position
            //if (verticalInput > 0 && transform.position.y <= -1f) // Move up if "up" arrow is pressed
            //{
            //    targetYPosition = -2f; // Target Y position for "up" movement
            //}
            //else if (verticalInput < 0 && transform.position.y >= -23f) // Move down if "down" arrow is pressed
            //{
            //    targetYPosition = -5f; // Target Y position for "down" movement
            //}

            //// Move directly to the target Y position if it's different from the current position
            //if (targetYPosition != transform.position.y)
            //{
            //    Vector3 targetPosition = new Vector3(transform.position.x, targetYPosition, transform.position.z);

            //    StartCoroutine(MoveSmoothTargetYPosition(targetPosition));
            //}
            //else
            //{
            // Smooth movement based on input if no direct movement is needed
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
            //}

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

        private IEnumerator MoveSmoothTargetYPosition(Vector3 _targetPosition)
        {
            float elapsedTime = 0f;
            float duration = 10f; // Duration for the smooth rotation
            while (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                // Smoothly rotate the shark
                _sharkRB.MovePosition(Vector3.Lerp(_sharkRB.position, _targetPosition, t));

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            // Ensure final rotation is set correctly
            _sharkRB.MovePosition(_targetPosition);
            _sharkRB.velocity = Vector3.zero; // Stop any current velocity
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
            if (_currentSharkDirection != _previousSharkDirection)
            {
                if (Time.time - lastSpawnTime > spawnCooldown) // Check cooldown
                {
                    SpawnManager._instance.SpawnFishes(_currentSharkDirection);
                    _previousSharkDirection = _currentSharkDirection;
                    lastSpawnTime = Time.time; // Update the last spawn time
                }
            }
        }
        #endregion

        #region Collision 

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "SmallFish")
            {
                collision.gameObject.transform.SetParent(_sharkHeadPosition);
                StartCoroutine(DeactiveSmallFishAndPushBackToPool(collision.gameObject));
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

            SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType; // Assuming your Fish class has a FishType property

            _fishObject.transform.parent = null;

            ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

            EnableBloodEffect();

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
