using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SharkGame
{
    public class MainMenuUIController : MonoBehaviour
    {
        #region Private Variables
        [SerializeField] private GameObject _subscriptionPage;
        [SerializeField] private GameObject _mainMenuPanel;
        #endregion

        #region Button Actions

        public void SubscriptionButtonClick()
        {
            _subscriptionPage.SetActive(false);
            _mainMenuPanel.SetActive(true);
        }

        public void PlayButtonClick()
        {
            _mainMenuPanel.SetActive(false);
            SceneManager.LoadScene("OurWaterScene");
        }

        #endregion
    }
}
