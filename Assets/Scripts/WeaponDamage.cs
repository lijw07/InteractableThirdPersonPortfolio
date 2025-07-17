using UnityEngine;

public class WeaponDamage : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private string enemyTag = "Enemy";
    
    private bool canDamage = false;
    
    public void EnableDamage(bool enable)
    {
        canDamage = enable;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!canDamage) return;
        
        if (other.CompareTag(enemyTag) || other.CompareTag("Damageable"))
        {
            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            
            Debug.Log($"Hit {other.name} for {damage} damage!");
        }
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    public float GetDamage()
    {
        return damage;
    }
}

public interface IDamageable
{
    void TakeDamage(float damage);
}