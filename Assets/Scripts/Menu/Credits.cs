using UnityEngine;

public class Credits : MonoBehaviour
{
    //--- Editor Parameters ---
    [Header("Credit Settings")]
    [SerializeField] [Tooltip("Game objects to show.")] private GameObject[] _show;
    [SerializeField] [Tooltip("Game objects to hide.")] private GameObject[] _hide;
    [SerializeField] [Tooltip("Game objects to move.")] private GameObject[] _move;
    [SerializeField] [Tooltip("Speed at which credits should scroll.")] private float _speed = 1.0f;

    //--- Private Variables ---
    private bool _creditsEnabled = false;

    //--- Public Methods ---
    public void ToggleCredits()
    {
        _creditsEnabled = !_creditsEnabled;
        MenuManager.Instance.MovementEnabled = !_creditsEnabled;
        foreach (GameObject obj in _move)
        {
            obj.transform.position = -Vector3.up*7;
        }
        foreach (GameObject obj in _hide)
        {
            obj.SetActive(!_creditsEnabled);
        }
        foreach (GameObject obj in _show)
        {
            obj.SetActive(_creditsEnabled);
        }
    }

    //--- Unity Methods ---
    void Update()
    {
        if (_creditsEnabled)
            foreach(GameObject obj in _move)
            {
                if (obj.transform.position.y < float.MaxValue - _speed)
                    obj.transform.position = obj.transform.position + (Vector3.up * _speed) * Time.deltaTime;
            }
    }
}
