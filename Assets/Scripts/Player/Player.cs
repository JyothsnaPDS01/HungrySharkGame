using System.Collections;
using UnityEngine;
using SharkGame.Models;
using DG.Tweening;

namespace SharkGame
{
    public class Player : MonoBehaviour
    {
        #region Private Variables
        [SerializeField] private float _sharkSpeed = 2f;
        [SerializeField] private float rotationSpeed = 1f; // Speed of rotation
        [SerializeField] private Animator _sharkAnimator;
        [SerializeField] private Rigidbody _sharkRB;
        [SerializeField] private Transform _sharkMouthPosition;
        [SerializeField] private Transform _sharkHeadPosition;
        [SerializeField] private GameObject _bloodEffectObject;

        private bool isSplashing = false; // Flag to track if splashing

        private Quaternion targetRotation;  // The target rotation based on input
        private bool isMoving;              // Whether the shark is currently moving
        private bool isTransitioning = false; // To manage transition state
        private bool transitionStarted = false;

        [SerializeField] private SharkGameDataModel.SharkDirection _currentSharkDirection;
        private SharkGameDataModel.SharkDirection _previousSharkDirection;
        private bool initialMovementCompleted = false;  // Track if initial movement is completed
        private float spawnCooldown = 2f; // Time interval between consecutive spawns
        private float lastSpawnTime = 0f;  // Time of last fish spawn

        private Vector3 transitionTargetPosition; // Target position for transition
        private Quaternion transitionTargetRotation = Quaternion.Euler(0, -90, 0); // Target rotation for transition
        private float transitionDuration = 1f; // Duration of the transition

        private float horizontalInput;
        private float verticalInput;

        [SerializeField] private bool isInputEnabled = true; // Flag to enable or disable input

        [Header("Shark Type")]
        [SerializeField] private SharkGameDataModel.SharkType _sharkType;

        public bool InitialMovement
        {
            get
            {
                return initialMovementCompleted;
            }
        }

        public SharkGameDataModel.SharkDirection CurrentSharkDirection { get { return _currentSharkDirection; } }
        #endregion

        #region Events
        internal void StartGameStartSequence()
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
        #endregion

        #region MonoBehaviour Methods
        [SerializeField] private float smoothTime = 0.3f; // Duration for smoothing transition
        [SerializeField] private float maxSpeed = 10f; // Maximum speed
        private Vector3 velocity = Vector3.zero; // Current velocity
        [SerializeField] private float smoothSpeed = 2f; // Speed of smoothing
        private bool isParabolicJumping = false; // Flag to indicate if a parabolic jump is in progress
        private bool transitionCompleted = false; // Flag to track if the transition is completed

         public float raycastDistance = .5f; // Distance to cast the ray
        public LayerMask wallLayer; // LayerMask to specify which layers are considered as walls


        private bool wasInputEnabled = true;
        private float inputToggleCooldown = 0.2f; // Time to debounce input toggling
        private float lastInputToggleTime = 0f;
        public float stopDistance = 0.05f; // Distance to stop movement from the wall
        private bool isMovementBlocked = false;

        // Store the direction where the wall is detected
        [SerializeField] private SharkGameDataModel.SharkDirection blockedDirection = SharkGameDataModel.SharkDirection.None;

        void FixedUpdate()
        {
            if (!IsReady()) return;

            DetectInput(); // Always check for input

            if (isParabolicJumping || isTransitioning) return;

            // Update current movement direction
            _currentSharkDirection = GetInputDirection(); // Implement this based on your input system

            // Raycast to check for walls in front of the player
            RaycastCheck();

            // Handle movement based on whether it's blocked
            if (!isMovementBlocked || (_currentSharkDirection != blockedDirection))
            {
                HandleSurfaceInteraction(); // Process movement if allowed
                HandleMovementAfterTransition(); // Continue movement logic
            }
        }

