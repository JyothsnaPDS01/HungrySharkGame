using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(float deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;

    private float oldPosition;

    void Start()
    {
        oldPosition = transform.position.x;
    }

    void Update()
    {
        if (transform.position.x != oldPosition)
        {
            if (onCameraTranslate != null)
            {
                float Xdelta = oldPosition - transform.position.x;
                onCameraTranslate(Xdelta);
            }

            oldPosition = transform.position.x;
        }
    }
}
