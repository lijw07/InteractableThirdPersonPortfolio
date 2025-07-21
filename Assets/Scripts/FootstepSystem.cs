using UnityEngine;

public class FootstepSystem : MonoBehaviour 
{
    [Header("Audio Source")]
    public AudioSource footstepAudioSource;
    
    [Header("Footstep Sounds")]
    public AudioClip[] dirtFootsteps;
    
    [Header("Volume Settings by Speed")]
    [Range(0f, 1f)] public float walkVolume = 0.1f;
    [Range(0f, 1f)] public float runVolume = 0.25f;
    [Range(0f, 1f)] public float sprintVolume = 0.4f;
    
    [Header("Speed Thresholds")]
    public float walkSpeedThreshold = 2f;
    public float runSpeedThreshold = 6f;
    
    [Header("Pitch Variation")]
    [Range(0f, 0.5f)] public float pitchVariation = 0.1f;
    
    private CharacterController characterController;
    private Rigidbody rb;
    
    void Start() 
    {
        if (footstepAudioSource == null)
            footstepAudioSource = GetComponent<AudioSource>();
        
        characterController = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        
        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.spatialBlend = 1f;
    }
    
    public void OnFootstep() 
    {
        PlayFootstepSound();
    }
    
    float GetCurrentMovementSpeed()
    {
        if (characterController != null)
        {
            return characterController.velocity.magnitude;
        }
        
        if (rb != null)
        {
            return rb.linearVelocity.magnitude;
        }
        
        return 0f;
    }
    
    void PlayFootstepSound() 
    {
        if (dirtFootsteps == null || dirtFootsteps.Length == 0) 
        {
            return;
        }
        
        float currentSpeed = GetCurrentMovementSpeed();
        float currentVolume;
        
        if (currentSpeed < walkSpeedThreshold)
        {
            currentVolume = walkVolume;
        }
        else if (currentSpeed < runSpeedThreshold)
        {
            currentVolume = runVolume;
        }
        else
        {
            currentVolume = sprintVolume;
        }
        
        AudioClip clipToPlay = dirtFootsteps[Random.Range(0, dirtFootsteps.Length)];
        
        footstepAudioSource.volume = currentVolume;
        footstepAudioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        
        footstepAudioSource.PlayOneShot(clipToPlay);
    }
}