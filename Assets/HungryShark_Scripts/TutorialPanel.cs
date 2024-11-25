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
                yield return new WaitForSeconds(3f);
                SharkGameManager.Instance.StartHealthTimer();
            }
        }
    }
}
