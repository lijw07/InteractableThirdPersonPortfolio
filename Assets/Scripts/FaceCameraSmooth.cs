using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FaceCameraSmooth : MonoBehaviour
{
    private Camera mainCamera;

    [Header("Rotation")]
    public float smoothSpeed = 5f;
    public List<GameObject> canvases = new List<GameObject>();

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip appearClip;
    public AudioClip disappearClip;

    private Coroutine activationCoroutine;

    private readonly string[] defaultCanvasNames = { "Education", "Project", "Experience", "Skill" };

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        foreach (GameObject canvas in canvases)
        {
            if (canvas == null || !canvas.activeSelf) continue;

            Vector3 direction = mainCamera.transform.position - canvas.transform.position;
            direction.y = 0f; // Optional: keeps canvas upright if needed
            Quaternion targetRotation = Quaternion.LookRotation(-direction.normalized);

            canvas.transform.rotation = Quaternion.Lerp(
                canvas.transform.rotation,
                targetRotation,
                Time.deltaTime * smoothSpeed
            );
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Stop any current coroutine
        if (activationCoroutine != null)
            StopCoroutine(activationCoroutine);

        // Deactivate default canvases immediately
        foreach (GameObject canvas in canvases)
        {
            if (canvas != null && IsDefaultCanvas(canvas))
                canvas.SetActive(false);
        }

        // Start gradual reveal of other canvases
        activationCoroutine = StartCoroutine(GradualActivateOtherCanvases());

        PlayClip(disappearClip);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Stop any ongoing coroutine
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }

        // Deactivate everything
        foreach (GameObject canvas in canvases)
        {
            if (canvas != null)
                canvas.SetActive(false);
        }

        // Reactivate the default ones
        foreach (GameObject canvas in canvases)
        {
            if (canvas != null && IsDefaultCanvas(canvas))
                canvas.SetActive(true);
        }

        PlayClip(appearClip);
    }

    private bool IsDefaultCanvas(GameObject canvas)
    {
        foreach (string name in defaultCanvasNames)
        {
            if (canvas.name.Contains(name)) return true;
        }
        return false;
    }

    private IEnumerator GradualActivateOtherCanvases()
    {
        yield return new WaitForSeconds(1f);

        foreach (GameObject canvas in canvases)
        {
            if (canvas != null && !IsDefaultCanvas(canvas))
            {
                canvas.SetActive(true);
                PlayClip(appearClip);
                yield return new WaitForSeconds(2f);
            }
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
