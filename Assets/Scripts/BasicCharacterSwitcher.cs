using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class BasicCharacterSwitcher : MonoBehaviour
{
    [Header("Character Settings")]
    [SerializeField] private List<GameObject> characterPrefabs = new List<GameObject>();
    [SerializeField] private Transform spawnPoint;
    
    [Header("Camera Settings")]
    [SerializeField] private GameObject cameraRig;
    
    private int currentCharacterIndex = 0;
    private GameObject currentCharacter;
    private List<GameObject> instantiatedCharacters = new List<GameObject>();
    
    [Header("Switch Settings")]
    [SerializeField] private KeyCode switchKey = KeyCode.T;
    [SerializeField] private float switchCooldown = 0.5f;
    private float lastSwitchTime = 0f;
    
    void Start()
    {
        if (characterPrefabs.Count == 0)
        {
            Debug.LogError("No character prefabs assigned to BasicCharacterSwitcher!");
            return;
        }
        
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
        
        InstantiateAllCharacters();
        ActivateCharacter(0);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(switchKey) && Time.time - lastSwitchTime > switchCooldown)
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
        var cameraFollow = cameraRig.GetComponent<BasicThirdPersonCamera>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(currentCharacter.transform);
        }
    }
    
    public GameObject GetCurrentCharacter()
    {
        return currentCharacter;
    }
}