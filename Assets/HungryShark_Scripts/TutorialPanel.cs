using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SharkGame
{
    public class TutorialPanel : MonoBehaviour
    {
        public void StartTimer()
        {
            Debug.Log("StartTimer");

            StartCoroutine(DelayToTurnTheTimer());

            IEnumerator DelayToTurnTheTimer()
            {
                yield return new WaitForSeconds(5f);
                SharkGameManager.Instance.StartHealthTimer();
            }
        }
    }
}
