using System;

using UnityEngine;

public class Pickup : MonoBehaviour
{
    //--- Serializable for Editor Dropdown ---
    [Serializable] private enum TYPE
    {
        Health,
        Ammo
    }

    //--- Editor Parameters ---
    [Header("Pickup Options")]
    [SerializeField] private TYPE _type = TYPE.Health;
    [SerializeField] [Tooltip("How much to give the player? (-1 for Maximum)")] private int _ammount = -1;


    //--- Unity Methods ---
    private void OnTriggerEnter2D(Collider2D collider)
    {
        onTouched(collider);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        onTouched(collision.collider);
    }

    //--- Custom Methods ---
    private void onTouched(Collider2D collider)
    {
        if (collider.gameObject.GetComponent<PlayerBehaviour>() == GameManager.Instance.Player)
        {
            switch (_type)
            {
                case TYPE.Health:
                    if (_ammount < 0)
                        GameManager.Instance.Player.Health = GameManager.Instance.Player.MaxHealth;
                    else
                        GameManager.Instance.Player.Health += _ammount;
                    break;

                case TYPE.Ammo:
                    GameManager.Instance.Player.Ammo += Mathf.Abs(_ammount);
                    break;

                default:
                    break;
            }
            Destroy(this.gameObject);
        }
    }

}
