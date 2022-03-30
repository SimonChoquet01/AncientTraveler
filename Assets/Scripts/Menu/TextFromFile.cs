using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextFromFile : MonoBehaviour
{
    //--- Editor Parameters ---
    [SerializeField] [Tooltip("The text asset to load from.")] private TextAsset _textAsset;
    
    //--- Unity Methods ---
    void Start()
    {
        if (GetComponent<TextMeshProUGUI>() != null)
            GetComponent<TextMeshProUGUI>().text = _textAsset.text;
    }

}
