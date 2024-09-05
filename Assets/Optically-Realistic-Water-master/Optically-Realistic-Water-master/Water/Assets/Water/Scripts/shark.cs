using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shark : MonoBehaviour
{
    public float speed = 5f;
    [SerializeField] private FixedJoystick joystick;
   
    float rotationSpeed = 5f;
    float moveSpeed = 2f;
    void Start()
    {
      //  StartCoroutine(InitialSharkMovement());
    }

    void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
      
        float horizontal = joystick.Horizontal;
        float vertical = joystick.Vertical;

        // Calculate rotation angle
        Vector3 direction = new Vector3(horizontal, vertical, 0);
            if (direction.magnitude > 0)
            {
                Vector3 movement = direction.normalized * moveSpeed * Time.deltaTime;
                transform.Translate(movement, Space.World);
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        
    }

    //private IEnumerator InitialSharkMovement()
    //{
    //    float elapsedTime = 0f;
    //    Quaternion initialRotation = body.rotation;
    //    Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
    //    Vector3 initialPosition = body.position;
    //    Vector3 targetPosition = initialPosition + new Vector3(0, -2, 0); // Move in Y-axis to keep movement in XY plane

    //    while (elapsedTime < 2f)
    //    {
    //        float t = elapsedTime / 2f;
    //        body.position = Vector3.Lerp(initialPosition, targetPosition, t);
    //        body.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

    //        elapsedTime += Time.deltaTime;
    //        yield return null;
    //    }

    //    body.position = targetPosition;
    //    body.rotation = targetRotation;
    //}
}
