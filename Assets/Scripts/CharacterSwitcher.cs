using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CharacterSwitcher : MonoBehaviour
{
    [Header("Character Settings")]
    [SerializeField] private List<GameObject> characterPrefabs = new List<GameObject>();
    [SerializeField] private Transform spawnPoint;
    
    [Header("Camera Settings")]
    [SerializeField] private GameObject cameraRig;
    [SerializeField] private float cameraFollowOffset = 5f;
    [SerializeField] private float cameraHeight = 2f;
    
    private int currentCharacterIndex = 0;
    private GameObject currentCharacter;
    private List<GameObject> instantiatedCharacters = new List<GameObject>();
    
    [Header("Switch Settings")]
    [SerializeField] private float switchCooldown = 0.5f;
    private float lastSwitchTime = 0f;
    
    private PlayerInputActions inputActions;
    
    void Awake()
    {
        inputActions = GetComponent<PlayerInputActions>();
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<PlayerInputActions>();
        }
    }
    
    void Start()
    {
        if (characterPrefabs.Count == 0)
        {
            Debug.LogError("No character prefabs assigned to CharacterSwitcher!");
            return;
        }
        
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        InstantiateAllCharacters();
        ActivateCharacter(0);
        
        inputActions.SwitchCharacterAction.performed += OnSwitchCharacter;
    }
    
    void OnDestroy()
    {
        inputActions.SwitchCharacterAction.performed -= OnSwitchCharacter;
    }
    
    void OnSwitchCharacter(InputAction.CallbackContext context)
    {
        if (Time.time - lastSwitchTime > switchCooldown)
        {
            SwitchToNextCharacter();
            lastSwitchTime = Time.time;
        }
    }
    
    void InstantiateAllCharacters()
    {
        foreach (GameObject prefab in characterPrefabs)
        {
            GameObject character = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
            character.SetActive(false);
            instantiatedCharacters.Add(character);
        }
    }
    
    void SwitchToNextCharacter()
    {
        if (instantiatedCharacters.Count <= 1) return;
        
        int previousIndex = currentCharacterIndex;
        currentCharacterIndex = (currentCharacterIndex + 1) % instantiatedCharacters.Count;
        
        DeactivateCharacter(previousIndex);
        ActivateCharacter(currentCharacterIndex);
        
        Debug.Log($"Switched to character: {currentCharacter.name}");
    }
    
    void ActivateCharacter(int index)
    {
        if (index < 0 || index >= instantiatedCharacters.Count) return;
        
        currentCharacter = instantiatedCharacters[index];
        currentCharacter.SetActive(true);
        
        Vector3 currentPosition = currentCharacter.transform.position;
        currentCharacter.transform.position = spawnPoint.position;
        
        if (cameraRig != null)
        {
            UpdateCameraTarget();
        }
        
        var characterController = currentCharacter.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        BroadcastMessage("OnCharacterSwitch", currentCharacter, SendMessageOptions.DontRequireReceiver);
    }
    
    void DeactivateCharacter(int index)
    {
        if (index < 0 || index >= instantiatedCharacters.Count) return;
        
        GameObject character = instantiatedCharacters[index];
        
        var characterController = character.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        character.SetActive(false);
    }
    
    void UpdateCameraTarget()
    {
        var cameraFollow = cameraRig.GetComponent<ThirdPersonCamera>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(currentCharacter.transform);
        }
        else
        {
            Transform cameraTarget = currentCharacter.transform.Find("CameraTarget");
            if (cameraTarget == null)
            {
                cameraTarget = currentCharacter.transform;
            }
            
            cameraRig.transform.position = cameraTarget.position - cameraTarget.forward * cameraFollowOffset + Vector3.up * cameraHeight;
            cameraRig.transform.LookAt(cameraTarget);
        }
    }
    
    public GameObject GetCurrentCharacter()
    {
        return currentCharacter;
    }
    
    public void SwitchToCharacter(int index)
    {
        if (index < 0 || index >= instantiatedCharacters.Count) return;
        
        DeactivateCharacter(currentCharacterIndex);
        currentCharacterIndex = index;
        ActivateCharacter(currentCharacterIndex);
    }
    
    public void SwitchToCharacter(string characterName)
    {
        for (int i = 0; i < instantiatedCharacters.Count; i++)
        {
            if (instantiatedCharacters[i].name.Contains(characterName))
            {
                SwitchToCharacter(i);
                return;
            }
        }
        
        Debug.LogWarning($"Character with name '{characterName}' not found!");
    }
}