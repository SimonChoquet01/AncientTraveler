using UnityEngine;
using UnityEngine.Events;

public class MenuItem : MonoBehaviour
{

    //--- Editor Parameters ---
    [Header("Menu Item")]
    [SerializeField] [Tooltip("The menu item above this one.")] private MenuItem _above;
    [SerializeField] [Tooltip("The menu item below this one.")] private MenuItem _below;
    [SerializeField] [Tooltip("What happens when this item is hovered.")] private UnityEvent _onFocus;
    [SerializeField] [Tooltip("What happens when this item is selected.")] private UnityEvent _onSelect;

    //--- Public Methods ---

    public void OnFocused()
    {
        _onFocus.Invoke();
    }

    public void OnSelected()
    {
        _onSelect.Invoke();
    }

    //--- Public Fields ---
    public Vector2 Position
    {
        get => this.transform.position;
    }
    public MenuItem Above { get => _above; set => _above = value; }
    public MenuItem Below { get => _below; set => _below = value; }
}
