using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    //--- Editor Parameters ---
    [Header("Required  Fields")]
    [Tooltip("The player")] public PlayerBehaviour Player = null;
    [Tooltip("The UI behaviour script the player refers to.")] public UIBehaviour UI = null;
    [Tooltip("The pathfinding script enemies will refer to.")] public Pathfinding Pathfinder = null;
    [Tooltip("The camera script the player will refer to.")] public CameraBehaviour Camera = null;

    //--- Public Fields ---
    [HideInInspector] public bool Paused = true;

    //--- Public Methods ---
    public void TogglePause()
    {
        Paused = !Paused;
        Time.timeScale = (Paused ? 0 : 1);
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.gameObject.SetActive(Paused);
            MenuManager.Instance.SelectionEnabled = Paused;
            MenuManager.Instance.MovementEnabled = Paused;
        }
    }

    //--- Unity Methods ---
    private void Start()
    {
        TogglePause();
    }
    private void OnValidate()
    {
        if (Player == null)
        {
            foreach (PlayerBehaviour ply in FindObjectsOfType(typeof(PlayerBehaviour)))
            {
                Player = ply;
            }
        }
        if (UI == null)
        {
            foreach (UIBehaviour ui in FindObjectsOfType(typeof(UIBehaviour)))
            {
                UI = ui;
            }
        }
        if (Pathfinder == null)
        {
            foreach (Pathfinding pf in FindObjectsOfType(typeof(Pathfinding)))
            {
                Pathfinder = pf;
            }
        }
        if (Camera == null)
        {
            foreach (CameraBehaviour cam in FindObjectsOfType(typeof(CameraBehaviour)))
            {
                Camera = cam;
            }
        }
        if (UI == null || Player == null || Pathfinder == null || Camera == null) Debug.LogWarning("Please fill out the game manager's required fields!");
    }

    //--- Singleton Behavior ---

    private static GameManager _instance = null;

    private GameManager() { }

    public static GameManager Instance { get => _instance;}

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else if (_instance != this)
            Destroy(this);
    }

}
