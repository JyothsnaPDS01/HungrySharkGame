using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Script;

namespace SharkGame
{
    public class ResetButton : MonoBehaviour
    {
        public GameObject buttonObject;

        private void Start()
        {
            if (AndroidTV.IsAndroidOrFireTv())
            {
                buttonObject.GetComponent<Animator>().enabled = false;
            }
        }
        public void ResetScale()
        {
            transform.localScale = Vector3.one;
        }
    }
}
