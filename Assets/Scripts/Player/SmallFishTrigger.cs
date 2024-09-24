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
        [SerializeField] private Transform _playerShark;

        [SerializeField] private float detectionInterval;
        [SerializeField] private float detectionRadius;
        [SerializeField] private LayerMask fishLayerMask;

        private void Start()
        {
            StartCoroutine(CheckNearbyFishesAtIntervals());
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
            Collider[] nearbyFishes = Physics.OverlapSphere(_playerShark.position, detectionRadius, fishLayerMask);

            foreach (Collider fishCollider in nearbyFishes)
            {
                if (fishCollider.CompareTag("SmallFish") && fishCollider.isTrigger)
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
            if (other.gameObject.tag == "SmallFish")
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
            _fishObject.transform.position = _playerShark.position;
            _fishObject.transform.SetParent(_playerShark); // Set parent to the shark's mouth
            RotatePlayerTowards(_fishObject.transform);

            // Trigger shark attack animation
            _player.GetComponent<Player>().PlayEatAnimation();

            // Play blood effect
            _player.GetComponent<Player>().EnableBloodEffect();

            // Optionally, you could have a slight delay after the animations if needed
            yield return new WaitForSeconds(0.2f); // Wait a bit for the attack animation to play

            // Mark the fish as dead and reset its state
            SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;

            // Return fish to the pool
            _fishObject.transform.parent = null; // Remove from the shark's mouth
            ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

            // Wait before resetting the shark's state
            yield return new WaitForSeconds(0.5f);

            // Reset shark attack animation and disable blood effect
            _player.GetComponent<Player>().BackToIdleAnimation();
            _player.GetComponent<Player>().DisableBloodEffect();
        }

        private void RotatePlayerTowards(Transform target)
        {
            // Calculate the direction to the target
            Vector3 direction = (target.position - _playerShark.position).normalized;

            // Calculate the rotation step based on the speed and frame time
            float step = 5f * Time.deltaTime; // Adjust the speed as needed

            // Calculate the new rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            _player.transform.rotation = Quaternion.Slerp(_playerShark.rotation, targetRotation, step);
        }

        private IEnumerator BringSmallFishesNearToPlayer(GameObject _fishObject)
        {
            _fishObject.transform.position = _playerShark.position;
            _fishObject.transform.SetParent(_playerShark);
            RotatePlayerTowards(_fishObject.transform);

            yield return new WaitForSeconds(0.5f);

            // Trigger shark attack animation
            _player.GetComponent<Player>().PlayEatAnimation();
            // Play blood effect
            _player.GetComponent<Player>().EnableBloodEffect();

            // Mark the fish as dead and reset its state
           SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;

            // Return fish to the pool
            _fishObject.transform.parent = null;
            ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

            yield return new WaitForSeconds(.5f);

            // Reset shark attack animation and disable blood effect
            _player.GetComponent<Player>().BackToIdleAnimation();

            _player.GetComponent<Player>().DisableBloodEffect();
        }
    }
}
