
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 12f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 3f;

    private Rigidbody2D rb;
    private Vector2 movement;

    private bool dashUnlocked;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;

    public float CurrentMoveSpeed => moveSpeed;
    public bool DashUnlocked => dashUnlocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movement = new Vector2(
            horizontal,
            vertical
        ).normalized;

        if (dashTimer > 0f)
            dashTimer -= Time.deltaTime;

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        // Space activates dash after Mobility Tier 3.
        if (dashUnlocked &&
            Input.GetKeyDown(KeyCode.Space) &&
            dashCooldownTimer <= 0f &&
            movement.sqrMagnitude > 0.01f)
        {
            dashDirection = movement;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            Debug.Log("PLAYER DASH!");
        }
    }

    private void FixedUpdate()
    {
        if (dashTimer > 0f)
        {
            rb.MovePosition(
                rb.position +
                dashDirection * dashSpeed * Time.fixedDeltaTime
            );

            return;
        }

        rb.MovePosition(
            rb.position +
            movement * moveSpeed * Time.fixedDeltaTime
        );
    }

    public void IncreaseMoveSpeed(float amount)
    {
        moveSpeed += amount;

        Debug.Log($"Player Move Speed: {moveSpeed}");
    }

    public void UnlockDash()
    {
        dashUnlocked = true;

        Debug.Log("Player unlocked Dash! Press SPACE to dash.");
    }
}
