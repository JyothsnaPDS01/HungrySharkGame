using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Script;

public class ButtonAnimation : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private GameObject _dayButton;

    void Start()
    {
        
    }

    private void Update()
    {
        if (!AndroidTV.IsAndroidOrFireTv())
        {
            if (_dayButton != null)
            {
                AnimateDefaultButton();
            }
        }
    }

    public void AnimateDefaultButton()
    {
        Debug.Log("AnimateDefaultButton");
        this.transform.localScale = Vector3.one;

        this.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), .5f)
           .SetLoops(-1, LoopType.Yoyo)  // Loop indefinitely with a "yoyo" effect
           .SetEase(Ease.InOutSine);
    }
}
