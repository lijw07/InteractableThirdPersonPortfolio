using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private Transform weaponSlot;
    [SerializeField] private string weaponTag = "Weapon";
    
    private GameObject equippedWeapon;
    private Animator weaponAnimator;
    private Collider weaponCollider;
    
    [Header("Weapon Detection")]
    [SerializeField] private bool detectWeaponAutomatically = true;
    
    void Start()
    {
        if (weaponSlot == null)
        {
            Transform rightHand = FindDeepChild(transform, "RightHand");
            if (rightHand == null)
            {
                rightHand = FindDeepChild(transform, "Right Hand");
            }
            if (rightHand == null)
            {
                rightHand = FindDeepChild(transform, "hand_r");
            }
            if (rightHand == null)
            {
                rightHand = FindDeepChild(transform, "mixamorig:RightHand");
            }
            
            if (rightHand != null)
            {
                weaponSlot = rightHand;
            }
            else
            {
                GameObject weaponSlotObj = new GameObject("WeaponSlot");
                weaponSlotObj.transform.parent = transform;
                weaponSlotObj.transform.localPosition = new Vector3(0.5f, 1f, 0.5f);
                weaponSlot = weaponSlotObj.transform;
            }
        }
        
        if (detectWeaponAutomatically)
        {
            DetectEquippedWeapon();
        }
    }
    
    void DetectEquippedWeapon()
    {
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.CompareTag(weaponTag) || child.name.ToLower().Contains("weapon") || 
                child.name.ToLower().Contains("sword") || child.name.ToLower().Contains("axe") ||
                child.name.ToLower().Contains("spear") || child.name.ToLower().Contains("staff"))
            {
                SetEquippedWeapon(child.gameObject);
                break;
            }
        }
    }
    
    Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>())
        {
            if (child.name.ToLower().Contains(name.ToLower()))
            {
                return child;
            }
        }
        return null;
    }
    
    public void SetEquippedWeapon(GameObject weapon)
    {
        equippedWeapon = weapon;
        
        if (equippedWeapon != null)
        {
            weaponAnimator = equippedWeapon.GetComponent<Animator>();
            weaponCollider = equippedWeapon.GetComponent<Collider>();
            
            if (weaponCollider == null)
            {
                weaponCollider = equippedWeapon.GetComponentInChildren<Collider>();
            }
            
            if (weaponCollider != null)
            {
                weaponCollider.isTrigger = true;
                
                WeaponDamage weaponDamage = weaponCollider.gameObject.GetComponent<WeaponDamage>();
                if (weaponDamage == null)
                {
                    weaponDamage = weaponCollider.gameObject.AddComponent<WeaponDamage>();
                }
            }
        }
    }
    
    public void EquipWeapon(GameObject weapon)
    {
        if (equippedWeapon != null)
        {
            UnequipWeapon();
        }
        
        equippedWeapon = weapon;
        weapon.transform.parent = weaponSlot;
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
        
        SetEquippedWeapon(weapon);
    }
    
    public void UnequipWeapon()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.transform.parent = null;
            equippedWeapon = null;
            weaponAnimator = null;
            weaponCollider = null;
        }
    }
    
    public bool HasWeaponEquipped()
    {
        return equippedWeapon != null;
    }
    
    public GameObject GetEquippedWeapon()
    {
        return equippedWeapon;
    }
    
    public void EnableWeaponCollider(bool enable)
    {
        if (weaponCollider != null)
        {
            weaponCollider.enabled = enable;
        }
    }
    
    public void PlayWeaponAnimation(string animationName)
    {
        if (weaponAnimator != null)
        {
            weaponAnimator.Play(animationName);
        }
    }
}