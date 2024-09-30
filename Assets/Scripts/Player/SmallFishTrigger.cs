using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;


namespace SharkGame
{
    public class SmallFishTrigger : MonoBehaviour
    {
        [SerializeField] private Transform _sharkHeadPosition;
        [SerializeField] private Transform _sharkMouthPosition;

        [SerializeField] private GameObject _player;
        [SerializeField] private Transform _eatingPosition;

        [SerializeField] private float detectionInterval;
        [SerializeField] private float detectionRadius;
        [SerializeField] private LayerMask fishLayerMask;

        private Rigidbody _sharkRigidBody;

        public float DetectionRadius
        {
            set
            {
                detectionRadius = value;
            }
        }

        private void Start()
        {
            _sharkRigidBody = _player.GetComponent<Rigidbody>();
        }


        private void OnEnable()
        {
            SharkGameManager.Instance.OnGameModeChanged += HandleGameMode;
        }

        private void OnDisable()
        {
            SharkGameManager.Instance.OnGameModeChanged -= HandleGameMode;
        }

        private void HandleGameMode(SharkGameDataModel.GameMode currentGameMode)
        {
            if (currentGameMode == SharkGameDataModel.GameMode.GameStart)
            {
                StartCoroutine(CheckNearbyFishesAtIntervals());
            }
        }

        private IEnumerator CheckNearbyFishesAtIntervals()
        {
            while (true) // This loop will keep running indefinitely
            {
                yield return new WaitForSeconds(detectionInterval); // Wait for the detection interval

                // Check for nearby fishes
                yield return StartCoroutine(CheckNearbyFishesOverTime());
            }
        }

