using UnityEngine;
using Unity.Netcode;

public class GrapplingHook : NetworkBehaviour
{
    #region Variables

    InputHandler handler;
    // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/DistanceJoint2D.html
    DistanceJoint2D rope;
    // // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/LineRenderer.html
    LineRenderer ropeRenderer;
    Transform playerTransform;
    [SerializeField] Material material;
    // https://docs.unity3d.com/2020.3/Documentation/ScriptReference/LayerMask.html
    LayerMask layer;
    Player player;

    readonly float climbSpeed = 2f;
    readonly float swingForce = 80f;

    Rigidbody2D rb;

    // https://docs-multiplayer.unity3d.com/netcode/current/basics/networkvariable
    NetworkVariable<bool> ropeVisibility;
    NetworkVariable<int> ropePositionIndex;
    NetworkVariable<Vector2> ropePosition;
    NetworkVariable<bool> ropeEnabled;
    NetworkVariable<float> ropeDistance;
    NetworkVariable<Vector2> connectedAnchor;

    #endregion

    #region Unity Event Functions

    void Awake()
    {
        handler = GetComponent<InputHandler>();
        player = GetComponent<Player>();

        //Configure Rope Renderer
        ropeRenderer = gameObject.AddComponent<LineRenderer>();
        ropeRenderer.startWidth = .05f;
        ropeRenderer.endWidth = .05f;
        ropeRenderer.material = material;
        ropeRenderer.sortingOrder = 3;
        ropeRenderer.enabled = false;

        // Configure Rope
        rope = gameObject.AddComponent<DistanceJoint2D>();
        rope.enableCollision = true;
        rope.enabled = false;

        playerTransform = transform;
        layer = LayerMask.GetMask("Obstacles");

        rb = GetComponent<Rigidbody2D>();

        ropeVisibility = new NetworkVariable<bool>();
        ropePosition = new NetworkVariable<Vector2>();
        ropePositionIndex = new NetworkVariable<int>();
        ropeEnabled = new NetworkVariable<bool>();
        ropeDistance = new NetworkVariable<float>();
        connectedAnchor = new NetworkVariable<Vector2>();
    }

    private void OnEnable()
    {
        handler.OnHookRender.AddListener(UpdateHookServerRpc);
        handler.OnMoveFixedUpdate.AddListener(SwingRopeServerRpc);
        handler.OnJump.AddListener(JumpPerformedServerRpc);
        handler.OnHook.AddListener(LaunchHookServerRpc);
    }

    public override void OnNetworkSpawn()
    {
        ropeVisibility.OnValueChanged += OnRopeVisibilityValueChanged;
        ropePosition.OnValueChanged += OnRopePositionValueChanged;
        ropeEnabled.OnValueChanged += OnRopeStateValueChanged;
        ropeDistance.OnValueChanged += OnRopeDistanceValueChanged;
        connectedAnchor.OnValueChanged += OnConnectedAnchorValueChanged;
    }

    private void OnDisable()
    {
        handler.OnHookRender.RemoveListener(UpdateHookServerRpc);
        handler.OnMoveFixedUpdate.RemoveListener(SwingRopeServerRpc);
        handler.OnJump.RemoveListener(JumpPerformedServerRpc);
        handler.OnHook.RemoveListener(LaunchHookServerRpc);
    }

    public override void OnNetworkDespawn()
    {
        ropeVisibility.OnValueChanged -= OnRopeVisibilityValueChanged;
        ropePosition.OnValueChanged -= OnRopePositionValueChanged;
        ropeEnabled.OnValueChanged -= OnRopeStateValueChanged;
        ropeDistance.OnValueChanged -= OnRopeDistanceValueChanged;
        connectedAnchor.OnValueChanged -= OnConnectedAnchorValueChanged;
    }

    #endregion

    #region Netcode RPC

