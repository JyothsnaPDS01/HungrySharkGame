using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Script;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] private GameObject _dayButton;

    void Start()
    {
        if (!AndroidTV.IsAndroidOrFireTv())
        {
            if (_dayButton != null)
            {
                if (_dayButton.activeInHierarchy) AnimateDefaultButton();
            }
        }
        //SwitchButtonTransitions();
    }

    private void SwitchButtonTransitions()
    {
        if (!AndroidTV.IsAndroidOrFireTv())
        {
            _dayButton.GetComponent<Button>().transition = Selectable.Transition.Animation;
        }
        else if (AndroidTV.IsAndroidOrFireTv())
        {
            _dayButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
        }
    }

    private void Update()
    {
        
    }

    public void AnimateDefaultButton()
    {
        Debug.Log("AnimateDefaultButton");
        this.transform.localScale = Vector3.one;

        this.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 1.5f)
           .SetLoops(-1, LoopType.Yoyo)  // Loop indefinitely with a "yoyo" effect
           .SetEase(Ease.InOutSine);
    }

    public void KillButtonAnimation()
    {
        if (!AndroidTV.IsAndroidOrFireTv())
        {
            this.transform.DOKill();
            this.transform.localScale = Vector3.one;
        }
    }


}
