using System.Collections;
using UnityEngine;

public class FinishGate : MonoBehaviour
{
    [Header("Requirements")]
    [SerializeField] private int requiredWealth = 20;
    [SerializeField] private int rewardMultiplier = 2;

    [Header("Door hinges")]
    [SerializeField] private Transform leftHinge;
    [SerializeField] private Transform rightHinge;

    [Header("Opening")]
    [SerializeField] private float openingDuration = 0.4f;
    [SerializeField] private float leftAngle = -95f;
    [SerializeField] private float rightAngle = 95f;

    private GameManager gameManager;
    private PlayerMovement approachingPlayer;
    private bool activated;
    private bool multiplierGranted;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (activated || gameManager == null)
        {
            return;
        }

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null || !player.IsRunning)
        {
            return;
        }
            

        PlayerWealth wealth = player.GetComponent<PlayerWealth>();

        if (wealth == null)
        {
            return;
        }
            

        activated = true;

        if (wealth.Value < requiredWealth)
        {
            gameManager.Win();
            return;
        }

        approachingPlayer = player;
        StartCoroutine(OpenDoors());
    }

    private void Update()
    {
        if (approachingPlayer == null || multiplierGranted)
        {
            return;
        }

        if (gameManager.State != GameState.Playing)
        {
            return;
        }
            
        float playerLocalZ = transform.InverseTransformPoint(approachingPlayer.transform.position).z;

        if (playerLocalZ >= 0f)
        {
            multiplierGranted = true;
            gameManager.IncreaseFinishMultiplier(rewardMultiplier);
        }
    }

    private IEnumerator OpenDoors()
    {
        Quaternion leftStart = leftHinge.localRotation;
        Quaternion rightStart = rightHinge.localRotation;

        Quaternion leftTarget = leftStart * Quaternion.Euler(0f, leftAngle, 0f);

        Quaternion rightTarget = rightStart * Quaternion.Euler(0f, rightAngle, 0f);

        float duration = Mathf.Max(0.01f, openingDuration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            leftHinge.localRotation = Quaternion.Slerp(leftStart, leftTarget, t);

            rightHinge.localRotation = Quaternion.Slerp(rightStart, rightTarget, t);

            yield return null;
        }

        leftHinge.localRotation = leftTarget;
        rightHinge.localRotation = rightTarget;
    }
}