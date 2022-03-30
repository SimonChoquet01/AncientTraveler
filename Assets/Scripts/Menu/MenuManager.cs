using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{

    //--- Editor Parameters ---
    [Header("Required Fields")]
    [SerializeField] [Tooltip("The game object that moves to our selection.")] private Transform _cursor;
    [SerializeField] [Tooltip("The currently selected menu item.")] private MenuItem _selected;
    [SerializeField] [Tooltip("The menu items in the menu.")] private MenuItem[] _items;
    [SerializeField] [Tooltip("The currently used camera.")] private Camera _camera;
    [SerializeField] [Tooltip("The area the mouse must be in to reach the buttom.")] private float _buttonRadius = 1.0f;
    [Tooltip("Allow the player to change menu selection.")] public bool MovementEnabled = true;
    [Tooltip("Allow the player to make a selection.")] public bool SelectionEnabled = true;

    //--- Private Variables ---
    private Coroutine _moveRoutine;
    
    //--- Unity Methods ---
    private void FixedUpdate()
    {
        if (Input.mousePresent)
        {
            Vector2 position = _camera.ScreenToWorldPoint(Input.mousePosition);
            for (int i = 0; i < _items.Length; i++)
            {
                if ( MovementEnabled && _selected != _items[i] && _moveRoutine == null && Vector2.Distance(position,_items[i].Position) < _buttonRadius)
                    _moveRoutine = StartCoroutine(menuChangeRoutine(_items[i]));      
            }
        }
    }
    private void OnStart()
    {
        _cursor.position = _selected.Position;
    }
    private void OnValidate()
    {
        if (_items.Length == 0)
            _items = GetComponentsInChildren<MenuItem>();
        if (_selected == null && _items.Length > 0)
            _selected = _items[0];
    }

    //--- Singleton Behavior ---

    private static MenuManager _instance = null;

    private MenuManager() { }

    public static MenuManager Instance { get => _instance;}

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(this);
    }

    //--- Public Methods ---
    public void Quit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    //--- Input System Events ---

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!MovementEnabled) return;
        Vector2 movement = context.ReadValue<Vector2>();
        if (_moveRoutine == null && Mathf.Abs(movement.y) > 0.5)
        {
            if (movement.y > 0 && _selected.Above != null)
                _moveRoutine = StartCoroutine(menuChangeRoutine(_selected.Above));
            else if (movement.y < 0 && _selected.Below != null)
                _moveRoutine = StartCoroutine(menuChangeRoutine(_selected.Below));
        }
    }
    public void OnUse(InputAction.CallbackContext context)
    {
        if (!SelectionEnabled) return;
        if (_moveRoutine == null) _selected.OnSelected();
    }

    //--- Coroutines ---
    private IEnumerator menuChangeRoutine(MenuItem newItem, float time = 0.25f, int intervals = 15)
    {
        Vector2 originalPosition = _selected.Position;
        Vector2 newPosition = newItem.Position;
        for (int i = 0; i <= intervals; i++)
        {
            _cursor.position = Vector2.Lerp(originalPosition, newPosition, i / (float)intervals);
            yield return new WaitForSecondsRealtime(time / intervals);
        }
        _selected = newItem;
        _selected.OnFocused();
        _moveRoutine = null;
    }

}
