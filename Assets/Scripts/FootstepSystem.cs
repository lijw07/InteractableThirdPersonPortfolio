using UnityEngine;

public class FootstepSystem : MonoBehaviour 
{
    [Header("Audio Source")]
    public AudioSource footstepAudioSource;
    
    [Header("Footstep Sounds")]
    public AudioClip[] grassFootsteps;
    
    [Header("Volume Settings by Speed")]
    [Range(0f, 1f)] public float walkVolume = 0.3f;
    [Range(0f, 1f)] public float runVolume = 0.6f;
    [Range(0f, 1f)] public float sprintVolume = 0.9f;
    
    [Header("Pitch Variation")]
    [Range(0f, 0.5f)] public float pitchVariation = 0.1f;
    
    [Header("Debug")]
    public bool debugFootsteps = false;
    
    private CharacterController characterController;
    private CharacterManageController characterManager;
    private PlayerController playerController;
    
    void Start() 
    {
        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();
        
        characterManager = GetComponent<CharacterManageController>();
        playerController = GetComponent<PlayerController>();
        
        if (characterManager != null)
        {
            characterController = characterManager.GetCurrentCharacterController();
        }
        
        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.spatialBlend = 1f;
    }
    
    public void OnCharacterSwitched()
    {
        if (characterManager != null)
        {
            characterController = characterManager.GetCurrentCharacterController();
        }
    }
    
    public void OnFootstep() 
    {
        PlayFootstepSound();
    }
    
    bool IsMoving()
    {
        if (characterController == null)
        {
            UpdateCharacterController();
            if (characterController == null) return false;
        }
        
        return characterController.velocity.magnitude > 0.05f; // Lowered threshold
    }
    
    bool IsGrounded() 
    {
        if (characterController == null)
        {
            UpdateCharacterController();
            if (characterController == null) return true; // Default to true if no controller
        }
        
        return characterController.isGrounded;
    }
    
    void UpdateCharacterController()
    {
        if (characterManager != null)
        {
            characterController = characterManager.GetCurrentCharacterController();
        }
    }
    
    float GetCurrentMovementSpeed()
    {
        // Try to get speed from PlayerController first (more reliable)
        if (playerController != null)
        {
            return playerController.GetCurrentSpeed();
        }
        
        // Fallback to CharacterController velocity
        if (characterController != null)
        {
            return characterController.velocity.magnitude;
        }
        
        return 0f;
    }
    
    void PlayFootstepSound() 
    {
        if (grassFootsteps == null || grassFootsteps.Length == 0) 
        {
            if (debugFootsteps) Debug.Log("No footstep sounds assigned");
            return;
        }
            
        float currentSpeed = GetCurrentMovementSpeed();
        float currentVolume;
        
        if (currentSpeed < 2f)
        {
            currentVolume = walkVolume;
        }
        else if (currentSpeed < 6f)
        {
            currentVolume = runVolume;
        }
        else
        {
            currentVolume = sprintVolume;
        }
        
        AudioClip clipToPlay = grassFootsteps[Random.Range(0, grassFootsteps.Length)];
        
        footstepAudioSource.volume = currentVolume;
        footstepAudioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        
        footstepAudioSource.PlayOneShot(clipToPlay);
    }
}