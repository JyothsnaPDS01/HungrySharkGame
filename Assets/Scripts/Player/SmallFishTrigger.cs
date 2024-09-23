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

        [SerializeField] private Player _player;
        [SerializeField] private Transform _playerShark;

        [SerializeField] private float detectionInterval;
        [SerializeField] private float detectionRadius;
        [SerializeField] private LayerMask fishLayerMask;

        private void Start()
        {
            //StartCoroutine(CheckNearbyFishesAtIntervals());
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
            float duration = .1f;
            float elapsedTime = 0f;

            // Trigger shark attack animation
            _player.PlayEatAnimation();

            // Move the fish to the shark's mouth position
            while (elapsedTime < duration)
            {
                _fishObject.transform.localPosition = Vector3.Lerp(_fishObject.transform.localPosition, _sharkMouthPosition.localPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

           

            _fishObject.transform.position = _sharkMouthPosition.localPosition;

            _fishObject.transform.SetParent(_sharkMouthPosition);

           

            // Play blood effect
            _player.EnableBloodEffect();

            yield return new WaitForSeconds(.7f);

            // Mark the fish as dead and reset its state
            SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;
            //_fishObject.GetComponent<SmallFish>()._currentState = SharkGameDataModel.SmallFishFiniteState.Die;
            //_fishObject.GetComponent<SmallFish>().ResetFishState();

            // Return fish to the pool
            _fishObject.transform.parent = null;
            ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

            yield return new WaitForSeconds(.5f);

            // Reset shark attack animation and disable blood effect
            _player.BackToIdleAnimation();

            _player.DisableBloodEffect();
        }


        private IEnumerator BringSmallFishesNearToPlayer(GameObject _fishObject)
        {
            float duration = .1f;
            float elapsedTime = 0f;

            // Move the fish to the shark's mouth position
            while (elapsedTime < duration)
            {
                _fishObject.transform.localPosition = Vector3.Lerp(_fishObject.transform.localPosition, _sharkMouthPosition.localPosition, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _fishObject.transform.position = _sharkMouthPosition.localPosition;
            _fishObject.transform.SetParent(_sharkMouthPosition);

            // Trigger shark attack animation
            _player.PlayEatAnimation();

            // Play blood effect
            _player.EnableBloodEffect();

            yield return new WaitForSeconds(.7f);

            // Mark the fish as dead and reset its state
            SharkGameDataModel.SmallFishType fishType = _fishObject.GetComponent<SmallFish>()._smallFishType;
            //_fishObject.GetComponent<SmallFish>()._currentState = SharkGameDataModel.SmallFishFiniteState.Die;
            //_fishObject.GetComponent<SmallFish>().ResetFishState();

            // Return fish to the pool
            _fishObject.transform.parent = null;
            ObjectPooling.Instance.ReturnToPool(_fishObject, fishType);

            yield return new WaitForSeconds(.5f);

            // Reset shark attack animation and disable blood effect
            _player.BackToIdleAnimation();

            _player.DisableBloodEffect();
        }
        private void OnCollisionExit(Collision collision)
        {

        }
    }
}
