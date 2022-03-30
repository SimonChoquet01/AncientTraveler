using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : LandDweller
{
    //--- Private Variables ---
    private bool _hunting = false;
    private Transform _target;
    private bool _attacking = false;
    private bool _interupted = false;
    private float _cooldown = 0f;

    //--- Components ---
    private Animator _animator;
    private SpriteRenderer _renderer;

    //--- Editor Parameters ---
    [Header("A.I. Settings")]
    [SerializeField] [Tooltip("The speed at which the enemy will move.")] private float _walkSpeed = 1.0f;
    [SerializeField] [Tooltip("How much damage does this enemy deal.")] private int _damage = 1;

    //--- Unity methods ---
    private new void Start()
    {
        base.Start();
        _animator = GetComponent<Animator>();
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _target = GameManager.Instance.Player.transform;
        _health = 3;
    }
    
    private void FixedUpdate()
    {
        //Is this skeleteon dead?
        if (_isDead)
        {
            if (_rigidbody.constraints != RigidbodyConstraints2D.FreezeAll && GetGrounded())
            {
                _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
                List<Collider2D> colliders = new List<Collider2D>();
                _rigidbody.GetAttachedColliders(colliders);
                for (int i = 0; i < _rigidbody.attachedColliderCount; i++)
                {
                    colliders[i].isTrigger = true;
                }
            }
            if (!_renderer.isVisible)
                Destroy(this.gameObject);
            return;
        }

        //Attack cooldown
        if (_cooldown > 0)
            if ((_cooldown-=Time.fixedDeltaTime) < 0) _cooldown = 0;

        //If the skeleton has made it onto our screen, start acting.
        if (!_hunting)
        {
            if (_renderer.isVisible)
                _hunting = true;
            else
                return;
        }
        //Has our grounded value possibly changed?
        if ((_isGrounded && _rigidbody.velocity.y != 0) || (!_isGrounded && (int)_rigidbody.velocity.y == 0))
            _animator.SetBool("Grounded", GetGrounded());
        _animator.SetFloat("Height", (_isGrounded ? -1 : 1));
        //Handle bumping into things
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        _rigidbody.GetContacts(contacts);
        for (int i = 0; i < contacts.Count; i++)
        {
            if (Mathf.Abs(contacts[i].normal.x) == 1)
            {
                Debug.Log(contacts[i].collider.name);
                if (contacts[i].collider.gameObject == GameManager.Instance.Player.gameObject && !_attacking && _cooldown <= 0)
                {
                    _attacking = true;
                    _cooldown = 1.5f;
                    _animator.SetTrigger("Attack");
                    _renderer.flipX = (contacts[i].normal.x > 0);
                    break;
                }
                else if (contacts[i].collider.gameObject.layer == LayerMask.NameToLayer("Level"))
                {
                    _renderer.flipX = (contacts[i].normal.x < 0);
                    break;
                }
            } 
        }

        //Handle walking
        if (!_attacking && !_interupted)
        {
            _rigidbody.velocity = new Vector2(_renderer.flipX ? -_walkSpeed : _walkSpeed, _rigidbody.velocity.y);
            _animator.SetBool("Walk", true);
        }
        else
        {
            _animator.SetBool("Walk", false);
            _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
        }

    }

    //--- Inherited Methods ---
    public override void TakeDamage(int damage)
    {
        if (_isDead) return;

        //Handle actually being hit
        _attacking = false;
        _animator.ResetTrigger("Attack");
        _animator.SetTrigger("Hurt");
        if ((_health -= damage) <= 0)
        {
            _health = 0;
            this.Die();
        }
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

    //--- Animation Events ---
    public void CastAttack()
    {
        _animator.ResetTrigger("Attack");
        Vector2 boxOffset = (Vector2)this.transform.position + new Vector2((_renderer.flipX ? -1 : 1) * ((_collider as CapsuleCollider2D).size.x / 2 + 0.5f), (_collider as CapsuleCollider2D).offset.y + 0.5f);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxOffset, Vector2.one, 0.0f);
        if (hits != null)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].GetComponent<IDamagable>() != null)
                {
                    if (hits[i].gameObject != this.gameObject)
                        hits[i].GetComponent<IDamagable>().TakeDamage(_damage);
                }
            }
        }
    }

    public void EndAttack()
    {
        _attacking = false;
    }

}
