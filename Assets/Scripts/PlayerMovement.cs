using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour, IPlayerLeft
{
    private CharacterController _controller;
    public float PlayerSpeed = 2f;
    public int PlayerId;

    InputAction MoveAction;
    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        MoveAction = InputSystem.actions.FindAction("Move");
    }

    public override void FixedUpdateNetwork()
    {
        // Only move own player and not every other player. Each player controls its own player object.
        if (HasStateAuthority == false)
        {
            return;
        }
        var vector = MoveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(vector.x, 0, vector.y) * Runner.DeltaTime * PlayerSpeed;

        _controller.Move(move);

        if (move != Vector3.zero)
        {
            gameObject.transform.forward = move;
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (player.PlayerId == PlayerId)
        {
            Destroy(gameObject);
        }
    }
}
