using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent (typeof(CapsuleCollider2D))]
public class PlayerBehaviour : LandDweller
{

    //--- Constants ---
    private const float HALF_OF_ROOT_TWO = 0.7071068f;

    //--- Private Variables ---
    private Vector2 _movement = new Vector2(0, 0);
    private Vector2 _smoothVelocity = new Vector2(0, 0);
    private bool _crouching = false;
    private bool _attacking = false;
    private bool _throwing = false;
    private bool _guarding = false;
    private Vector2 _colliderOffset;
    private Vector2 _colliderSize;
    private float _guardHealth;
    private int _ammo = 0;

    //--- Editor Parameters ---
    [Header("Movement Options")]
    [SerializeField] [Tooltip("The force at which the player jumps.")] private float _jumpForce = 12f;
    [SerializeField] [Tooltip("The speed at which the player walks.")] private float _walkSpeed = 2.5f;

    [Header("Attack Options")]
    [SerializeField] [Tooltip("The attack strenth the player starts with.")] private int _attackDamage = 1;
    [SerializeField] [Tooltip("The size of the area used for attacks.")] private Vector2 _attackArea = Vector2.one;
    [SerializeField] [Tooltip("The prefab of the projectile the player throws.")] private GameObject _projectile;

    [Header("Health & Guard")]
    [SerializeField] [Tooltip("The max health the player starts with.")] private int _maxHealth = 3;
    [SerializeField] [Tooltip("The amount of padding the guard can offer before depleting.")] private float _maxGuardHealth = 3f;

    [Header("Collider")]
    [SerializeField] [Tooltip("The crouched version of the collider's offset.")] private Vector2 _crouchColliderOffset = new Vector2(0, -1);
    [SerializeField] [Tooltip("The crouched version of the collider's size.")] private Vector2 _crouchColliderSize = new Vector2(0.4f, 2);


    //--- Components ---
    private Animator _animator;
    private SpriteRenderer _renderer;

