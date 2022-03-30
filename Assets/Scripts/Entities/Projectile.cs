using UnityEngine;

public class Projectile : MonoBehaviour
{
    //--- Public Fields ---
    public LivingEntity Owner;
    public int Damage = 1;

    //--- Unity Method ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!(Owner != null && collision.collider.gameObject == Owner.gameObject) && collision.collider.GetComponent<LivingEntity>() != null)
            collision.collider.GetComponent<LivingEntity>().TakeDamage(Damage);
        Destroy(this.gameObject);
    }
}
