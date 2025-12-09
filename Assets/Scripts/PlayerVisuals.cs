using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer bodyRenderer;
    public PlayerController playerController;

    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int deathStateHash = Animator.StringToHash("Death");

    void Update()
    {
        animator.SetBool(isWalkingHash, playerController.IsWalking());
        animator.SetBool(isGroundedHash, playerController.IsGrounded());

        switch (playerController.GetFacingDirection())
        {
            case PlayerController.FacingDirection.left:
                bodyRenderer.flipX = true;
                break;
            case PlayerController.FacingDirection.right:
                bodyRenderer.flipX = false;
                break;
        }

        //debug purpose only
        if (Input.GetKeyDown(KeyCode.X))
        {
            animator.Play(deathStateHash);
        }
    }
}
