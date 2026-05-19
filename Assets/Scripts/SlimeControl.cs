using UnityEngine;

public class SlimeController : MonoBehaviour {
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    public float wallSlideSpeed = 2f;

    [Header("Physics")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private bool isWalled;
    private bool isWallClinging;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Wall Jump logic triggers here
        if (Input.GetButtonDown("Jump") && isWallClinging)
        {
            // Launch away from the wall
            rb.linearVelocity = new Vector2(-horizontalInput * moveSpeed, jumpForce);
        }
    }

    void FixedUpdate()
    {
        // 2. Apply Horizontal Movement safely
        if (!isWallClinging)
        {
            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        }

        // 3. Environmental Checks using small invisible overlap circles
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        isWalled = Physics2D.OverlapCircle(wallCheck.position, 0.2f, groundLayer);

        // 4. Wall Cling Logic
        if (isWalled && !isGrounded && horizontalInput != 0)
        {
            isWallClinging = true;
            // Slow down his descent to simulate "sticky slime friction"
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else
        {
            isWallClinging = false;
        }
    }
}