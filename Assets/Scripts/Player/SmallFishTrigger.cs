using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;
using DG.Tweening;


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

        [SerializeField] private float cooldownDuration = 0.02083333f; // Cooldown duration in seconds
        [SerializeField] private bool isOnCooldown = false; // Flag to check if the shark is on cooldown

        public bool IsOnCoolDown
        {
            set { isOnCooldown = value; }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("SmallFish") || other.gameObject.CompareTag("EscapeSmallFish") || other.gameObject.CompareTag("DoubleAttackSmallFish"))
            {
#if UNITY_EDITOR
                Debug.Log("Small fish enters");
#endif
                if (!isOnCooldown) // Check if the shark is not on cooldown
                {
                    StartCoroutine(DeactiveSmallFishAndPushBackToPool(other.gameObject));
                }
            }
            else if(other.gameObject.CompareTag("Bomb"))
            {
                TriggerShake();
            }
        }

        private IEnumerator DeactiveSmallFishAndPushBackToPool(GameObject _fishObject)
        {
            isOnCooldown = true; // Set cooldown flag

            // Move the fish to the shark's mouth position
            _fishObject.transform.position = _eatingPosition.position;
            RotatePlayerTowards(_fishObject.transform);

            // Trigger shark attack animation
            _player.GetComponent<Player>().PlayEatAnimation();

            // Play blood effect
            _player.GetComponent<Player>().EnableBloodEffect();
            if (_fishObject.tag == "DoubleAttackSmallFish")
            {
                yield return new WaitForSeconds(.1f);

                _player.GetComponent<Player>().PlayEatAnimation();

                // Play blood effect
                _player.GetComponent<Player>().EnableBloodEffect();

                SharkGameManager.Instance.DestroyCount += 1;

                UIController.Instance.MakeMaxHealth();

                UIController.Instance.UpdateKillAmount();

                if (SharkGameManager.Instance.DestroyCount == SharkGameManager.Instance.CurrentLevelTargetAmount)
                {
                    _fishObject.transform.parent = null; // Remove from the shark's mouth
                    ObjectPooling.Instance.ClearFishPoolList();
                    yield return new WaitForSeconds(1f);
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

                // Increment destroy count and update UI
                SharkGameManager.Instance.DestroyCount += 1;
                UIController.Instance.MakeMaxHealth();

                UIController.Instance.UpdateKillAmount();
                _fishObject.transform.parent.GetComponent<FishGroup>().UpdateDestroyCount(1, _fishObject);

                if (SharkGameManager.Instance.DestroyCount == SharkGameManager.Instance.CurrentLevelTargetAmount)
                {
                    _fishObject.transform.parent = null; // Remove from the shark's mouth
                    yield return new WaitForSeconds(1f);
                    SharkGameManager.Instance.LoadNextLevel();
                }

                // Wait a bit for the attack animation to play
                yield return new WaitForSeconds(0.25f);

                // Mark the fish as dead and reset its state
                SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;

                // Wait before resetting the shark's state
                yield return new WaitForSeconds(1f);

                // Reset shark attack animation and disable blood effect
                _player.GetComponent<Player>().BackToIdleAnimation();
                _player.GetComponent<Player>().DisableBloodEffect();
            }

            // Cooldown period before allowing the next fish to be eaten
            yield return new WaitForSeconds(cooldownDuration);
            isOnCooldown = false; // Reset cooldown flag
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

                UIController.Instance.MakeMaxHealth();

                UIController.Instance.UpdateKillAmount();

                if (SharkGameManager.Instance.DestroyCount == SharkGameManager.Instance.CurrentLevelTargetAmount)
                {
                    _fishObject.transform.parent = null; // Remove from the shark's mouth
                    ObjectPooling.Instance.ClearFishPoolList();
                    yield return new WaitForSeconds(1f);
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

                UIController.Instance.UpdateKillAmount();

                UIController.Instance.MakeMaxHealth();

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
                yield return new WaitForSeconds(.5f);
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

        public Camera mainCamera;

        // Shake parameters
        public float shakeDuration = 0.5f;   // How long the camera will shake
        public float shakeStrength = 1f;     // How intense the shaking is
        public int shakeVibrato = 10;        // How many times it shakes in the given duration
        public float randomness = 90f;       // How random the shaking is

        // Call this function to trigger the shake
        public void TriggerShake()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, randomness);
            }
        }
    }
}
