using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isJumping = false;
    private float horizontalInput;

    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpForce = 10f;

    [Header("Sprite Settings")]
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleInput();
        FlipSprite(horizontalInput);
    }

    void FixedUpdate()
    {
        MovePlayer();
        if (isJumping)
        {
            Jump();
            isJumping = false;
        }
    }

    void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        animator.SetFloat("Speed",Mathf.Abs(horizontalInput));

        if (Input.GetKeyDown(KeyCode.Space) && !isGrounded())
        {
            isJumping = true;
            animator.SetBool("IsJumping", isJumping);
        }
    }

    void MovePlayer()
    {
        Vector2 velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
        rb.velocity = velocity;
    }

    void Jump()
    {
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
    }

    void FlipSprite(float horizontalInput)
    {
        if (horizontalInput >= 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (horizontalInput < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private bool isGrounded()
    {

        Transform groundCheck = transform.Find("FloorCheck");
        float checkRadius = 0.1f;
        LayerMask groundLayer = LayerMask.GetMask("Floor");

        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            isJumping = false;
            animator.SetBool("IsJumping", isJumping);
        }
    }
}