        void RaycastCheck()
        {
            RaycastHit hit;
            float distanceToWall;

            // Check in the forward direction
            Vector3 rayDirection;
            // Check in the up direction
            rayDirection = transform.up;
            if (Physics.Raycast(transform.position, rayDirection, out hit, raycastDistance, wallLayer))
            {
                distanceToWall = hit.distance;
                if (distanceToWall <= stopDistance)
                {
#if UNITY_EDITOR
                    //Debug.Log("Wall detected above within stop distance!");
#endif
                    _sharkRB.velocity = Vector3.zero;
                    isMovementBlocked = true;
                    return; // Exit early if a wall is detected above
                }
            }

            // Check in the down direction
            rayDirection = -transform.up;
            if (Physics.Raycast(transform.position, rayDirection, out hit, raycastDistance, wallLayer))
            {
                distanceToWall = hit.distance;
                if (distanceToWall <= stopDistance)
                {
#if UNITY_EDITOR
                    //Debug.Log("Wall detected below within stop distance!");
#endif
                    _sharkRB.velocity = Vector3.zero;
                    isMovementBlocked = true;
                    return; // Exit early if a wall is detected below
                }
            }

            // Check in the right direction
            rayDirection = transform.right;
            if (Physics.Raycast(transform.position, rayDirection, out hit, raycastDistance, wallLayer))
            {
                distanceToWall = hit.distance;
                if (distanceToWall <= stopDistance)
                {
#if UNITY_EDITOR
                    //Debug.Log("Wall detected on the right within stop distance!");
#endif
                    _sharkRB.velocity = Vector3.zero;
                    isMovementBlocked = true;
                    return; // Exit early if a wall is detected in the right direction
                }
            }

            // Check in the left direction
            rayDirection = -transform.right;
            if (Physics.Raycast(transform.position, rayDirection, out hit, raycastDistance, wallLayer))
            {
                distanceToWall = hit.distance;
                if (distanceToWall <= stopDistance)
                {
#if UNITY_EDITOR
                    //Debug.Log("Wall detected on the left within stop distance!");
#endif
                    _sharkRB.velocity = Vector3.zero;
                    isMovementBlocked = true;
                    return; // Exit early if a wall is detected in the left direction
                }
            }

            // Check in the left direction
            rayDirection = transform.forward;
            if (Physics.Raycast(transform.position, rayDirection, out hit, raycastDistance, wallLayer))
            {
                distanceToWall = hit.distance;
                if (distanceToWall <= stopDistance)
                {
#if UNITY_EDITOR
                    //Debug.Log("Wall detected in front within stop distance!");
#endif
                    _sharkRB.velocity = Vector3.zero;
                    isMovementBlocked = true;
                    return; // Exit early if a wall is detected in the forward direction
                }
            }

            // No walls detected in any direction
            isMovementBlocked = false;

            // Visualize the raycasts in the editor (optional for debugging)
#if UNITY_EDITOR
            Debug.DrawRay(transform.position, transform.forward * raycastDistance, Color.red);
            Debug.DrawRay(transform.position, transform.up * raycastDistance, Color.cyan);
            Debug.DrawRay(transform.position, -transform.up * raycastDistance, Color.magenta);
            Debug.DrawRay(transform.position, transform.right * raycastDistance, Color.green);
            Debug.DrawRay(transform.position, -transform.right * raycastDistance, Color.yellow);
#endif
        }



        bool IsReady()
        {
            return initialMovementCompleted && isInputEnabled;
        }

        void DetectInput()
        {
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
        }

        void HandleSurfaceInteraction()
        {
            bool isCloseToSurface = IsCloseToSurface();
            bool isInTransitionRange = IsInTransitionRange();

            if (!isCloseToSurface && _sharkRB.position.y > -1f && isInTransitionRange)
            {
                StartSmoothTransitionIfNecessary();
            }
            else
            {
                transitionCompleted = false;
            }
        }

        bool IsCloseToSurface()
        {
            const float smallThreshold = 0.05f;
            const float yTargetValue = -0.155f;
            return Mathf.Abs(_sharkRB.position.y - yTargetValue) < smallThreshold || Mathf.Approximately(_sharkRB.position.y, yTargetValue);
        }

        bool IsInTransitionRange()
        {
            return _sharkRB.position.y >= -1f && _sharkRB.position.y <= 0f;
        }

        bool IsUpKeyPressed()
        {
            return verticalInput > 0 && Mathf.Approximately(horizontalInput, 0);
        }

        void StartParabolicJump()
        {
#if UNITY_EDITOR
            Debug.LogError("Starting parabolic jump.");
#endif
            isParabolicJumping = true;
            StartCoroutine(HandleParabolicJump());
        }

        void StartSmoothTransitionIfNecessary()
        {
            if ((Mathf.Approximately(horizontalInput, 0) && Mathf.Approximately(verticalInput, 0) || IsUpKeyPressed()) && !transitionCompleted)
            {
                if (!transitionStarted)
                {
#if UNITY_EDITOR
#endif
                    StartSmoothTransition();
                }
            }
        }

