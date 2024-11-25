using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public GameObject buttonObject;
    public void ResetScale()
    {
        transform.localScale = Vector3.one;
    }
}
