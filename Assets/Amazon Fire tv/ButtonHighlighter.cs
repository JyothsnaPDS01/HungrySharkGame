using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Script;
public class ButtonHighlighter : MonoBehaviour
{
    private Button previousButton;
    [SerializeField] private float scaleAmount = 1.1f;
    public GameObject defaultButton;
    public GameObject singleplaybtn;
    private void Awake()
    {
        if (!AndroidTV.IsAndroidOrFireTv())
        {
           // this.gameObject.GetComponent<ButtonHighlighter>().enabled = false;
        }
    }
    void Start()
    {
        if (defaultButton != null)
        {
            EventSystem.current.SetSelectedGameObject(defaultButton);
        }
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
    void HighlightButton(Button butt)
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
}