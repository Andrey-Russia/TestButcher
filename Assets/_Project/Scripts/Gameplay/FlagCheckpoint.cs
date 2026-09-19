using System.Collections;
using UnityEngine;

/// <summary>A visual checkpoint; it does not change wealth or save progress.</summary>
[RequireComponent(typeof(BoxCollider))]
public sealed class FlagCheckpoint : MonoBehaviour
{
    [SerializeField] private Transform[] flags;
    [SerializeField, Min(0f)] private float raiseHeight = 1.25f;
    [SerializeField, Min(0.01f)] private float raiseDuration = 0.65f;
    [SerializeField] private AudioClip activationSound;
    [SerializeField, Range(0f, 1f)] private float volume = 0.65f;

    private Vector3[] initialPositions;
    private AudioSource audioSource;
    private bool activated;

    private void Awake()
    {
        initialPositions = new Vector3[flags.Length];
        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i] != null)
                initialPositions[i] = flags[i].localPosition;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated)
            return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null || !player.IsRunning)
            return;

        activated = true;
        if (activationSound != null)
            audioSource.PlayOneShot(activationSound, volume);

        StartCoroutine(RaiseFlags());
    }

    private IEnumerator RaiseFlags()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, raiseDuration);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            SetHeight(raiseHeight * progress);
            yield return null;
        }

        SetHeight(raiseHeight);
    }

    private void SetHeight(float height)
    {
        for (int i = 0; i < flags.Length; i++)
        {
            if (flags[i] != null)
                flags[i].localPosition = initialPositions[i] + Vector3.up * height;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (initialPositions != null)
            SetHeight(0f);
        if (audioSource != null)
            audioSource.Stop();
        activated = false;
    }

    private void Reset()
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.center = new Vector3(0f, 1.25f, 0f);
        trigger.size = new Vector3(5f, 2.5f, 1.2f);
    }
}
