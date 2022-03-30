using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Bat : LivingEntity
{

    //--- Private Variables ---
    private bool _flying = false;
    private List<Vector3> _path = new List<Vector3>();
    private Transform _target;
    private float _attackTimer = 0;

    //--- Components ---
    private Rigidbody2D _rigidbody;
    private Animator _animator;
    private SpriteRenderer _renderer;

    //--- Editor Parameters ---
    [Header("A.I. Settings")]
    [SerializeField] [Tooltip("The speed at which the enemy will move.")] private float _speed = 1.0f;
    [SerializeField] [Tooltip("How close does the enemy need to get to a point on the path before continuing.")] private float _threshold = 0.2f;
    [SerializeField] [Tooltip("The delay (in seconds) before this enemy can attack again.")] private float _attackCooldown = 1f;
    [SerializeField] [Tooltip("How much damage does this enemy deal.")] private int _damage = 1;
    
    //--- Unity methods ---
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _target = GameManager.Instance.Player.transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<PlatformEffector2D>() != null)
        {
            Physics2D.IgnoreCollision(collision.otherCollider, collision.collider);
        }
    }
    private void FixedUpdate()
    {

        //Attack handling
        if ((_attackTimer -= Time.fixedDeltaTime) <= 0)
        {
            _attackTimer = 0;
            List<ContactPoint2D> contacts = new List<ContactPoint2D>();
            _rigidbody.GetContacts(contacts);
            for (int i = 0; i < contacts.Count; i++)
            {
                if(contacts[i].collider.gameObject == _target.gameObject)
                {
                    _attackTimer = _attackCooldown;
                    contacts[i].collider.gameObject.GetComponent<PlayerBehaviour>().TakeDamage(_damage);
                    //Move a tile or two away after attacking
                    Vector2 direction = (this.transform.position - contacts[i].collider.transform.position).normalized;
                    _path = GameManager.Instance.Pathfinder.RequestAirPath(this.transform.position, this.transform.position + (Vector3)direction * 2);
                }
            }
        }
        //If player walks under bat, start flying
        if (!_flying && _target.position.y <= this.transform.position.y && Mathf.Floor(_target.position.x) == Mathf.Floor(this.transform.position.x))
        {
            _flying = true;
            _animator.SetBool("Flying", true);
        }

        if (_flying)
        {
            //Generate a new path after completing our last one
            if (_path.Count <= 1)
                _path = GameManager.Instance.Pathfinder.RequestAirPath(this.transform.position, _target.position);
            else
            {
                //Remove a point after we've reached it
                if (Vector2.Distance(this.transform.position,_path[1]) <= _threshold)
                {
                    _path = GameManager.Instance.Pathfinder.RequestAirPath(this.transform.position, _target.position);
                }
                else
                {
                    //Move towards our next point, and flip our sprite if needed.
                    _rigidbody.velocity = ((Vector2)(_path[1] - this.transform.position)).normalized * _speed;
                    if (_rigidbody.velocity.x != 0 && _renderer != null)
                        transform.localScale = Vector3.one + (_rigidbody.velocity.x > 0 ? Vector3.left * 2 : Vector3.zero);
                }
            }
        }
    }

    //--- Inherited Methods ---
    public override void Die()
    {
        base.Die();
        Destroy(this.gameObject);
    }

}