        void HandleMovementAfterTransition()
        {
            if (transitionCompleted || !isTransitioning)
            {
                if (SharkGameManager.Instance.CurrentGameMode == SharkGameDataModel.GameMode.GameStart)
                {
                    HandleMovement();
                    HandleRotation();

                    if (Mathf.Approximately(horizontalInput, 0) && Mathf.Approximately(verticalInput, 0))
                    {
#if UNITY_EDITOR
#endif
                        //  MoveSharkForward();
                    }
                }
            }
        }



        // Method to move the shark forward
        void MoveSharkForward()
        {
            if (targetRotation != Quaternion.Euler(-25, 110, 50) || targetRotation != Quaternion.Euler(60, -250, -70) || targetRotation != Quaternion.Euler(230, 30, 130) || targetRotation != Quaternion.Euler(50, -90, 80))
            {
                float forwardSpeed = 0.5f; // Adjust this value to control the forward movement speed
                Vector3 forwardMovement = transform.forward * forwardSpeed * Time.fixedDeltaTime;
                _sharkRB.MovePosition(_sharkRB.position + forwardMovement);
            }
        }
        // Assuming rotationSpeed is defined somewhere in your class
      

        private IEnumerator HandleParabolicJump()
        {
            float elapsedTime = 0f;
            float duration = 1f; // Duration for the parabolic jump

            Vector3 initialPosition = _sharkRB.position;
            Quaternion initialRotation = _sharkRB.rotation; // Save the initial rotation

            // Define the peak height of the parabolic jump
            float peakHeight = 3f; // Adjust this value to control the height of the jump

            // Define the target position for the end of the jump
            Vector3 targetPosition = new Vector3(_sharkRB.position.x, -0.155f, _sharkRB.position.z); // The landing point at water level

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
            transitionTargetPosition = new Vector3(_sharkRB.position.x, -0.155f, _sharkRB.position.z);

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
        Quaternion _targetRotation;

        private IEnumerator InitialSharkMovement()
        {
            Debug.Log("Coming to InitialSharkMovement");
            float elapsedTime = 0f;
            Quaternion initialRotation = _sharkRB.rotation;
            if (_sharkType == SharkGameDataModel.SharkType.GeneralShark)
            {
                _targetRotation = Quaternion.Euler(90, 90, -90);
            }
            else if(_sharkType == SharkGameDataModel.SharkType.LemonShark)
            {
                _targetRotation = Quaternion.Euler(0, 0, 180);
            }

            Vector3 initialPosition = _sharkRB.position;
            Vector3 targetPosition = initialPosition + new Vector3(0, -30f, 0);

            SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.WaterSplash);

            while (elapsedTime < 3f)
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

            GameObject.Find("Main Camera").GetComponent<CameraFollow>().smoothSpeed = 0.0015625f;
            GameObject.Find("Water Surface").SetActive(false);

            // Mark the initial movement as completed
            initialMovementCompleted = true;

            SharkGameManager.Instance.StartTimer();
        }

