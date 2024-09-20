using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class SmallFish : MonoBehaviour
    {
        #region Private Variables
        private Transform _playerShark;
        [SerializeField] private float movementSpeed = 10f;
        [SerializeField] private Animator _smallFishAnimator;
        [SerializeField] private float curveIntensity = 0.5f;
        [SerializeField] private float directionChangeInterval = .0035f;
        [SerializeField] private float separationDistance = 1f;
        [SerializeField] private float rotationSpeed = 5f;
        private Vector3 _targetDirection;
        private float _initialZ;
        [SerializeField] internal SharkGameDataModel.SmallFishType _smallFishType;
        private List<SmallFish> allFish;
        private bool _isPlayerNearby = false;
        private bool _isCoroutineRunning = false;

        private Vector3 movementDirection;

       [SerializeField] internal SharkGameDataModel.SmallFishFiniteState _currentState = SharkGameDataModel.SmallFishFiniteState.ReBorn;  // FSM state
        #endregion

        #region Monobehaviour Methods
        private void OnEnable()
        {
            GameObject sharkObject = GameObject.Find("Player_Shark");
            if (sharkObject != null)
            {
                _playerShark = sharkObject.transform;
            }
            else
            {
                Debug.LogError("Player_Shark not found in the scene. Check the name or ensure it's active.");
            }
        }
        private void OnDisable()
        {
            StopAllCoroutines();  // Ensure that all coroutines stop when the object is disabled
            _isCoroutineRunning = false;  // Reset the flag
        }

        private void Start()
        {
            _targetDirection = GetRandomDirection();
            _initialZ = transform.position.z;
            allFish = new List<SmallFish>(FindObjectsOfType<SmallFish>());
            if (_currentState == SharkGameDataModel.SmallFishFiniteState.Die)
                _currentState = SharkGameDataModel.SmallFishFiniteState.ReBorn;

            transform.rotation = Quaternion.Euler(0, 90, 0);
            // Start coroutines for direction change and movement
            StartCoroutine(FSMUpdate());
        }
        #endregion

        #region Coroutines
        private IEnumerator FSMUpdate()
        {
            while (true)
            {
                switch (_currentState)
                {
                    case SharkGameDataModel.SmallFishFiniteState.ReBorn:
                        HandleBornState();
                        break;
                    case SharkGameDataModel.SmallFishFiniteState.Movement:
                        HandleMovingState();
                        break;
                    case SharkGameDataModel.SmallFishFiniteState.Die:
                        HandleDeadState();
                        break;
                }
                yield return null; // Wait for the next frame
            }
        }

        private IEnumerator ChangeDirectionAtIntervals()
        {
            while (_currentState == SharkGameDataModel.SmallFishFiniteState.Movement)
            {
                yield return new WaitForSeconds(directionChangeInterval);
                _targetDirection = GetRandomDirection();
            }
        }

        private IEnumerator MoveTheSmallFishCoroutine()
        {
            while (_currentState == SharkGameDataModel.SmallFishFiniteState.Movement)
            {
                MoveTheSmallFish();
                yield return null; // Wait for the next frame
            }
        }
        #endregion

        #region State Handling
        private void HandleBornState()
        {
            // Initialization or birth logic
            _currentState = SharkGameDataModel.SmallFishFiniteState.Movement;  // Transition to Moving state
        }
        private void HandleMovingState()
        {
            if (!_isCoroutineRunning)
            {
                StartCoroutine(ChangeDirectionAtIntervals());
                StartCoroutine(MoveTheSmallFishCoroutine());
                _isCoroutineRunning = true;
            }

            // Check if the shark is nearby
            float distanceToShark = Vector3.Distance(transform.position, _playerShark.position);

            if (distanceToShark <= 1f)
            {
                StartCoroutine(EscapeFromSharkCoroutine()); // Use coroutine for escape
            }
            else
            {
                MoveTheSmallFish();
            }

            if (IsDeadConditionMet())
            {
                _currentState = SharkGameDataModel.SmallFishFiniteState.Die;
            }
        }

        private IEnumerator EscapeFromSharkCoroutine()
        {
            // Calculate direction away from the shark
            Vector3 directionAwayFromShark = (transform.position - _playerShark.position).normalized;

            // Add separation vector to avoid other fish while escaping
            Vector3 separationVector = CalculateSeparationVector();
            Vector3 escapeDirection = (directionAwayFromShark + separationVector).normalized;

            float originalSpeed = movementSpeed;
            float escapeSpeed = .02f; // Increase speed during escape
            float curveAmplitude = 4f; // Amplitude of the curve
            float curveFrequency = 5f; // Frequency of the curve oscillation
            float escapeDuration = 2f; // Duration of the escape phase

            float timeElapsed = 0f;

            while (timeElapsed < escapeDuration)
            {
                // Calculate the curve offset
                float curveOffset = Mathf.Sin(timeElapsed * curveFrequency) * curveAmplitude;
                Vector3 curvedDirection = Quaternion.Euler(0, 0, curveOffset) * escapeDirection;

                // Rotate the fish smoothly towards the escape direction
                  RotateTowards(curvedDirection);
                // Handle raycasting to detect walls and adjust movement accordingly

                transform.LookAt(curvedDirection);

                RaycastHit hitInfo;
                bool isHit = Physics.Raycast(transform.position, -transform.up, out hitInfo, rayCastDistance);
                Debug.DrawRay(transform.position, -transform.up, Color.red);

                if (isHit)
                {
                    if (hitInfo.collider.CompareTag("Ground"))
                    {
                        Debug.Log("Ground Hit");
                        // Move up or down with a small offset
                        Vector3 offset = new Vector3(0, 0.5f, 0); // Adjust this offset as needed
                        transform.position += offset; // Move the fish up
                    }
                    else
                    {
                        Debug.Log("Wall Hit");
                        Debug.Log("Hit Object: " + hitInfo.collider.gameObject);
                        // Move up or down with a small offset
                        Vector3 offset = new Vector3(0, Random.Range(-0.5f, 0.5f), 0); // Randomly choose to move up or down
                        transform.position += offset; // Move the fish up or down

                        // Move horizontally away from the wall
                        Vector3 reverseDirection = new Vector3(-curvedDirection.x, 0, 0);
                        transform.position += reverseDirection * escapeSpeed * Time.deltaTime;
                    }
                }
                else
                {
                    transform.position += curvedDirection * escapeSpeed * Time.deltaTime;
                }
                // Move the fish in the escape direction
                _smallFishAnimator.SetFloat("moveAmount", 0.5f); // Slightly faster movement for escape
               


                // Maintain fish's z-position and allow y-position to change during escape
                Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, _initialZ);
                newPosition.y = Mathf.Clamp(newPosition.y, -20f, -0.5f); // Adjust this if needed for escape
                transform.position = newPosition;

                timeElapsed += Time.deltaTime;
                yield return null; // Wait for the next frame
            }

            // Reset the speed to its original value after escaping
            movementSpeed = originalSpeed;
        }

        public void ResetFishState()
        {
            // Reset the state to ReBorn and ensure coroutines restart
            _currentState = SharkGameDataModel.SmallFishFiniteState.ReBorn;
            _isCoroutineRunning = false; // Mark coroutines not running so FSM can start them again

            // Reset other necessary parameters if needed (e.g., position, speed, etc.)
         //   _targetDirection = GetRandomDirection();
            StopAllCoroutines(); // Ensure no old coroutines are running
            if(this.gameObject.activeInHierarchy)
                StartCoroutine(FSMUpdate()); // Restart the FSM
        }

        private void HandleDeadState()
        {
            
        }
        #endregion

        #region Private Methods
        private bool IsDeadConditionMet()
        {
            // Implement logic to determine if the fish should transition to the Dead state
            // For example, check if fish collides with a certain object or goes out of bounds
            return false; // Placeholder for actual logic
        }

        private float rayCastDistance = .5f;

        private void MoveTheSmallFish()
        {
            // Get the individual behavior vectors
            Vector3 separationVector = CalculateSeparationVector();
            Vector3 alignmentVector = CalculateAlignmentVector();
            Vector3 cohesionVector = CalculateCohesionVector();

            // Adjust the weighting of each behavior
            float separationWeight = 1.5f;
            float alignmentWeight = 1.0f;
            float cohesionWeight = 1.0f;

            // Combine the behavior vectors to determine the new movement direction
            movementDirection = (separationVector * separationWeight +
                                 alignmentVector * alignmentWeight +
                                 cohesionVector * cohesionWeight).normalized;

            // Restrict movement to x and y direction
            movementDirection.y = 0;
            movementDirection.z = 0;

            RotateTowards(movementDirection);

            // Move the fish in the calculated direction
            _smallFishAnimator.SetFloat("moveAmount", 0.5f);

            // Handle raycasting to detect walls and adjust movement accordingly
            RaycastHit hitInfo;
            bool isHit = Physics.Raycast(transform.position, transform.forward, out hitInfo, rayCastDistance);
            Debug.DrawRay(transform.position, transform.forward, Color.red);

            if (isHit)
            {
                if (hitInfo.collider.CompareTag("Ground"))
                {
                    Debug.Log("Ground Hit");
                    // Move up or down with a small offset
                    Vector3 offset = new Vector3(0, 0.5f, 0); // Adjust this offset as needed
                    transform.position += offset; // Move the fish up
                }
                else
                {
                    Debug.Log("Wall Hit");
                    Debug.Log("Hit Object: " + hitInfo.collider.gameObject);
                    // Move up or down with a small offset
                    Vector3 offset = new Vector3(0, Random.Range(-0.5f, 0.5f), 0); // Randomly choose to move up or down
                    transform.position += offset; // Move the fish up or down

                    // Move horizontally away from the wall
                    Vector3 reverseDirection = new Vector3(-movementDirection.x, 0, 0);
                    transform.position += reverseDirection * movementSpeed * Time.deltaTime;
                }
            }
            else
            {
                transform.position += movementDirection * movementSpeed * Time.deltaTime;
            }

            // Keep the z position constant and clamp y position within bounds
            Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, _initialZ);
            newPosition.y = Mathf.Clamp(newPosition.y, -20f, -0.5f);
            newPosition.x = Mathf.Clamp(newPosition.x, -80f, 80f);
            transform.position = newPosition;

            if (transform.position.x == 80f || transform.position.x == -80f)
            {
                movementDirection.x = -movementDirection.x;
                RotateTowards(movementDirection);
            }
        }



        private void EscapeFromShark()
        {
            Debug.Log("EscapeFromShark");

            // Calculate direction away from the shark
            Vector3 directionAwayFromShark = (transform.position - _playerShark.position).normalized;

            // Add separation vector to avoid other fish while escaping
            Vector3 separationVector = CalculateSeparationVector();
            Vector3 escapeDirection = (directionAwayFromShark + separationVector).normalized;

            // Rotate the fish smoothly towards the escape direction
           //RotateTowards(escapeDirection);

            // Move the fish in the escape direction
            _smallFishAnimator.SetFloat("moveAmount", 0.8f);  // Slightly faster movement for escape
            transform.position += escapeDirection * (movementSpeed * 1.5f) * Time.deltaTime;  // Increase speed during escape

            // Maintain fish's z-position and allow y-position to change during escape
            Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, _initialZ);
            newPosition.y = Mathf.Clamp(newPosition.y, -20f, -0.5f); // Adjust this if needed for escape
            transform.position = newPosition;
        }


        private Vector3 GetRandomDirection()
        {
            List<Vector3> directions = new List<Vector3>
            {
                transform.forward,
                -transform.forward
            };

            return directions[Random.Range(0, directions.Count)].normalized;
        }

        private Vector3 CalculateSeparationVector()
        {
            Vector3 separation = Vector3.zero;
            int nearbyFishCount = 0;

            foreach (SmallFish fish in allFish)
            {
                if (fish != this)
                {
                    float distance = Vector3.Distance(transform.position, fish.transform.position);
                    if (distance < separationDistance)
                    {
                        separation += (transform.position - fish.transform.position) / distance;
                        nearbyFishCount++;
                    }
                }
            }

            if (nearbyFishCount > 0)
            {
                separation /= nearbyFishCount;
            }

            return separation.normalized * curveIntensity; // Scale the separation effect
        }

        private Vector3 CalculateAlignmentVector()
        {
            Vector3 alignment = Vector3.zero;
            int nearbyFishCount = 0;

            foreach (SmallFish fish in allFish)
            {
                if (fish != this)
                {
                    float distance = Vector3.Distance(transform.position, fish.transform.position);
                    if (distance < separationDistance * 2) // Align over a larger distance
                    {
                        alignment += fish.movementDirection; // Consider the direction of other fish
                        nearbyFishCount++;
                    }
                }
            }

            if (nearbyFishCount > 0)
            {
                alignment /= nearbyFishCount; // Get the average direction
            }

            return alignment.normalized; // Return normalized vector to apply alignment force
        }

        private Vector3 CalculateCohesionVector()
        {
            Vector3 cohesion = Vector3.zero;
            int nearbyFishCount = 0;

            foreach (SmallFish fish in allFish)
            {
                if (fish != this)
                {
                    float distance = Vector3.Distance(transform.position, fish.transform.position);
                    if (distance < separationDistance * 2)
                    {
                        cohesion += fish.transform.position; // Add the position of other fish
                        nearbyFishCount++;
                    }
                }
            }

            if (nearbyFishCount > 0)
            {
                cohesion /= nearbyFishCount; // Get the average position of nearby fish
                cohesion = (cohesion - transform.position).normalized; // Direction towards the average position
            }

            return cohesion;
        }


        private void RotateTowards(Vector3 direction)
        {
            if (direction != Vector3.zero)
            {
                //Create a rotation that faces the direction, but keep z - axis unchanged
                 Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, 0), Vector3.up);


                if (targetRotation != Quaternion.Euler(0, 0, 0))
                {
                    //Smoothly interpolate to the target rotation
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                    //Ensure that the z rotation is fixed if that's intended
                    Vector3 euler = transform.rotation.eulerAngles;
                    euler.z = 0; // Fix z-axis rotation
                    transform.rotation = Quaternion.Euler(euler);
                }
            }
        }

        public void SetMovementDirection(Vector3 direction)
        {
            movementDirection = direction;
        }

        #endregion


    }
}
