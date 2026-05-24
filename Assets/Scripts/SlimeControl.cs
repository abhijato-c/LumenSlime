using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;

public class SlimeController : MonoBehaviour {
    [Header("Movement")]
    public float MoveForce;
    public float JumpForce;
    public float DownForce;
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
    private BoxCollider2D col;
    private bool Grounded;
    private float inputX = 0f;
    private bool Jumping = false;
    private bool Downing = false;
    private Vector2 BaseScale;
    private Vector2 BaseCol;
    private GameObject LatestCheckpoint;
    private int CpIndex = 0;

    private Queue<Vector2> AccelHist;
    private Vector2 PrevVel;

    private bool RightInput => Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
    private bool LeftInput => Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
    private bool UpInput => Keyboard.current.wKey.wasPressedThisFrame || 
                              Keyboard.current.spaceKey.wasPressedThisFrame || 
                              Keyboard.current.upArrowKey.wasPressedThisFrame;
    private bool DownInput => Keyboard.current.sKey.wasPressedThisFrame || 
                              Keyboard.current.downArrowKey.wasPressedThisFrame;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        BaseScale = transform.localScale;
        BaseCol = col.size;
        AccelHist = new Queue<Vector2>(Enumerable.Repeat(Vector2.zero, SmoothingSamples));
        if (PlayerPrefs.HasKey("CheckpointIndex")) {
            CpIndex = PlayerPrefs.GetInt("CheckpointIndex");
            LatestCheckpoint = GameObject.Find($"Checkpoint-{CpIndex}");
            MoveToCheckpoint();
        }
    }

    void Update() {
        if (Keyboard.current == null) return;

        inputX = 0f;
        if (RightInput) inputX = -1f;
        if (LeftInput) inputX = 1f;
        if (UpInput) Jumping = true;
        if (DownInput) Downing = true;
    }

    void FixedUpdate() {
        Grounded = Physics2D.OverlapCircle(GroundCheck.position, 0.1f, GroundLayer);
        rb.AddForce(new Vector2(inputX * MoveForce, 0f), ForceMode2D.Force);

        if (Mathf.Abs(rb.linearVelocityX) > MaxSpeed) {
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocityX) * MaxSpeed, rb.linearVelocityY);
        }

        if (Grounded && inputX == 0) {
            rb.linearVelocityX = Mathf.Lerp(rb.linearVelocityX, 0, Time.fixedDeltaTime * Friction);
        }

        if (Jumping) {
            if (Grounded)
                rb.AddForce(new Vector2(0f, JumpForce), ForceMode2D.Impulse);
            Jumping = false;
        }

        if (Downing) {
            if (!Grounded)
                rb.AddForce(new Vector2(0f, -DownForce), ForceMode2D.Impulse);
            Downing = false;
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
        col.size = transform.localScale * BaseCol;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Respawn")) {
            LatestCheckpoint = other.gameObject;
            int ind = int.Parse(LatestCheckpoint.transform.name.Split("-")[1]);
            if (ind > CpIndex) {
                CpIndex = ind;
                PlayerPrefs.SetInt("CheckpointIndex", CpIndex);
                Debug.Log($"Checkpoint {CpIndex} reached");
            }
        }
    }

    private void MoveToCheckpoint() {
        if (LatestCheckpoint != null) {
            transform.position = LatestCheckpoint.transform.position + Vector3.up * 6;
            rb.linearVelocity = Vector2.zero;
        }
    }
}