        private void HandleMovement()
        {
            // Read input values
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");

            const float smallThreshold = 0.05f; // Adjusted to accommodate fluctuations
            const float yTargetValue = -0.155f; // Target Y value for surface interactions
            bool isMovingOnWater = (Mathf.Abs(_sharkRB.position.y - yTargetValue) < smallThreshold  && (Input.GetAxis("Horizontal") != 0));

            if (horizontalInput != 0 || verticalInput != 0)
            {
                Vector3 inputDirection = new Vector3(horizontalInput, verticalInput, 0).normalized;
                Vector3 targetPosition = _sharkRB.position + (inputDirection * _sharkSpeed * 5f * Time.fixedDeltaTime);

                // Maintain the current Z position
                targetPosition.z = _sharkRB.position.z;

                // Clamp the Y position within specified bounds
                targetPosition.y = Mathf.Clamp(targetPosition.y, -45f, -20f);

                targetPosition.x = Mathf.Clamp(targetPosition.x, -53.27f, 86f);

                // Move the shark smoothly to the new position
                _sharkRB.MovePosition(Vector3.Lerp(_sharkRB.position, targetPosition, 0.1f));

                isMoving = true; 

                if(_sharkType != SharkGameDataModel.SharkType.GeneralShark)
                {
                    _sharkAnimator.SetFloat("sharkAmount", .25f);
                }
            }
            else
            {
                // Stop the shark's movement if no input is detected
                _sharkRB.velocity = Vector3.zero;
                isMoving = false;

                if (_sharkType != SharkGameDataModel.SharkType.GeneralShark)
                {
                    _sharkAnimator.SetFloat("sharkAmount", 0f);
                }
            }
        }

      
        private SharkGameDataModel.SharkDirection GetInputDirection()
        {
            if (verticalInput > 0 && horizontalInput == 0)
                return SharkGameDataModel.SharkDirection.Up;
            else if (verticalInput < 0 && horizontalInput == 0)  // Down
            {
                return SharkGameDataModel.SharkDirection.Down;
            }
            else if (horizontalInput < 0 && verticalInput == 0)  // Left
            {
                return SharkGameDataModel.SharkDirection.Left;
            }
            else if (horizontalInput > 0 && verticalInput == 0)  // Right
            {
                return SharkGameDataModel.SharkDirection.Right;
            }

            else if (horizontalInput != 0 && verticalInput != 0)
            {
                if (horizontalInput > 0 && verticalInput > 0)  // Up-Right Diagonal
                {
                    return _currentSharkDirection = SharkGameDataModel.SharkDirection.UpRight;
                }
                else if (horizontalInput > 0 && verticalInput < 0)  // Down-Right Diagonal
                {
                    return _currentSharkDirection = SharkGameDataModel.SharkDirection.DownRight;
                }
                else if (horizontalInput < 0 && verticalInput > 0)  // Up-Left Diagonal
                {
                    return _currentSharkDirection = SharkGameDataModel.SharkDirection.UpLeft;
                }
                else if (horizontalInput < 0 && verticalInput < 0)  // Down-Left Diagonal
                {
                    return SharkGameDataModel.SharkDirection.DownLeft;
                }
            }

            return SharkGameDataModel.SharkDirection.None;
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
                if (_sharkType == SharkGameDataModel.SharkType.GeneralShark)
                {
                    targetRotation = Quaternion.Euler(-90, 0, -180);
                }
                else if (_sharkType == SharkGameDataModel.SharkType.LemonShark)
                {
                    targetRotation = Quaternion.Euler(180 , 0, 180);
                }
                _currentSharkDirection = SharkGameDataModel.SharkDirection.Up;
            }
            else if (verticalInput < 0 && horizontalInput == 0)  // Down
            {
                if (_sharkType == SharkGameDataModel.SharkType.GeneralShark)
                {
                    targetRotation = Quaternion.Euler(90, 90, -90);
                }
                else if (_sharkType == SharkGameDataModel.SharkType.LemonShark)
                {
                    targetRotation = Quaternion.Euler(0, 0, 180);
                }
                _currentSharkDirection = SharkGameDataModel.SharkDirection.Down;
            }
            else if (horizontalInput < 0 && verticalInput == 0)  // Left
            {
                if (_sharkType == SharkGameDataModel.SharkType.GeneralShark)
                {
                    targetRotation = Quaternion.Euler(0, -90, 0);
                }
                else if (_sharkType == SharkGameDataModel.SharkType.LemonShark)
                {
                    targetRotation = Quaternion.Euler(90, 0, 90);
                }
                _currentSharkDirection = SharkGameDataModel.SharkDirection.Left;
            }
            else if (horizontalInput > 0 && verticalInput == 0)  // Right
            {
                if (_sharkType == SharkGameDataModel.SharkType.GeneralShark)
                {
                    targetRotation = Quaternion.Euler(0, 90, 0);
                }
                else if (_sharkType == SharkGameDataModel.SharkType.LemonShark)
                {
                    targetRotation = Quaternion.Euler(90, 0, -90);
                }
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
            }

