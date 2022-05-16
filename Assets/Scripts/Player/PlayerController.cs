using System.Collections;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(InputHandler))]
public class PlayerController : NetworkBehaviour
{

    #region Variables

    readonly float speed = 3.4f;
    readonly float jumpHeight = 6.5f;
    readonly float gravity = 1.5f;
    readonly int maxJumps = 2;

    LayerMask _layer;
    int _jumpsLeft;

    ContactFilter2D filter;
    InputHandler handler;
    Player player;
    Rigidbody2D rb;
    new CapsuleCollider2D collider;
    Animator anim;
    SpriteRenderer spriteRenderer;

    NetworkVariable<bool> FlipSprite;

    // Firing system variables
    [SerializeField]
    private GameObject bulletPrefab = null;
    private float bulletSpeed = 2f;
    private const int WEAPON_RELOAD_TIME = 2;
    private int weaponCooldown = 0;
    private WeaponCooldownCounter cooldownCounter;
    private bool hasFired = false;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<CapsuleCollider2D>();
        handler = GetComponent<InputHandler>();
        player = GetComponent<Player>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        cooldownCounter = GetComponentInChildren<WeaponCooldownCounter>();

        FlipSprite = new NetworkVariable<bool>();
    }

    private void OnEnable()
    {
        handler.OnMove.AddListener(UpdatePlayerVisualsServerRpc);
        handler.OnJump.AddListener(PerformJumpServerRpc);
        handler.OnFire.AddListener(FireBulletServerRpc);
        handler.OnMoveFixedUpdate.AddListener(UpdatePlayerPositionServerRpc);

        FlipSprite.OnValueChanged += OnFlipSpriteValueChanged;
    }

    private void OnDisable()
    {
        handler.OnMove.RemoveListener(UpdatePlayerVisualsServerRpc);
        handler.OnJump.RemoveListener(PerformJumpServerRpc);
        handler.OnFire.RemoveListener(FireBulletServerRpc);
        handler.OnMoveFixedUpdate.RemoveListener(UpdatePlayerPositionServerRpc);

        FlipSprite.OnValueChanged -= OnFlipSpriteValueChanged;
    }

    void Start()
    {
        // Configure Rigidbody2D
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = gravity;

        // Configure LayerMask
        _layer = LayerMask.GetMask("Obstacles");

        // Configure ContactFilter2D
        filter.minNormalAngle = 45;
        filter.maxNormalAngle = 135;
        filter.useNormalAngle = true;
        filter.layerMask = _layer;
    }

    void Update()
    {
        if (hasFired)
        {
            hasFired = false;
            // Restart weapon cooldown
            StartCoroutine(RestartWeaponCooldown());
        }
    }

    #endregion

    #region RPC

    #region ServerRPC

    [ServerRpc]
    void UpdatePlayerVisualsServerRpc(Vector2 input)
    {
        UpdateAnimatorStateServerRpc();
        UpdateSpriteOrientation(input);
    }

    [ServerRpc]
    void UpdateAnimatorStateServerRpc()
    {
        if (IsGrounded)
        {
            anim.SetBool("isGrounded", true);
            anim.SetBool("isJumping", false);
        }
        else
        {
            anim.SetBool("isGrounded", false);
        }
    }

    [ServerRpc]
    void PerformJumpServerRpc()
    {
        if (IsGrounded)
        {
            _jumpsLeft = maxJumps;
        }
        else if (_jumpsLeft == 0)
        {
            return;
        }

        player.State.Value = PlayerState.Jumping;
        anim.SetBool("isJumping", true);
        rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
        _jumpsLeft--;
    }


    /// <summary>
    /// Fire a bullet in the direction of aim
    /// </summary>
    [ServerRpc]
    private void FireBulletServerRpc(Vector2 input)
    {
        if (weaponCooldown <= 0) // only shoot if weapon is charged
        {
            hasFired = true;
            // Instantiate a bullet
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            // Change instance's rigidbody velocity
            Vector2 direction = (input - (Vector2)transform.position).normalized;
            bullet.GetComponent<Rigidbody2D>().velocity = direction * bulletSpeed;
            // Spawn the bullet
            bullet.GetComponent<NetworkObject>().Spawn();
            // Change bullet shooter id
            bullet.GetComponent<Bullet>().ShooterId.Value = NetworkObjectId;
        }
    }

    [ServerRpc]
    void UpdatePlayerPositionServerRpc(Vector2 input)
    {
        if (IsGrounded)
        {
            player.State.Value = PlayerState.Grounded;
        }

        if (player.State.Value != PlayerState.Hooked)
        {
            rb.velocity = new Vector2(input.x * speed, rb.velocity.y);
        }
    }

    #endregion

    #endregion

    #region Methods

    void UpdateSpriteOrientation(Vector2 input)
    {
        if (input.x < 0)
        {
            FlipSprite.Value = false;
        }
        else if (input.x > 0)
        {
            FlipSprite.Value = true;
        }
    }

    void OnFlipSpriteValueChanged(bool previous, bool current)
    {
        spriteRenderer.flipX = current;
    }

    bool IsGrounded => collider.IsTouching(filter);

    #endregion

    #region Coroutines

    /// <summary>
    /// Resets the weapon cooldown to reload time and progressively depletes it
    /// </summary>
    private IEnumerator RestartWeaponCooldown()
    {
        weaponCooldown = WEAPON_RELOAD_TIME;
        cooldownCounter.ResetCooldown(WEAPON_RELOAD_TIME);
        while (weaponCooldown > 0)
        {
            yield return new WaitForSeconds(1f);
            weaponCooldown--;
            cooldownCounter.DepleteCooldown();
        }
        hasFired = false;
    }

    #endregion

}
