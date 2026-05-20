using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SlimeController : MonoBehaviour {
    [Header("Movement")]
    public float MoveForce;
    public float JumpForce;
    public float MaxSpeed;
    public float Friction;

    [Header("Physics")]
    public Transform GroundCheck;
    public LayerMask GroundLayer;

    [Header("Squish")]
    public float SquishForce;
    public float Elasticity;
    public int SmoothingSamples;

    private Rigidbody2D rb;
    private bool Grounded;
    private float inputX = 0f;
    private bool jumpRequested = false;
    private Vector2 BaseScale;

    private Queue<Vector2> AccelHist = new Queue<Vector2>();
    private Vector2 PrevVel;

    private bool RightInput => Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
    private bool LeftInput => Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
    private bool JumpInput => Keyboard.current.wKey.wasPressedThisFrame || 
                              Keyboard.current.spaceKey.wasPressedThisFrame || 
                              Keyboard.current.upArrowKey.wasPressedThisFrame;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        BaseScale = transform.localScale;
    }

    void Update() {
        if (Keyboard.current == null) return;

        inputX = 0f;
        if (RightInput) inputX = -1f;
        if (LeftInput) inputX = 1f;

        if (JumpInput) {
            jumpRequested = true;
        }
    }

    void FixedUpdate() {
        Grounded = Physics2D.OverlapCircle(GroundCheck.position, 0.2f, GroundLayer);
        rb.AddForce(new Vector2(inputX * MoveForce, 0f), ForceMode2D.Force);

        if (Mathf.Abs(rb.linearVelocityX) > MaxSpeed) {
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocityX) * MaxSpeed, rb.linearVelocityY);
        }

        if (Grounded && inputX == 0) {
            rb.linearVelocityX = Mathf.Lerp(rb.linearVelocityX, 0, Time.fixedDeltaTime * Friction);
        }

        if (jumpRequested) {
            if (Grounded) {
                rb.linearVelocityY = 0f; 
                rb.AddForce(new Vector2(0f, JumpForce), ForceMode2D.Impulse);
            }
            jumpRequested = false;
        }

        Vector2 accel = (rb.linearVelocity - PrevVel) / Time.fixedDeltaTime;
        PrevVel = rb.linearVelocity;

        AccelHist.Enqueue(accel);
        if (AccelHist.Count > SmoothingSamples)
            AccelHist.Dequeue();

        ComputeSquish();
    }

    void ComputeSquish() {
        Vector2 sum = Vector2.zero;
        foreach (Vector2 a in AccelHist){
            sum += a;
        }
        sum /= AccelHist.Count;

        float stretchX = 1f + (Mathf.Abs(sum.x) * SquishForce);
        float stretchY = 1f + (Mathf.Abs(sum.y) * SquishForce);

        Vector2 targetScale = new Vector2(
            BaseScale.x * (stretchX / stretchY), 
            BaseScale.y * (stretchY / stretchX)
        );

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * Elasticity);
    }
}