    //--- Public Fields ---
    public int Health 
    {
        get => _health;
        set
        {
            _health = (int)Mathf.Clamp(value,0,_maxHealth);
            GameManager.Instance.UI.UpdateHealth(_health,_maxHealth);
        }
    }
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            _maxHealth = value;
            _health = (int)Mathf.Clamp(_health, 0, _maxHealth);
            GameManager.Instance.UI.UpdateHealth(_health, _maxHealth);
        }
    }

    public int Ammo
    {
        get => _ammo;
        set
        {
            _ammo = value;
            GameManager.Instance.UI.UpdateAmmo(value);
        }
    }

    //--- Unity Methods ---
    new private void Start()
    {
        base.Start();
        _health = _maxHealth;
        _guardHealth = _maxGuardHealth;

        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();

        _colliderOffset = _collider.offset;
        _colliderSize = (_collider as CapsuleCollider2D).size;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector2 boxOffset = (Vector2)this.transform.position + new Vector2((this.GetComponent<SpriteRenderer>().flipX ? -1 : 1) * (this.GetComponent<CapsuleCollider2D>().size.x / 2 + _attackArea.x / 2), this.GetComponent<CapsuleCollider2D>().offset.y + _attackArea.y / 2);
        Gizmos.DrawWireCube(boxOffset, _attackArea);
    }

    private void Update()
    {

        if (_isDead)
        {
            if (_rigidbody.constraints != RigidbodyConstraints2D.FreezeAll && GetGrounded())
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
            return;
        }
        //Regenerate guard
        if (!_guarding)
        {
            if(_guardHealth < _maxGuardHealth)
            {
                if ((_guardHealth += Time.deltaTime) >= _maxGuardHealth)
                {
                    _guardHealth = _maxGuardHealth;
                    GameManager.Instance.UI.UpdateGuard(_guardHealth, _maxGuardHealth,false,true);
                }
                else
                GameManager.Instance.UI.UpdateGuard(_guardHealth, _maxGuardHealth, true, true);
            }
        }

        //Apply movement
        if (_movement.x != 0) _renderer.flipX = _movement.x < 0;
        if (_movement.x != 0 && ((!_crouching && !_attacking && !_guarding) || !_isGrounded))
        {
            _rigidbody.velocity = new Vector2(_movement.x * _walkSpeed, _rigidbody.velocity.y);
            _animator.SetBool("Walking", true);
        }
        else
        {
            _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
            _animator.SetBool("Walking", false);
        }

        //Handle the falling/jumping animation
        _animator.SetFloat("YVelocity", _rigidbody.velocity.y);
        if ((_isGrounded && _rigidbody.velocity.y != 0) || (!_isGrounded && (int)_rigidbody.velocity.y == 0))
            _animator.SetBool("Grounded", GetGrounded());

        //For attack blendtrees
        if (!_isGrounded)
            _animator.SetFloat("AttackHeight", 1);
        else if (_crouching)
            _animator.SetFloat("AttackHeight", -1);
        else
            _animator.SetFloat("AttackHeight", 0);
            
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Go through the platform and keep our velocity
        Rigidbody2D rb = collision.collider.GetComponent<Rigidbody2D>();
        Vector2 velocity = -collision.relativeVelocity;
        if (rb != null) velocity = -(collision.relativeVelocity - rb.velocity);
        if (_crouching && velocity.y < 0 && attemptPassthrough()) _rigidbody.velocity = velocity;
    }

    //--- Inherited Methods ---
    public override void TakeDamage(int damage)
    {
        if (_invincible || _isDead) return;

        //Handle guarding
        if (_guarding)
        {
            if ((_guardHealth -= damage) <= 0)
            {
                _guarding = false;
                _animator.SetBool("Guard", false);
                damage = (int)Mathf.Round(Mathf.Abs(_guardHealth));
                _guardHealth = 0;
            }
            else
            {
                damage = 0;
            }
            GameManager.Instance.UI.UpdateGuard(_guardHealth, _maxGuardHealth);

        }
        if (damage <= 0) return;

        //Handle actually being hit
        _attacking = false;
        _animator.ResetTrigger("Attack");
        _animator.SetTrigger("Hurt");
        if ((Health -= damage) <= 0)
        {
            Health = 0;
            this.Die();
        }
        else
            StartCoroutine(Invincibility());
        GameManager.Instance.Camera.ShakeCam();
    }
    public override void Die()
    {
        if (_isDead) return;
        base.Die();
        _isDead = true;
        _attacking = false;
        _animator.ResetTrigger("Attack");
        _animator.ResetTrigger("Hurt");
        _animator.SetTrigger("Die");
    }

    //--- Input Actions ---
    public void OnMove(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.Paused) return;
        _movement = context.ReadValue<Vector2>();
        bool previous = _crouching;
        _crouching = (_movement.y <= -HALF_OF_ROOT_TWO);
        if (previous ^ _crouching)
        {
            (_collider as CapsuleCollider2D).offset = (_crouching ? _crouchColliderOffset : _colliderOffset);
            (_collider as CapsuleCollider2D).size = (_crouching ? _crouchColliderSize : _colliderSize);
        }
        _animator.SetBool("Crouching", _crouching);
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.Paused) return;
        if (!context.performed || _isDead || _attacking || _guarding || _throwing) return;

        //Handle one way platforms
        if (_crouching)
        {
            if (attemptPassthrough())
            {
                _animator.SetBool("Grounded", false);
                _isGrounded = false;
            }
        }
        else if (GetGrounded())
        {
            resetPassthrough();
            _isGrounded = false;
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _jumpForce);
            _animator.SetTrigger("Jump");
            _animator.SetBool("Grounded", false);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.Paused) return;
        if (_guarding || _isDead || _throwing) return;

        if (context.performed)
        {
            _attacking = true;
            _animator.SetTrigger("Attack");
        }
    }
    public void OnGuard(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.Paused) return;
        if (_attacking || _isDead || _throwing) return;

        _guarding = context.performed && (_guardHealth > 0);
        GameManager.Instance.UI.UpdateGuard(_guardHealth, _maxGuardHealth, _guarding, true);
        _animator.SetBool("Guard", _guarding);
    }
    public void OnSpecial(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.Paused) return;
        if (_guarding || _isDead || _attacking || _throwing) return;
        if (_ammo > 0)
        {
            Ammo--; //use public field to force the UI to update
            _throwing = true;
            _animator.SetTrigger("Throw");
        }
    }

    //--- Animator Event ---

    public void CastAttack()
    {
        _animator.ResetTrigger("Attack");
        Vector2 boxOffset = (Vector2)this.transform.position + new Vector2((_renderer.flipX?-1:1)*(_colliderSize.x/2 + _attackArea.x/2), (_collider as CapsuleCollider2D).offset.y + _attackArea.y/2);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxOffset, _attackArea, 0.0f);
        if (hits != null)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].GetComponent<IDamagable>() != null)
                {
                    if (hits[i].gameObject != this.gameObject)
                        hits[i].GetComponent<IDamagable>().TakeDamage(_attackDamage);
                }
            }
        }
    }
    public void ThrowProjectile()
    {
        if (_projectile != null)
        {
            GameObject newProjectile = Instantiate(_projectile, this.transform, false);
            newProjectile.transform.parent = null;

            if (newProjectile.GetComponent<Projectile>())
                newProjectile.GetComponent<Projectile>().Owner = this;

            if (newProjectile.GetComponent<Collider2D>())
                Physics2D.IgnoreCollision(newProjectile.GetComponent<Collider2D>(), _collider, true);

            if (newProjectile.GetComponent<Rigidbody2D>())
                newProjectile.GetComponent<Rigidbody2D>().velocity = ((_renderer.flipX ? Vector2.left : Vector2.right) + Vector2.up / 4) * 10;
        }
    }
    public void EndAttack()
    {
        _attacking = false;
    }

    public void EndThrow()
    {
        _throwing = false;
    }

}
