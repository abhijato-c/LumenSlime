using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class SlimeController : MonoBehaviour {
    [Header("Movement")]
    public float MoveForce;
    public float JumpForce;
    public float DownForce;
    public float MaxSpeed;
    public float Friction;
    public float PadForce;

    [Header("Physics")]
    public Transform GroundCheck;
    public LayerMask GroundLayer;

    [Header("Squish")]
    public float SquishForce;
    public float Elasticity;
    public int SmoothingSamples;

    [Header("Misc")]
    public GameObject EndFrame;
    public float ColorLerpDuration;
    public InnerParallax ParallaxScript;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private SpriteRenderer sr;
    private bool Grounded;
    private float inputX = 0f;
    private bool Jumping = false;
    private bool Downing = false;
    private Vector2 BaseScale;
    private Vector2 BaseCol;
    private int CpIndex = 0;

    private Queue<Vector2> AccelHist;
    private Vector2 PrevVel;

    private bool RightInput => Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed;
    private bool LeftInput => Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed;
    private bool UpInput => Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;
    private bool DownInput => Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        sr = GetComponent<SpriteRenderer>();

        BaseScale = transform.localScale;
        BaseCol = col.size;
        AccelHist = new Queue<Vector2>(Enumerable.Repeat(Vector2.zero, SmoothingSamples));
        
        if (PlayerPrefs.HasKey("CheckpointIndex")) {
            CpIndex = PlayerPrefs.GetInt("CheckpointIndex");
            ParallaxScript.SetIndex(CpIndex, FindCheckpointTransform(CpIndex).y);
            MoveToCheckpoint(CpIndex);
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
            GameObject touched = other.gameObject;
            int ind = int.Parse(touched.transform.name.Split("-")[1]);
            MoveToCheckpoint(ind);
            CpIndex = ind;
            if (ind > PlayerPrefs.GetInt("CheckpointIndex")) 
                PlayerPrefs.SetInt("CheckpointIndex", CpIndex);
        }
        else if (other.CompareTag("ManaOrb")) {
            other.gameObject.SetActive(false);
        }
        else if (other.CompareTag("Kill")) {
            StartCoroutine(AnimateColor());
            MoveToCheckpoint(CpIndex);
        }
        else if (other.CompareTag("Jump")) {
            rb.linearVelocityY = 0f;
            rb.AddForce(new Vector2(0f, PadForce), ForceMode2D.Impulse);
        }
    }

    private Vector3 FindCheckpointTransform(int ind) {
        if (ind != 0)
            return GameObject.Find($"Checkpoint-{ind}").transform.position + Vector3.up * 6f;
        else
            return new Vector3(0, 0, 0);
    }

    public void MoveToCheckpoint(int checkpoint) {
        Vector3 trans = FindCheckpointTransform(checkpoint);
        transform.position = trans;
        rb.linearVelocity = Vector2.zero;
        ParallaxScript.SetIndex(checkpoint, trans.y);

        if (checkpoint == 5)
            EndFrame.SetActive(true);
    }

    IEnumerator AnimateColor() {
        float elapsed = 0f;
        while (elapsed < ColorLerpDuration) {
            elapsed += Time.deltaTime; 
            sr.color = Color.Lerp(Color.white, Color.red, elapsed / ColorLerpDuration);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < ColorLerpDuration) {
            elapsed += Time.deltaTime; 
            sr.color = Color.Lerp(Color.red, Color.white, elapsed / ColorLerpDuration);
            yield return null;
        }
    }


    public void Quit(){
        Application.Quit();
    }
}