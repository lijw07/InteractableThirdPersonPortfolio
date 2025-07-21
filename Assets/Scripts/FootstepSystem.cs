using UnityEngine;
using UnityEngine.Serialization;

public class FootstepSystem : MonoBehaviour 
{
    [Header("Audio Source")]
    public AudioSource footstepAudioSource;
    
    [FormerlySerializedAs("grassFootsteps")] [Header("Footstep Sounds")]
    public AudioClip[] dirtFootsteps;
    public AudioClip[] grassFootsteps;
    public AudioClip[] stoneFootsteps;
    
    [Header("Volume Settings by Speed")]
    [Range(0f, 1f)] public float walkVolume = 0.3f;
    [Range(0f, 1f)] public float runVolume = 0.6f;
    [Range(0f, 1f)] public float sprintVolume = 0.9f;
    
    [Header("Pitch Variation")]
    [Range(0f, 0.5f)] public float pitchVariation = 0.1f;
    
    [Header("Surface Detection")]
    [Range(0.1f, 2f)] public float raycastDistance = 1f;
    
    [Header("Debug")]
    public bool debugFootsteps = false;
    
    private CharacterController characterController;
    private CharacterManageController characterManager;
    private PlayerController playerController;
    private string currentSurfaceType = "dirt";
    private string lastHitObject = "None";
    
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
    
    public void OnFootstep() 
    {
        DetectSurface();
        PlayFootstepSound();
    }
    
    void DetectSurface()
    {
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;
        
        if (debugFootsteps)
        {
            Debug.DrawRay(rayStart, Vector3.down * raycastDistance, Color.red, 2f);
            Debug.Log($"[FootstepSystem] Raycast from: {rayStart} | Distance: {raycastDistance}");
        }
        
        if (Physics.Raycast(rayStart, Vector3.down, out hit, raycastDistance))
        {
            lastHitObject = hit.collider.name;
            
            if (hit.collider.CompareTag("Grass"))
                currentSurfaceType = "grass";
            else if (hit.collider.CompareTag("Stone"))
                currentSurfaceType = "stone";
            else
                currentSurfaceType = "dirt";
                
            if (debugFootsteps) 
                Debug.Log($"Detected surface: {currentSurfaceType} on object: {hit.collider.name}");
        }
        else
        {
            currentSurfaceType = "dirt";
            if (debugFootsteps) 
                Debug.Log("No surface detected, using dirt");
        }
    }
    
    AudioClip[] GetFootstepsForCurrentSurface()
    {
        switch (currentSurfaceType)
        {
            case "grass": 
                return grassFootsteps != null && grassFootsteps.Length > 0 ? grassFootsteps : dirtFootsteps;
            case "stone": 
                return stoneFootsteps != null && stoneFootsteps.Length > 0 ? stoneFootsteps : dirtFootsteps;
            default: 
                return dirtFootsteps;
        }
    }
    
    float GetCurrentMovementSpeed()
    {
        if (playerController != null)
        {
            return playerController.GetCurrentSpeed();
        }
        
        if (characterController != null)
        {
            return characterController.velocity.magnitude;
        }
        
        return 0f;
    }
    
    void PlayFootstepSound() 
    {
        AudioClip[] currentFootsteps = GetFootstepsForCurrentSurface();
        
        if (currentFootsteps == null || currentFootsteps.Length == 0) 
        {
            if (debugFootsteps) Debug.Log($"No footstep sounds assigned for surface: {currentSurfaceType}");
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
        
        AudioClip clipToPlay = currentFootsteps[Random.Range(0, currentFootsteps.Length)];
        
        footstepAudioSource.volume = currentVolume;
        footstepAudioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        
        footstepAudioSource.PlayOneShot(clipToPlay);
    }
    
    public string GetCurrentSurface() => currentSurfaceType;
    public string GetLastRaycastObject() => lastHitObject;
}