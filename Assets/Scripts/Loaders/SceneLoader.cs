using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    //--- Editor Parameters ---
    [Header("Loader Settings")]
    [SerializeField] [Tooltip("Automatically change scene once loaded?")] private bool _changeScene = false;
    [SerializeField] [Tooltip("The text fields to update. (Optional)")] private TextMeshProUGUI[] _textFields;
    [SerializeField] [Tooltip("The text to display when complete. (Also Optional)")] private string _completionText = "Complete!";

    //--- Private Variables ---
    private AsyncOperation _async;

    //--- Unity Methods ---
    void Start()
    {
        _async = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        _async.allowSceneActivation = _changeScene;
    }

    void Update()
    {
        if (_textFields.Length > 0)
        {
            for (int i = 0; i < _textFields.Length; i++)
            {
                if (_async.progress < 0.9f)
                {
                    _textFields[i].text = Mathf.FloorToInt(_async.progress * 100.0f).ToString();
                }
                else
                {
                    _textFields[i].text = _completionText;
                }
            }
        }
    }

    //--- Public Methods ---
    public void ChangeScene()
    {
        if (_async.progress >= 0.9f)
        _async.allowSceneActivation = true;
    }

}
