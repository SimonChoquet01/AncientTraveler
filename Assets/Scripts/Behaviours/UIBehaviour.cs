using System.Collections;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class UIBehaviour : MonoBehaviour
{
    //--- Editor Parameters ---
    [Header("Components")]
    [SerializeField][Tooltip("The guard stamina bar that will be moved to player.")] private Image _guardBar;
    [SerializeField][Tooltip("The health bar on screen space.")] private Image _healthBar;
    [SerializeField] [Tooltip("Ammo text fields.")] private TextMeshProUGUI[] _ammoTextFields;
    
    //--- Private Variables ---
    private Coroutine _guardCoroutine;
    private Coroutine _healthCoroutine;
    private Coroutine _hideTextFields;

    //--- Unity Methods ---
    private void Start()
    {
    }

    //--- Custom Methods ---

    public void UpdateAmmo(int count)
    {
        if (_hideTextFields != null)
            StopCoroutine(_hideTextFields);
        foreach(TextMeshProUGUI text in _ammoTextFields)
        {
            text.text = count.ToString();
            text.enabled = true;
        }
        _hideTextFields = StartCoroutine(hideAmmo());
    }
    public void UpdateHealth(float remaining, float maximum)
    {
        if (_healthCoroutine != null)
            StopCoroutine(_healthCoroutine);
        _healthCoroutine = StartCoroutine(barFill(_healthBar, remaining, maximum));
    }

    public void UpdateGuard(float remaining, float maximum, bool display = true,  bool instant = false)
    {
        if (_guardCoroutine != null)
            StopCoroutine(_guardCoroutine);
        if (!instant)
            _guardCoroutine = StartCoroutine(barFill(_guardBar, remaining, maximum));
        else
            _guardBar.GetComponent<RectTransform>().localScale = new Vector3(remaining / maximum, 1, 1);
        _guardBar.transform.parent.GetComponent<Image>().enabled = display;
        _guardBar.enabled = display;
    }

    //--- Coroutines ---
    private IEnumerator barFill(Image fillBar, float num, float max, float time = 0.25f, int intervals = 15)
    {
        RectTransform rect = fillBar.GetComponent<RectTransform>();
        float startWidth = rect.localScale.x;
        float endWidth = num / max;
        float value = 0;
        for (int i = 0; i < intervals; i++)
        {
            if (i == intervals - 1)
                value = endWidth;
            else
                value = startWidth + ((endWidth - startWidth) / intervals) * i;
            rect.localScale = new Vector3(value, 1, 1);
            yield return new WaitForSeconds(time / intervals);
        }
    }

    private IEnumerator hideAmmo(float seconds = 1.5f)
    {
        yield return new WaitForSeconds(seconds);
        foreach(TextMeshProUGUI TMP_GUI in _ammoTextFields)
        {
            TMP_GUI.enabled = false;
        }
    }
}
