using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class LandDweller : LivingEntity
{
    //--- Protected Variables ---
    protected bool _isGrounded = false;
    protected List<Collider2D> _passthroughList = new List<Collider2D>();

    //--- Editor Parameters ---
    [Header("Land Dweller Options")]
    [SerializeField] [Tooltip("The angle at which a collision is still considered the floor.")] [Min(0)] private float _maxSlope = 45f;
    [SerializeField] [Tooltip("The sound that is play when this creature takes a step.")] private AudioClip _stepSound;

    //--- Components ---
    protected Collider2D _collider;
    protected Rigidbody2D _rigidbody;
    protected AudioSource _audioSource;

    //--- Unity Methods ---
    protected void Start()
    {
        _collider = GetComponent<Collider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
    }

    //--- Custom Methods ---
    private Collider2D[] getGroundColliders()
    {
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        List<Collider2D> colliders = new List<Collider2D>();
        _rigidbody.GetContacts(contacts);
        for (int i = 0; i < contacts.Count; i++)
        {
            float slope = Vector2.SignedAngle(Vector2.up, contacts[i].normal);
            if (-_maxSlope <= slope && slope <= _maxSlope) colliders.Add(contacts[i].collider);
        }
        return colliders.ToArray();
    }
    protected bool attemptPassthrough()
    {
        bool found = false;
        Collider2D[] colliders = getGroundColliders();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].GetComponent<PlatformEffector2D>() != null)
            {
                _passthroughList.Add(colliders[i]);
                Physics2D.IgnoreCollision(_collider, colliders[i], true);
                found = true;
            }
        }
        return found;
    }

    protected void resetPassthrough()
    {
        for (int i = 0; i < _passthroughList.Count; i++)
        {
            Physics2D.IgnoreCollision(_collider, _passthroughList[i], false);
            _passthroughList.RemoveAt(i);
        }
    }

    public bool GetGrounded(bool recalculate = true)
    {
        if (recalculate)
        {
            _isGrounded = (getGroundColliders().Length > 0);
        }
        return _isGrounded;
    }

    //--- Animator Events ---
    public void StepSound()
    {
        if (_audioSource != null && _stepSound != null)
        {
            _audioSource.pitch = Random.Range(1.1f, 1.3f);
            _audioSource.PlayOneShot(_stepSound);
        } 
    }

}
