using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Script;
using DG.Tweening;
public class ButtonHighlighter : MonoBehaviour
{
    private Button previousButton;
    [SerializeField] private float scaleAmount = 1.1f;
    public GameObject defaultButton;
    public GameObject singleplaybtn;

    public GameObject _defaultImageParent;
    private void Awake()
    {
        if (!AndroidTV.IsAndroidOrFireTv())
        {
            this.gameObject.GetComponent<ButtonHighlighter>().enabled = false;
        }
    }
    void Start()
    {
        if (defaultButton != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultButton);
        }

        //if (!AndroidTV.IsAndroidOrFireTv())
        //{
        //    if (_defaultImageParent != null)
        //    {
        //        AnimateDefaultButton(_defaultImageParent);
        //    }
        //}
    }
    private void OnEnable()
    {
        if (defaultButton != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultButton);
        }
    }
    void Update()
    {
        var selectedObj = EventSystem.current.currentSelectedGameObject;
#if UNITY_EDITOR
#endif
        if (selectedObj == null) return;
        var selectedAsButton = selectedObj.GetComponent<Button>();
        if (selectedAsButton != null && selectedAsButton != previousButton)
        {
            if (selectedAsButton.transform.name != "PauseButton")
                HighlightButton(selectedAsButton);
        }
        if (previousButton != null && previousButton != selectedAsButton)
        {
            UnHighlightButton(previousButton);
        }
        previousButton = selectedAsButton;

       
    }
    public static GameObject FindGameObjectInChildWithTag(GameObject parent, string tag)
    {
        Transform t = parent.transform;
        for (int i = 0; i < t.childCount; i++)
        {
            if (t.GetChild(i).gameObject.tag == tag)
            {
                return t.GetChild(i).gameObject;
            }
        }
        return null;
    }
    public void HighlightButton(Button butt)
    {
        butt.transform.localScale = new Vector3(scaleAmount, scaleAmount, scaleAmount);
        if (butt.transform.tag == "Giveborder")
        {
            Debug.Log("Button Name" + butt.gameObject);
            FindGameObjectInChildWithTag(butt.gameObject, "border").SetActive(true);
        }
        if (butt.GetComponent<Outline>() != null)
        {
            butt.GetComponent<Outline>().enabled = true;
        }
    }
    void UnHighlightButton(Button butt)
    {
        butt.transform.localScale = new Vector3(1, 1, 1);
        if (butt.transform.tag == "Giveborder")
        {
            FindGameObjectInChildWithTag(butt.gameObject, "border").SetActive(false);
        }
        if (butt.GetComponent<Outline>() != null)
        {
            butt.GetComponent<Outline>().enabled = false;
        }
    }

    public void SetDefaultButton(GameObject _defaultButton)
    {
        defaultButton = _defaultButton;
    }

    //private void AnimateDefaultButton(GameObject button)
    //{
    //    Debug.Log("AnimateDefaultButton in ButtonHightlighter");
    //    button.transform.localScale = Vector3.one;

    //    button.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), .5f)
    //       .SetLoops(-1, LoopType.Yoyo)  // Loop indefinitely with a "yoyo" effect
    //       .SetEase(Ease.InOutSine);     
    //}

    //private void RepeatAnimateDefaultButton()
    //{
    //    if (_defaultImageParent != null)
    //    {
    //        //AnimateDefaultButton(_defaultImageParent);
    //    }
    //}

    public void SetDefaultParentForTab(GameObject defaultParent)
    {
        _defaultImageParent = defaultParent;
        //AnimateDefaultButton(_defaultImageParent);
    }

    //private IEnumerator AnimateDefaultButtonwithDelay()
    //{
    //    yield return new WaitForSeconds(.1f);
    //    //AnimateDefaultButton(_defaultImageParent);
    //}
}