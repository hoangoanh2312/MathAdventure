using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 3f;

    private Rigidbody2D body;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.down;
    private string currentAnimation;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        body.gravityScale = 0f;
        body.freezeRotation = true;
    }

    private void Update()
    {
        ReadMovementInput();
        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        body.MovePosition(body.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    private void ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            moveInput = Vector2.zero;
            return;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) horizontal -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) horizontal += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) vertical -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) vertical += 1f;

        moveInput = new Vector2(horizontal, vertical).normalized;

        if (moveInput != Vector2.zero)
        {
            // The stronger axis decides the facing direction while moving diagonally.
            lastDirection = Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y)
                ? new Vector2(Mathf.Sign(moveInput.x), 0f)
                : new Vector2(0f, Mathf.Sign(moveInput.y));
        }
    }

    private void UpdateAnimation()
    {
        string prefix = moveInput == Vector2.zero ? "Idle" : "Walk";
        string direction;

        if (lastDirection.y > 0f) direction = "Up";
        else if (lastDirection.y < 0f) direction = "Down";
        else if (lastDirection.x < 0f) direction = "Left";
        else direction = "Right";

        PlayAnimation(prefix + direction);
    }

    private void PlayAnimation(string animationName)
    {
        if (currentAnimation == animationName) return;

        currentAnimation = animationName;
        animator.Play(animationName);
    }
}