            // Smooth rotation
            _sharkRB.MoveRotation(Quaternion.Slerp(_sharkRB.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }
        #endregion

        #region Collision 
        [Header("Collision Parameters")]
        private bool isGrounded = false;
        [SerializeField] private float collisionCooldownDuration = 0.5f; // Adjust the cooldown duration as needed
        private bool isCollisionOnCooldown = false;    // Flag to track if collision is on cooldown

        private void OnCollisionEnter(Collision collision)
        {
            if (isCollisionOnCooldown) return; // Exit early if collision is on cooldown
            if (collision.gameObject.tag == "Ground" || collision.gameObject.tag == "Ground1")
            {
#if UNITY_EDITOR
                Debug.LogError("Ground hits");
#endif
                _sharkRB.velocity = Vector3.zero;
                StopMovementAndRotation();
                targetRotation = GetTargetRotation();
                isGrounded = true;
                blockedDirection = _currentSharkDirection;

                // Optional: You can rotate the shark here if needed
                // StartCoroutine(RotateToTargetRotation(targetRotation));

                // Start the cooldown to avoid jerky movements
                StartCoroutine(CollisionCooldown());
            }


            if (collision.gameObject.CompareTag("Bomb"))
            {
                if (UIController.Instance.CurrentAmmo > 0)
                {
                    UIController.Instance.UpdateAmmoHealth(5);
                    SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.BombSound);
                    TriggerShake();
                }
                else if (UIController.Instance.CurrentAmmo == 0)
                {
                    SharkGameManager.Instance.SetGameOver();
                }
            }
        }

       

        // Shake parameters
        public float shakeDuration = 0.5f;   // How long the camera will shake
        public float shakeStrength = 1f;     // How intense the shaking is
        public int shakeVibrato = 10;        // How many times it shakes in the given duration
        public float randomness = 90f;       // How random the shaking is

        public GameObject _particleEffect;

        // Call this function to trigger the shake
        public void TriggerShake()
        {
            Camera mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
            if (mainCamera != null)
            {
                // Shake the camera
                mainCamera.transform.DOShakePosition(shakeDuration, new Vector3(shakeStrength, 0, 0), shakeVibrato, randomness);

                // Set the particle effect active
                _particleEffect.SetActive(true);

                // Scale the particle effect from (0,0,0) to (5.5,1,5.5) using DOScale
                _particleEffect.transform.localScale = Vector3.zero; // Start at scale (0,0,0)
                _particleEffect.transform.DOScale(new Vector3(5.5f, 1f, 5.5f), .5f); // Tween to scale (5.5, 1, 5.5)

                // Deactivate the particle effect after the shake duration
                StartCoroutine(DeactiveTheParticleEffect());

                IEnumerator DeactiveTheParticleEffect()
                {
                    yield return new WaitForSeconds(shakeDuration);
                    _particleEffect.SetActive(false);
                    _particleEffect.transform.localScale = Vector3.zero; // Reset scale after deactivation
                }
            }
        }


        private IEnumerator CollisionCooldown()
        {
            isCollisionOnCooldown = true; // Set cooldown to active
            yield return new WaitForSeconds(collisionCooldownDuration); // Wait for the cooldown duration
            isCollisionOnCooldown = false; // Reset cooldown after the duration
        }


        private Quaternion GetTargetRotation()
        {
            if (_currentSharkDirection == SharkGameDataModel.SharkDirection.Up)  // Up
            {
                return Quaternion.Euler(90, 90, -90);
            }
            else if (_currentSharkDirection == SharkGameDataModel.SharkDirection.Down)  // Down
            {
                return Quaternion.Euler(-90, 0, -180);
            }
            else if (_currentSharkDirection == SharkGameDataModel.SharkDirection.Left)  // Left
            {
                return Quaternion.Euler(0, 90, 0);
            }
            else if (_currentSharkDirection == SharkGameDataModel.SharkDirection.Right)  // Right
            {
                return Quaternion.Euler(0, -90, 0);

            }
            else if (_currentSharkDirection == SharkGameDataModel.SharkDirection.UpRight)
            {
                return Quaternion.Euler(230, 30, 130);
            }
            else if (_currentSharkDirection == SharkGameDataModel.SharkDirection.DownRight)  // Down-Right Diagonal
            {
                return Quaternion.Euler(50, -90, 80);
            }
            else if (_currentSharkDirection == SharkGameDataModel.SharkDirection.UpLeft)  // Up-Left Diagonal
            {
                return Quaternion.Euler(-25, 110, 50);

            }
            else if (_currentSharkDirection == SharkGameDataModel.SharkDirection.DownLeft)  // Down-Left Diagonal
            {
                return Quaternion.Euler(60, -250, -70);
            }
            return Quaternion.Euler(0, 0, 0);
        }

        // Methods to enable and disable input
        public void EnableInput()
        {
            isInputEnabled = true;
        }

