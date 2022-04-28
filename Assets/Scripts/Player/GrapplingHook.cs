using UnityEngine;
using Unity.Netcode;


/// <summary>
/// This class represents the grappling hook equipment in the game
/// </summary>
public class GrapplingHook : NetworkBehaviour
{
    #region Variables

    InputHandler handler;
    DistanceJoint2D rope;
    LineRenderer ropeRenderer;
    Transform playerTransform;
    [SerializeField] Material material;
    LayerMask layer;
    Player player;

    readonly float climbSpeed = 2f;
    readonly float swingForce = 80f;

    Rigidbody2D rb;

    #endregion

    #region Network Variables

    NetworkVariable<bool> ropeVisibility;
    NetworkVariable<int> ropePositionIndex; // indicates whether we are setting the
                                            // position of the origin of the rope (0) or the end (1)
    NetworkVariable<Vector2> ropePosition;  // stores the position of either the origin or the end
                                            // of the rope, which depends on the ropePositionIndex value
    NetworkVariable<bool> ropeEnabled;      // true if the DistanceJoint2D rope is enabled
    NetworkVariable<float> ropeDistance;
    NetworkVariable<Vector2> connectedAnchor;

    #endregion

    #region Unity Event Functions

    void Awake()
    {
        handler = GetComponent<InputHandler>();
        player = GetComponent<Player>();

        //Configure RopeRenderer (LineRenderer)
        ropeRenderer = gameObject.AddComponent<LineRenderer>();
        ropeRenderer.startWidth = .05f;
        ropeRenderer.endWidth = .05f;
        ropeRenderer.material = material;
        ropeRenderer.sortingOrder = 3;
        ropeRenderer.enabled = false;

        // Configure Rope (DistanceJoint2D)
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

    [ServerRpc]
    void UpdateHookServerRpc(Vector2 input)
    {
        if (player.State.Value == PlayerState.Hooked)
        {
            // Allow player to climb the rope
            ClimbRope(input.y);
            // Update origin position of the rope to match the player position
            SetRopePosition(0, playerTransform.position);
        }
        else if (player.State.Value == PlayerState.Grounded)
        {
            // Disable rope
            ChangeRopeState(false);
            ChangeRopeVisibility(false);
        }
    }

    [ServerRpc]
    void JumpPerformedServerRpc()
    {
        // Disable rope
        ChangeRopeState(false);
        ChangeRopeVisibility(false);
    }

    [ServerRpc]
    void LaunchHookServerRpc(Vector2 input)
    {
        var hit = Physics2D.Raycast(playerTransform.position, input - (Vector2)playerTransform.position, Mathf.Infinity, layer);

        if (hit.collider)
        {
            // Connect rope to anchor
            var anchor = hit.centroid;
            ConnectAnchor(anchor);
            SetRopePosition(1, anchor);
            SetConnectedAnchorClientRpc(anchor); // this method makes sure that the client updates its anchor point 
            ChangeRopeState(true);
            ChangeRopeVisibility(true);
            player.State.Value = PlayerState.Hooked;
        }
    }

    [ServerRpc]
    void SwingRopeServerRpc(Vector2 input)
    {
        if (player.State.Value == PlayerState.Hooked)
        {
            // Compute player to hook direction
            var direction = (connectedAnchor.Value - (Vector2)playerTransform.position).normalized;

            // Compute the perpendicular direction
            var forceDirection = new Vector2(input.x * direction.y, direction.x);

            // Compute the force
            var force = forceDirection * swingForce;

            // Add swing force to player rigidbody
            rb.AddForce(force, ForceMode2D.Force);
        }
    }

    #endregion

    #region ClientRPC

    [ClientRpc]
    void SetConnectedAnchorClientRpc(Vector2 anchor)
    {
        ropeRenderer.SetPosition(1, anchor);
    }

    #endregion

    #endregion

    #region Methods

    void ClimbRope(float input)
    {
        ropeDistance.Value = (input) * climbSpeed * Time.deltaTime;
    }

    void OnRopeDistanceValueChanged(float previous, float current)
    {
        rope.distance -= current;
    }

    void ConnectAnchor(Vector2 anchor)
    {
        connectedAnchor.Value = anchor;
    }

    void OnConnectedAnchorValueChanged(Vector2 previous, Vector2 current)
    {
        rope.connectedAnchor = current;
    }

    public void ChangeRopeState(bool state)
    {
        ropeEnabled.Value = state;
    }

    void OnRopeStateValueChanged(bool previous, bool current)
    {
        rope.enabled = current;
    }

    void ChangeRopeVisibility(bool state)
    {
        ropeVisibility.Value = state;
    }

    void OnRopeVisibilityValueChanged(bool previous, bool current)
    {
        ropeRenderer.enabled = current;
    }

    void SetRopePosition(int index, Vector2 position)
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
