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

        void FixedUpdate()
        {
            // Check if the initial movement is completed
            if (initialMovementCompleted)
            {
                // Handle smooth transition if it is in progress
                if (isTransitioning)
                {
                    StartCoroutine(HandleSmoothTransition());
                }
                else
                {
                    // Start the smooth transition if conditions are met
                    if (transform.position.y >= -1f)
                    {
                        StartSmoothTransition();
                    }

                    // Regular movement and rotation handling
                    HandleMovement();
                    HandleRotation();
                }
            }
        }
        float transitionElapsedTime;
        Vector3 transitionStartPosition;

        private IEnumerator HandleSmoothTransition()
        {
            float elapsedTime = 0f;

            Vector3 initialPosition = _sharkRB.position;
            transitionTargetPosition = new Vector3(_sharkRB.position.x, 0, _sharkRB.position.z);

            while (elapsedTime < .5f)
            {
                float t = elapsedTime / .5f;

                // Move and rotate the shark smoothly
                _sharkRB.MovePosition(Vector3.Lerp(initialPosition, transitionTargetPosition, t));

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure final position and rotation are set correctly
            _sharkRB.MovePosition(transitionTargetPosition);

            // Stop the transition and finalize the position
            _sharkRB.velocity = Vector3.zero;
            isTransitioning = false;
            
        }

        private void StartSmoothTransition()
        {
            isTransitioning = true;
            transitionElapsedTime = 0f;
            transitionStartPosition = _sharkRB.position;
            velocity = Vector3.zero; // Reset the velocity
        }


        private IEnumerator InitialSharkMovement()
        {
            float elapsedTime = 0f;
            Quaternion initialRotation = _sharkRB.rotation;
            Quaternion _targetRotation = Quaternion.Euler(90, 90, -90);

            Vector3 initialPosition = _sharkRB.position;
            Vector3 targetPosition = initialPosition + new Vector3(0, -5, 0);

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

            // Only proceed with movement if there's input
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

            if (transform.position.y >= -0.5f)
                RenderSettings.fog = false;
            else if (transform.position.y <= -0.5f)
                EnableFog();
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
