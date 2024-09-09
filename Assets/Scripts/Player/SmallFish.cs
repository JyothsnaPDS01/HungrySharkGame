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
        [SerializeField] private float movementSpeed = 20f;
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

            // Start coroutine for direction change at intervals
            StartCoroutine(ChangeDirectionAtIntervals());
        }

        private void FixedUpdate()
        {
            // Move the fish continuously
            MoveTheSmallFish();
        }
        #endregion

        #region Coroutines
        private IEnumerator ChangeDirectionAtIntervals()
        {
            while (true)
            {
                yield return new WaitForSeconds(directionChangeInterval);
                _targetDirection = GetRandomDirection();
            }
        }

        #endregion

        #region Private Methods
        private void MoveTheSmallFish()
        {
            Vector3 separationVector = CalculateSeparationVector();
            Vector3 movementDirection = (_targetDirection + separationVector).normalized;

            // Smoothly rotate the fish towards the movement direction
            RotateTowards(movementDirection);

            _smallFishAnimator.SetFloat("moveAmount", 0.5f);

            // Move the fish forward
            transform.position += movementDirection * movementSpeed * Time.deltaTime;
            Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, _initialZ);
            newPosition.y = Mathf.Clamp(newPosition.y, -20f, -0.5f);
            transform.position = newPosition;
        }

        private Vector3 GetRandomDirection()
        {
            List<Vector3> directions = new List<Vector3>
            {
                transform.forward,
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
