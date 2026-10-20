using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float speedIncreaseRate = 0.055f;
    // Retained for existing scene serialization; runtime speed intentionally has no cap.
    [HideInInspector] public float maxMoveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 12f;

    [Header("Dash")]
    public float dashForce = 25f;
    public float dashSpeedBonus = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1.2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Sound Effects")]
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip hitSound;

    [Header("Double Jump UI")]
    public TextMeshProUGUI doubleJumpText;
    public GameObject doubleJumpEffectPrefab;

    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Vector3 originalScale;
    private GameManager gameManager;
    private CameraShake cameraShake;
    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;

    public float dashCooldownTimer;
    public int doubleJumpItemCount = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerController requires a Rigidbody2D.", this);
            enabled = false;
            return;
        }

        audioSource = GetComponent<AudioSource>();
        originalScale = transform.localScale;
        gameManager = FindFirstObjectByType<GameManager>();
        cameraShake = FindFirstObjectByType<CameraShake>();
        UpdateDoubleJumpUI();
    }

    private void Update()
    {
        if (rb == null || (gameManager != null && gameManager.IsGameOver))
            return;

        if (TitleManager.IsAttractMode)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            return;
        }

        isGrounded = CheckGrounded();
        if (!isDashing)
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);

        // Keep the run getting faster for as long as the player survives.
        moveSpeed += Mathf.Max(0f, speedIncreaseRate) * Time.deltaTime;
        dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
            HandleJump();

        if (Input.GetKeyDown(KeyCode.Tab) && dashCooldownTimer <= 0f)
            StartDash();

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                transform.localScale = originalScale;
            }
        }
    }

    private bool CheckGrounded()
    {
        if (groundCheck != null)
            return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) != null;

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
            return false;

        Bounds bounds = playerCollider.bounds;
        Vector2 checkSize = new Vector2(
            Mathf.Max(0.1f, bounds.size.x * 0.7f),
            Mathf.Max(0.06f, groundCheckRadius * 0.6f));
        Vector2 checkCenter = new Vector2(bounds.center.x, bounds.min.y - checkSize.y * 0.25f);
        return Physics2D.OverlapBox(checkCenter, checkSize, 0f, groundLayer) != null;
    }

    private void HandleJump()
    {
        if (isGrounded && !isDashing)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            PlaySound(jumpSound);
            return;
        }

        if (isGrounded || doubleJumpItemCount <= 0)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        PlaySound(jumpSound);
        if (doubleJumpEffectPrefab != null)
            Instantiate(doubleJumpEffectPrefab, transform.position, Quaternion.identity);

        doubleJumpItemCount--;
        UpdateDoubleJumpUI();
    }

    private void StartDash()
    {
        isDashing = true;
        dashTimer = Mathf.Max(0f, dashDuration);
        dashCooldownTimer = Mathf.Max(0f, dashCooldown);
        transform.localScale = new Vector3(
            originalScale.x * 1.4f,
            originalScale.y * 0.8f,
            originalScale.z);
        rb.linearVelocity = new Vector2(
            Mathf.Max(dashForce, moveSpeed + Mathf.Max(0f, dashSpeedBonus)),
            rb.linearVelocity.y);
        PlaySound(dashSound);
    }

    public void AddDoubleJumpItem()
    {
        doubleJumpItemCount++;
        UpdateDoubleJumpUI();
    }

    private void UpdateDoubleJumpUI()
    {
        if (doubleJumpText != null)
            doubleJumpText.text = "AIR JUMPS  x" + doubleJumpItemCount;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null || collision.gameObject == null ||
            !collision.gameObject.name.Contains("Obstacle"))
            return;

        PlaySound(hitSound);
        if (cameraShake != null)
            cameraShake.Shake();
        if (gameManager != null)
            gameManager.GameOver();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
