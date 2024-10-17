using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharkGame.Models;

namespace SharkGame
{
    public class BombScript : MonoBehaviour
    {
        #region Private Variables
        public float moveAmount = 2f;   // How far up and down the object moves
        public float moveSpeed = 2f;    // How fast the object moves

        private float originalY;        // Starting Y position of the object
        private bool movingUp = true;   // Direction flag

        [SerializeField] private SharkGameDataModel.BombType _bombType;
        private Transform _spawnPosition;
        #endregion

        #region MonoBehaviour Methods
        void Start()
        {
            originalY = transform.position.y;  // Store the initial Y position
        }

        void Update()
        {
            Vector3 position = transform.position;

            // Move up and down between (originalY - moveAmount) and (originalY + moveAmount)
            if (movingUp)
            {
                position.y += moveSpeed * Time.deltaTime;
                if (position.y >= originalY + moveAmount)
                {
                    position.y = originalY + moveAmount;
                    movingUp = false;
                }
            }
            else
            {
                position.y -= moveSpeed * Time.deltaTime;
                if (position.y <= originalY - moveAmount)
                {
                    position.y = originalY - moveAmount;
                    movingUp = true;
                }
            }

            transform.position = position; // Apply the new position
        }
        #endregion

        public SharkGameDataModel.BombType GetBombType()
        {
            return _bombType;
        }

        public void SetPosition(Transform position)
        {
            _spawnPosition = position;
        }

        public Transform GetSpawnPosition()
        {
            return _spawnPosition;
        }

        #region Collision

        #endregion
    }
}
