using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class CharacterManageController : MonoBehaviour
{
    [Header("Character Settings")]
    [SerializeField] private float switchCooldown = 0f;
    
    private List<GameObject> characters = new();
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
            currentCharacterIndex = 0;
            ActivateCharacter(currentCharacterIndex);
        }
        else
        {
            Debug.LogError("[CharacterManageController] No child characters found with CharacterController components!");
        }
        
        if (inputActions != null)
        {
            inputActions.OnNextAction += OnSwitchCharacter;
        }
    }
    
    void OnDestroy()
    {
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
    }
    
    void OnSwitchCharacter()
    {
        if (Time.time - lastSwitchTime > switchCooldown)
        {
            if (Keyboard.current != null && (Keyboard.current.altKey.isPressed || Keyboard.current.leftAltKey.isPressed || Keyboard.current.rightAltKey.isPressed))
            {
                SwitchToPreviousCharacter();
            }
            else
            {
                SwitchToNextCharacter();
            }
            lastSwitchTime = Time.time;
        }
    }
    
    public void SwitchToNextCharacter()
    {
        if (characters.Count <= 1) return;

        int previousIndex = currentCharacterIndex;
        GameObject previousCharacter = characters[previousIndex];

        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;
        GameObject nextCharacter = characters[currentCharacterIndex];

        nextCharacter.transform.position = previousCharacter.transform.position;
        nextCharacter.transform.rotation = previousCharacter.transform.rotation;

        DeactivateCharacter(previousIndex);
        ActivateCharacter(currentCharacterIndex);
    }

    
    public void SwitchToPreviousCharacter()
    {
        if (characters.Count <= 1) return;

        int previousIndex = currentCharacterIndex;
        GameObject previousCharacter = characters[previousIndex];

        currentCharacterIndex = (currentCharacterIndex + 1) % characters.Count;
        GameObject nextCharacter = characters[currentCharacterIndex];
        
        nextCharacter.transform.position = previousCharacter.transform.position;
        nextCharacter.transform.rotation = previousCharacter.transform.rotation;

        DeactivateCharacter(previousIndex);
        ActivateCharacter(currentCharacterIndex);

        Debug.Log($"Switched to character: {currentCharacter.name}");
    }
    
    
    void ActivateCharacter(int index)
    {
        if (index < 0 || index >= characters.Count)
        {
            Debug.LogError($"[CharacterManageController] Invalid character index: {index}");
            return;
        }
        
        currentCharacter = characters[index];
        Animator anim = currentCharacter.GetComponent<Animator>();
        if (anim != null) anim.enabled = true;
        currentCharacter.SetActive(true);
        
        CharacterController controller = currentCharacter.GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.LogError($"Character {currentCharacter.name} is missing CharacterController!");
        }
        
        if (playerController != null)
        {
            playerController.OnCharacterSwitched();
        }
        
        if (animationController != null)
        {
            animationController.OnCharacterSwitched();
        }
    }
    
    void DeactivateCharacter(int index)
    {
        if (index < 0 || index >= characters.Count) return;
        
        GameObject character = characters[index];
        Animator anim = character.GetComponent<Animator>();
        if (anim != null) anim.enabled = false;
        character.SetActive(false);
    }
    
    public GameObject GetCurrentCharacter()
    {
        return currentCharacter;
    }
    
    public CharacterController GetCurrentCharacterController()
    {
        if (currentCharacter != null)
        {
            return currentCharacter.GetComponent<CharacterController>();
        }
        return null;
    }
    
    public Animator GetCurrentAnimator()
    {
        if (currentCharacter != null)
        {
            return currentCharacter.GetComponent<Animator>();
        }
        return null;
    }
}