        public void DisableInput()
        {
            isInputEnabled = false;
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

        private bool IsSmallFishNearToPlayer()
        {
            float detectionRadius = .5f;
            Collider[] nearbyFishes = Physics.OverlapSphere(_sharkMouthPosition.position, detectionRadius, wallLayer);
            return nearbyFishes.Length > 0; // Returns true if any fish are detected nearby
        }

        internal void EnableBloodEffect()
        {
            _bloodEffectObject.SetActive(true);
            _bloodEffectObject.GetComponent<ParticleSystem>().Play();
        }
        internal void DisableBloodEffect()
        {
            _bloodEffectObject.SetActive(false);
        }

        internal void PlayEatAnimation()
        {
            if (_sharkType == SharkGameDataModel.SharkType.GeneralShark)
            {
                _sharkAnimator.SetBool("attack", true);
            }
            else if(_sharkType != SharkGameDataModel.SharkType.GeneralShark)
            {
                _sharkAnimator.SetBool("attack", true);
            }
            SoundManager.Instance.PlayAudioClip(SharkGameDataModel.Sound.EatingShark);
        }

        internal void ShowDieState()
        {
            if (_sharkType == SharkGameDataModel.SharkType.GeneralShark)
            {
                _sharkAnimator.SetTrigger("Die");
            }
            else if (_sharkType != SharkGameDataModel.SharkType.GeneralShark)
            {
                _sharkAnimator.SetFloat("sharkAmount", 1f);
            }
            
        }

        internal void BackToIdleAnimation()
        {
            if (_sharkType == SharkGameDataModel.SharkType.GeneralShark)
            {
                _sharkAnimator.SetBool("attack", false);
            }
            else if (_sharkType != SharkGameDataModel.SharkType.GeneralShark)
            {
                _sharkAnimator.SetBool("attack", false);
            }

        }
        #endregion

        #region Player Die Animation
        public float sineAmplitude = 0.5f;   // Amplitude of the sine wave (how much it oscillates up/down)
        public float sineFrequency = 2f;     // Frequency of the sine wave (how fast it oscillates)
        public float dierotationSpeed = 20f;    // Speed of rotation around Z-axis
        public float moveSpeed = 2f;         // Speed of moving down
        public float totalFallDistance = 5f; // Total distance to move down

        private float initialYPosition;
        private float currentDistanceMoved = 0f;

        private bool isDying = false;

        public void StartDieAnimation()
        {
            initialYPosition = _sharkRB.position.y;

            DisableInput();

            // Check if the initial Y position is between -40 and -45
            if (initialYPosition >= -45f && initialYPosition <= -35f)
            {
                // Add the offset of 10 to the Y position
                initialYPosition += 10f;
            }

            Debug.LogError("StartDieAnimation");

            if (!isDying)
            {
                StartCoroutine(DieAnimation());
            }
        }

        IEnumerator DieAnimation()
        {
            isDying = true;
            currentDistanceMoved = 0f;  // Reset the distance moved

            while (currentDistanceMoved < totalFallDistance)
            {
                // Calculate new Y position with sine wave
                float sineWaveOffset = Mathf.Sin(Time.time * sineFrequency) * sineAmplitude;
                float newYPosition = initialYPosition - currentDistanceMoved + sineWaveOffset;

                // Move the shark using Rigidbody (MovePosition)
                Vector3 newPosition = new Vector3(transform.position.x, newYPosition, transform.position.z);
                _sharkRB.MovePosition(newPosition);

                // Get current Z-axis rotation
                float currentZRotation = _sharkRB.rotation.eulerAngles.z;
                // Calculate the new Z-axis rotation, clamped between 180 and 260 degrees
                float newZRotation = Mathf.Clamp(currentZRotation + (dierotationSpeed * Time.deltaTime), 180f, 260f);

                // Apply the X-axis rotation based on the sine wave for wobble effect
                float newXRotation = Mathf.Sin(Time.time * sineFrequency) * 10f;  // Adjust '10f' for how much the X-axis rotates

                // Apply the new rotation using Rigidbody (MoveRotation)
                Quaternion newRotation = Quaternion.Euler(newXRotation, 0, newZRotation);
                _sharkRB.MoveRotation(newRotation);

                // Increase the distance moved down smoothly
                currentDistanceMoved += moveSpeed * Time.deltaTime;

                yield return null; // Wait for the next frame
            }

            // Ensure the shark reaches the exact destination using Rigidbody
            Vector3 finalPosition = new Vector3(transform.position.x, initialYPosition - totalFallDistance, transform.position.z);
            _sharkRB.MovePosition(finalPosition);

            isDying = false;

        }
        #endregion
    }
}
