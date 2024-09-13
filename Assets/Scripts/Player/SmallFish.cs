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
        [SerializeField] private float directionChangeInterval = 2f;
        [SerializeField] private float separationDistance = 1f;
        [SerializeField] private float rotationSpeed = 5f;
        private Vector3 _targetDirection;
        private float _initialZ;
        [SerializeField] internal SharkGameDataModel.SmallFishType _smallFishType;
        private List<SmallFish> allFish;
        private bool _isPlayerNearby = false;
        private bool _isCoroutineRunning = false;

        [SerializeField] private SharkGameDataModel.SmallFishFiniteState _currentState = SharkGameDataModel.SmallFishFiniteState.ReBorn;  // FSM state
        #endregion

        #region Monobehaviour Methods
        private void Start()
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

            _targetDirection = GetRandomDirection();
            _initialZ = transform.position.z;
            allFish = new List<SmallFish>(FindObjectsOfType<SmallFish>());

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
            MoveTheSmallFish();
            if (IsDeadConditionMet())
            {
                _currentState = SharkGameDataModel.SmallFishFiniteState.Die;
            }
        }


        private void HandleDeadState()
        {
            // Logic for when fish is dead
            _smallFishAnimator.SetTrigger("Die"); // Assuming you have a die animation
            // Optionally remove or disable the fish object
            Destroy(gameObject, 2f); // Destroy the fish after 2 seconds
        }
        #endregion

        #region Private Methods
        private bool IsDeadConditionMet()
        {
            // Implement logic to determine if the fish should transition to the Dead state
            // For example, check if fish collides with a certain object or goes out of bounds
            return false; // Placeholder for actual logic
        }

        private float rayCastDistance = 5f;
        [SerializeField] private LayerMask wallLayer;
        private void MoveTheSmallFish()
        {
            Vector3 separationVector = CalculateSeparationVector();
            Vector3 movementDirection = (_targetDirection + separationVector).normalized;

            RotateTowards(movementDirection);

            _smallFishAnimator.SetFloat("moveAmount", 0.5f);

            RaycastHit hitInfo;
            bool isHit = Physics.Raycast(transform.position, movementDirection, out hitInfo, rayCastDistance);

            if (isHit)
            {
                if (hitInfo.collider.gameObject.tag != "Wall")
                {
                    transform.position += movementDirection * movementSpeed * Time.deltaTime;
                }
                else
                {
                    Vector3 reverseDirection = -movementDirection;
                    RotateTowards(reverseDirection);
                    transform.position += reverseDirection * movementSpeed * Time.deltaTime;
                }
            }
            else
            {
                transform.position += movementDirection * movementSpeed * Time.deltaTime;
            }

            Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, _initialZ);
            newPosition.y = Mathf.Clamp(newPosition.y, -20f, -0.5f);
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

            return separation.normalized * curveIntensity;
        }

        private void RotateTowards(Vector3 direction)
        {
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        #endregion
    }
}
