using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CharacterManageController : MonoBehaviour
{
    [Header("Character Settings")]
    [SerializeField] private int startingCharacterIndex = 0;
    [SerializeField] private float switchCooldown = 0f;
    
    private List<GameObject> characters = new List<GameObject>();
    private int currentCharacterIndex = 0;
    private GameObject currentCharacter;
    private float lastSwitchTime = 0f;
    
    private PlayerInputActions inputActions;
    private PlayerController playerController;
    private PlayerAnimationController animationController;
    
    void Awake()
    {
        inputActions = GetComponent<PlayerInputActions>();
        if (inputActions == null)
        {
            inputActions = gameObject.AddComponent<PlayerInputActions>();
        }
        
        playerController = GetComponent<PlayerController>();
        animationController = GetComponent<PlayerAnimationController>();
    }
    
    void Start()
    {
        FindChildCharacters();
        
        if (characters.Count > 0)
        {
            // Select a random character instead of using starting index
            int randomIndex = Random.Range(0, characters.Count);
            currentCharacterIndex = randomIndex;
            ActivateCharacter(randomIndex);
        }
        else
        {
            Debug.LogError("No child characters found with CharacterController components!");
        }
        
        // Subscribe to input events
        if (inputActions != null)
        {
            inputActions.OnNextAction += OnSwitchCharacter;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (inputActions != null)
        {
            inputActions.OnNextAction -= OnSwitchCharacter;
        }
    }
    
    void FindChildCharacters()
    {
        CharacterController[] childControllers = GetComponentsInChildren<CharacterController>(true);
        
        foreach (CharacterController controller in childControllers)
        {
            GameObject character = controller.gameObject;
            character.SetActive(false);
            characters.Add(character);
        }
        
        Debug.Log($"Found {characters.Count} child characters");
    }
    
    void OnSwitchCharacter()
    {
        if (Time.time - lastSwitchTime > switchCooldown)
        {
            SwitchToNextCharacter();
            lastSwitchTime = Time.time;
        }
    }
    
    public void SwitchToNextCharacter()
    {
        if (characters.Count <= 1) return;
        
        int previousIndex = currentCharacterIndex;
        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;
        
        DeactivateCharacter(previousIndex);
        ActivateCharacter(currentCharacterIndex);
        
        // Ensure new character is at local origin
        currentCharacter.transform.localPosition = Vector3.zero;
        currentCharacter.transform.localRotation = Quaternion.identity;
        
        Debug.Log($"Switched to character: {currentCharacter.name}");
    }
    
    
    void ActivateCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        
        currentCharacter = characters[index];
        currentCharacter.SetActive(true);
        
        CharacterController controller = currentCharacter.GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError($"Character {currentCharacter.name} is missing CharacterController!");
        }
        
        if (playerController != null)
        {
            playerController.SetCurrentCharacter(currentCharacter);
        }
        
        if (animationController != null)
        {
            animationController.SetCurrentCharacter(currentCharacter);
        }
        
        var cameraFollow = Camera.main.GetComponent<ThirdPersonCamera>();
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(transform);
        }
    }
    
    void DeactivateCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        
        GameObject character = characters[index];
        character.SetActive(false);
    }
    
    public GameObject GetCurrentCharacter()
    {
        return currentCharacter;
    }
    
    public List<GameObject> GetAllCharacters()
    {
        return characters;
    }
}