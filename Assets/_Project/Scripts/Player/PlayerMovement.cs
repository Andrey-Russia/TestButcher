using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float forwardSpeed = 5f;
    [SerializeField] private float horizontalSensitivity = 8f;
    [SerializeField] private float horizontalLimit = 2f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private bool isDragging;
    private Vector2 previousPointerPosition;
    private float verticalSpeed;
    private bool isRunning;

    public bool IsRunning => isRunning;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!isRunning)
        { 
            return;
        }

        float horizontalMovement = ReadHorizontalMovement();

        float targetX = Mathf.Clamp(transform.position.x + horizontalMovement, -horizontalLimit, horizontalLimit);

        if (controller.isGrounded && verticalSpeed < 0f)
        {
            verticalSpeed = -2f;
        }

        verticalSpeed += gravity * Time.deltaTime;

        Vector3 movement = new Vector3(targetX - transform.position.x, verticalSpeed * Time.deltaTime, forwardSpeed * Time.deltaTime);

        controller.Move(movement);
    }

    private float ReadHorizontalMovement()
    {
        Pointer pointer = Pointer.current;

        if (pointer == null || !pointer.press.isPressed)
        {
            isDragging = false;
            return 0f;
        }

        Vector2 currentPosition = pointer.position.ReadValue();

        if (!isDragging)
        {
            isDragging = true;
            previousPointerPosition = currentPosition;
            return 0f;
        }

        float deltaX = currentPosition.x - previousPointerPosition.x;
        previousPointerPosition = currentPosition;

        return deltaX / Mathf.Max(Screen.width, 1) * horizontalSensitivity;
    }

    private void OnDisable()
    {
        isDragging = false;
        verticalSpeed = 0f;
    }

    public void SetRunning(bool value)
    {
        isRunning = value;
        isDragging = false;
        verticalSpeed = 0f;
    }
}