        private IEnumerator CheckNearbyFishesOverTime()
        {
            Debug.Log("CheckNearbyFishesOverTime");
            Collider[] nearbyFishes = Physics.OverlapSphere(this.transform.position, detectionRadius, fishLayerMask);

            foreach (Collider fishCollider in nearbyFishes)
            {
                if (fishCollider.CompareTag("SmallFish"))
                {
#if UNITY_EDITOR
                    Debug.Log("Finding the small fishes");
#endif
                    GameObject fishObject = fishCollider.gameObject;
                    StartCoroutine(BringSmallFishesNearToPlayer(fishObject));
                    yield return null; // Yield after each fish to avoid performance spikes
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "SmallFish" || other.gameObject.tag == "EscapeSmallFish" || other.gameObject.tag == "DoubleAttackSmallFish")
            {
#if UNITY_EDITOR
                Debug.Log("smallfish enters");
#endif
                StartCoroutine(DeactiveSmallFishAndPushBackToPool(other.gameObject));
            }
        }

        private IEnumerator DeactiveSmallFishAndPushBackToPool(GameObject _fishObject)
        {
            // Move the fish to the shark's mouth position
            _fishObject.transform.position = _eatingPosition.position;
            _fishObject.transform.SetParent(_eatingPosition); // Set parent to the shark's mouth
            RotatePlayerTowards(_fishObject.transform);

            // Trigger shark attack animation
            _player.GetComponent<Player>().PlayEatAnimation();

            // Play blood effect
            _player.GetComponent<Player>().EnableBloodEffect();

            if(_fishObject.tag == "DoubleAttackSmallFish")
            {
                yield return new WaitForSeconds(.1f);
                // Trigger shark attack animation
                _player.GetComponent<Player>().PlayEatAnimation();

                // Play blood effect
                _player.GetComponent<Player>().EnableBloodEffect();

                SharkGameManager.Instance.DestroyCount += 1;

                if (SharkGameManager.Instance.DestroyCount == SharkGameManager.Instance.CurrentLevelTargetAmount)
                {
                    _fishObject.transform.parent = null; // Remove from the shark's mouth
                    ObjectPooling.Instance.ClearFishPoolList();
                    yield return new WaitForSeconds(2f);
                    SharkGameManager.Instance.LoadNextLevel();
                }
                // Optionally, you could have a slight delay after the animations if needed
                yield return new WaitForSeconds(.7f); // Wait a bit for the attack animation to play

                // Mark the fish as dead and reset its state
                SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;

                // Return fish to the pool
                _fishObject.transform.parent = null; // Remove from the shark's mouth
                ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

                // Wait before resetting the shark's state
                yield return new WaitForSeconds(1f);

                // Reset shark attack animation and disable blood effect
                _player.GetComponent<Player>().BackToIdleAnimation();
                _player.GetComponent<Player>().DisableBloodEffect();
            }
            else
            {
                SharkGameManager.Instance.DestroyCount += 1;

                if (SharkGameManager.Instance.DestroyCount == SharkGameManager.Instance.CurrentLevelTargetAmount)
                {
                    _fishObject.transform.parent = null; // Remove from the shark's mouth
                    ObjectPooling.Instance.ClearFishPoolList();
                    yield return new WaitForSeconds(2f);
                    SharkGameManager.Instance.LoadNextLevel();
                }
                // Optionally, you could have a slight delay after the animations if needed
                yield return new WaitForSeconds(.25f); // Wait a bit for the attack animation to play

                // Mark the fish as dead and reset its state
                SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;

                // Return fish to the pool
                _fishObject.transform.parent = null; // Remove from the shark's mouth
                ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

                // Wait before resetting the shark's state
                yield return new WaitForSeconds(1f);

                // Reset shark attack animation and disable blood effect
                _player.GetComponent<Player>().BackToIdleAnimation();
                _player.GetComponent<Player>().DisableBloodEffect();
            }
        }

        private void RotatePlayerTowards(Transform target)
        {
            // Calculate the direction to the target
            Vector3 direction = (target.position - _eatingPosition.position).normalized;

            // Calculate the new rotation based on the direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Calculate the rotation step based on the rotation speed and frame time
            float rotationSpeed = .5f; // Adjust the rotation speed as needed
            Quaternion smoothedRotation = Quaternion.Slerp(_sharkRigidBody.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Apply the rotation to the Rigidbody
            _sharkRigidBody.MoveRotation(smoothedRotation);
        }


        private IEnumerator BringSmallFishesNearToPlayer(GameObject _fishObject)
        {
            // Move the fish to the shark's mouth position
            _fishObject.transform.position = _eatingPosition.position;
            _fishObject.transform.SetParent(_eatingPosition); // Set parent to the shark's mouth
            RotatePlayerTowards(_fishObject.transform);

            // Trigger shark attack animation
            _player.GetComponent<Player>().PlayEatAnimation();
            // Play blood effect
            _player.GetComponent<Player>().EnableBloodEffect();

            Debug.Log("BringSmallFishesNearToPlayer");
            Vector3 initialPosition = _sharkRigidBody.position;
            Vector3 targetPosition = _fishObject.transform.position;

            // Calculate the new position using Lerp
            Vector3 newPosition = Vector3.Lerp(_sharkRigidBody.position, targetPosition, .25f * Time.deltaTime);

            // Move the shark's Rigidbody to the new position
            _sharkRigidBody.MovePosition(newPosition);

            if(_fishObject.tag == "DoubleAttackSmallFish")
            {
                yield return new WaitForSeconds(.1f);

                _player.GetComponent<Player>().PlayEatAnimation();

                // Play blood effect
                _player.GetComponent<Player>().EnableBloodEffect();

                SharkGameManager.Instance.DestroyCount += 1;

                if (SharkGameManager.Instance.DestroyCount == SharkGameManager.Instance.CurrentLevelTargetAmount)
                {
                    _fishObject.transform.parent = null; // Remove from the shark's mouth
                    ObjectPooling.Instance.ClearFishPoolList();
                    yield return new WaitForSeconds(2f);
                    SharkGameManager.Instance.LoadNextLevel();
                }

                yield return new WaitForSeconds(.7f);

                // Mark the fish as dead and reset its state
                SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;

                // Return fish to the pool
                ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

                yield return new WaitForSeconds(1f);

                // Reset shark attack animation and disable blood effect
                _player.GetComponent<Player>().BackToIdleAnimation();
                _player.GetComponent<Player>().DisableBloodEffect();
            }
            else
            {
                SharkGameManager.Instance.DestroyCount += 1;

                if (SharkGameManager.Instance.DestroyCount == SharkGameManager.Instance.CurrentLevelTargetAmount)
                {
                    _fishObject.transform.parent = null; // Remove from the shark's mouth
                    ObjectPooling.Instance.ClearFishPoolList();
                    yield return new WaitForSeconds(1f);
                    SharkGameManager.Instance.LoadNextLevel();
                }

                yield return new WaitForSeconds(.25f);

                // Mark the fish as dead and reset its state
                SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;

                // Return fish to the pool
                ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

                yield return new WaitForSeconds(1f);

                // Reset shark attack animation and disable blood effect
                _player.GetComponent<Player>().BackToIdleAnimation();
                _player.GetComponent<Player>().DisableBloodEffect();
            }
          
        }

        public IEnumerator SmallFishNearToSharkCoroutine(GameObject _fishObject)
        {
            RotatePlayerTowards(_fishObject.transform);

            // Trigger shark attack animation
            _player.GetComponent<Player>().PlayEatAnimation();
            // Play blood effect
            _player.GetComponent<Player>().EnableBloodEffect();

            SharkGameManager.Instance.DestroyCount += 1;

            if (SharkGameManager.Instance.DestroyCount == SharkGameManager.Instance.CurrentLevelTargetAmount)
            {
                _fishObject.transform.parent = null; // Remove from the shark's mouth
                ObjectPooling.Instance.ClearFishPoolList();
                yield return new WaitForSeconds(1f);
                SharkGameManager.Instance.LoadNextLevel();
            }

            yield return new WaitForSeconds(.25f);

            // Mark the fish as dead and reset its state
            SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;

            // Return fish to the pool
            _fishObject.transform.parent = null;
            ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

            yield return new WaitForSeconds(1f);

            // Reset shark attack animation and disable blood effect
            _player.GetComponent<Player>().BackToIdleAnimation();

            _player.GetComponent<Player>().DisableBloodEffect();
        }

        public void SmallFishNearToShark(GameObject _fishObject)
        {
            StartCoroutine(SmallFishNearToSharkCoroutine(_fishObject));
        }
    }
}
