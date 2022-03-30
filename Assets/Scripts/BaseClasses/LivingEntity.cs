using System.Collections;

using UnityEngine;
using UnityEngine.Events;

public abstract class LivingEntity : MonoBehaviour, IDamagable
{
    //--- Protected Variables --- 
    protected int _health = 1;
    protected bool _isDead = false;
    protected bool _invincible = false;

    //--- Editor Parameters ---
    [Header("Death")]
    [SerializeField] [Tooltip("What will be instantiated on death.")] private GameObject _drop = null;
    [SerializeField] [Tooltip("What will be fired on death.")] private UnityEvent _onDeath = null;

    //--- From Interface ---
    public virtual void TakeDamage(int damage)
    {
        if (_invincible) return;
        if ((_health -= damage) <= 0)
        {
            _health = 0;
            this.Die();
        }
    }

    //--- Custom Methods ---
    public virtual void Die()
    {
        if (_isDead) return;
        if (_drop != null) Instantiate(_drop, this.transform.position, this.transform.rotation, null);
        if (_onDeath != null) _onDeath.Invoke();
    }

    //--- Coroutines ---
    protected IEnumerator Invincibility(float seconds = 1f, float frameLength = 0.1f)
    {
        _invincible = true;
        for (float i = 0; i < seconds; i += frameLength)
        {
            this.GetComponent<Renderer>().enabled = !this.GetComponent<Renderer>().enabled;
            yield return new WaitForSeconds(frameLength);
        }
        this.GetComponent<Renderer>().enabled = true;
        _invincible = false;
    }
}
