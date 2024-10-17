using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SharkGame
{
    public class CoinSpawnManager : MonoBehaviour
    {
        [Header("Coin Prefab")]
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private Transform coinParent;
        [Header("CoinStartPosition")]
        [SerializeField] private Transform coinStartPosition;
        [Header("CoinEndPosition")]
        [SerializeField] private Transform coinEndPosition;
        [Header("Coin Amount")]
        [SerializeField] private int coinAmount;
        [Header("Coin Per Delay")]
        [SerializeField] private float coinDelay;

        [SerializeField] private Ease moveEase;

        public void SpawnCoins()
        {
            for(int i=0;i<coinAmount;i++)
            {
                var targetDelay = i * coinDelay;
                ShowCoin(targetDelay);
            }
        }

        private void ShowCoin(float delay)
        {
            GameObject coinObject = Instantiate(coinPrefab, coinParent);
            coinObject.transform.position = coinStartPosition.transform.position;
            coinObject.transform.DOMove(coinEndPosition.transform.position, 1f).SetEase(moveEase).SetDelay(delay).OnComplete(() =>
            {
                Destroy(coinObject);
            });
               
        }
    }
}