    #region ServerRPC

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void UpdateHookServerRpc(Vector2 input)
    {
        if (player.State.Value == PlayerState.Hooked)
        {
            ClimbRopeServerRpc(input.y);
            SetRopePositionServerRpc(0, playerTransform.position);
        }
        else if (player.State.Value == PlayerState.Grounded)
        {
            ChangeRopeStateServerRpc(false);
            ChangeRopeVisibilityServerRpc(false);
        }
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void JumpPerformedServerRpc()
    {
        ChangeRopeStateServerRpc(false);
        ChangeRopeVisibilityServerRpc(false);
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void LaunchHookServerRpc(Vector2 input)
    {
        var hit = Physics2D.Raycast(playerTransform.position, input - (Vector2)playerTransform.position, Mathf.Infinity, layer);

        if (hit.collider)
        {
            var anchor = hit.centroid;
            ConnectAnchorServerRpc(anchor);
            SetRopePositionServerRpc(1, anchor);
            //UpdateRopeClientRpc(anchor);
            ChangeRopeStateServerRpc(true);
            ChangeRopeVisibilityServerRpc(true);
            player.State.Value = PlayerState.Hooked;
        }
    }

    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/serverrpc
    [ServerRpc]
    void SwingRopeServerRpc(Vector2 input)
    {
        if (player.State.Value == PlayerState.Hooked)
        {
            // Player 2 hook direction
            var direction = (connectedAnchor.Value - (Vector2)playerTransform.position).normalized;

            // Perpendicular direction
            var forceDirection = new Vector2(input.x * direction.y, direction.x);

            var force = forceDirection * swingForce;

            rb.AddForce(force, ForceMode2D.Force);
        }
    }

    #endregion

    #region ClientRPC

    /*
    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/clientrpc
    [ClientRpc]
    void UpdateAnchorClientRpc(Vector2 anchor)
    {
        rope.connectedAnchor = anchor;
        ShowRopeClientRpc();
        ropeRenderer.SetPosition(1, anchor);
    }
    */

    /*
    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/clientrpc
    [ClientRpc]
    void UpdateRopeClientRpc(Vector2 anchor)
    {
        ropeRenderer.SetPosition(1, anchor);
    }
    */

    /*
    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/clientrpc
    [ClientRpc]
    void ShowRopeClientRpc()
    {
        rope.enabled = true;
        ropeRenderer.enabled = true;
    }
    */

    /*
    // https://docs-multiplayer.unity3d.com/netcode/current/advanced-topics/message-system/clientrpc
    [ClientRpc]
    void RemoveRopeClientRpc()
    {
        rope.enabled = false;
        ropeRenderer.enabled = false;
    }
    */

    #endregion

    #endregion

    #region Methods

    [ServerRpc]
    public void ClimbRopeServerRpc(float input)
    {
        ropeDistance.Value = (input) * climbSpeed * Time.deltaTime;
    }

    void OnRopeDistanceValueChanged(float previous, float current)
    {
        rope.distance -= current;
    }

    [ServerRpc]
    public void ConnectAnchorServerRpc(Vector2 anchor)
    {
        connectedAnchor.Value = anchor;
    }

    void OnConnectedAnchorValueChanged(Vector2 previous, Vector2 current)
    {
        rope.connectedAnchor = current;
    }

    [ServerRpc]
    public void ChangeRopeStateServerRpc(bool state)
    {
        ropeEnabled.Value = state;
    }

    void OnRopeStateValueChanged(bool previous, bool current)
    {
        rope.enabled = current;
    }

    [ServerRpc]
    public void ChangeRopeVisibilityServerRpc(bool state)
    {
        ropeVisibility.Value = state;
    }

    void OnRopeVisibilityValueChanged(bool previous, bool current)
    {
        ropeRenderer.enabled = current;
    }

    [ServerRpc]
    public void SetRopePositionServerRpc(int index, Vector2 position)
    {
        ropePositionIndex.Value = index;
        ropePosition.Value = position;
    }

    void OnRopePositionValueChanged(Vector2 previous, Vector2 current)
    {
        ropeRenderer.SetPosition(ropePositionIndex.Value, current);
    }

    #endregion
}
