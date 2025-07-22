using UnityEngine;

public class FootstepBridge : MonoBehaviour 
{
    private FootstepSystem parentFootstepSystem;
    
    void Start()
    {
        parentFootstepSystem = GetComponentInParent<FootstepSystem>();
    }
    
    public void OnFootstep() 
    {
        Debug.Log($"{name} triggered OnFootstep at {Time.time}");
        if (parentFootstepSystem != null)
            parentFootstepSystem.OnFootstep();
    }

    public void land()
    { 
        if (parentFootstepSystem != null)
            parentFootstepSystem.OnLand(); 
    }
}