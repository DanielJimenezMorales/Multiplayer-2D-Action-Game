using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains all the functionality of a weapon with
/// aiming support and a cooldown timer
/// </summary>
public class Weapon : NetworkBehaviour
{

    #region Variables

    [SerializeField] Transform crossHair;
    [SerializeField] Transform weapon;
    [SerializeField] Transform cooldownCounter;

    // renderers and sprites
    SpriteRenderer crossHairRenderer;
    SpriteRenderer weaponRenderer;
    [SerializeField] List<Sprite> numberSprite = new List<Sprite>(2);
    SpriteRenderer cooldownCounterRenderer;

    // input handler
    InputHandler handler;

    #endregion

    #region Network Variables

    NetworkVariable<bool> HasFired;
    NetworkVariable<int> WeaponCooldown;

    #endregion

    #region Constants

    const int RELOAD_TIME = 4;

    #endregion

    #region Getters and Setters

    public void SetHasFired(bool hasFired)
    {
        HasFired.Value = hasFired;
    }

    public int GetWeaponCooldown()
    {
        return WeaponCooldown.Value;
    }

    #endregion

    #region Unity Event Functions

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        HasFired.OnValueChanged += OnHasFiredValueChanged;
        WeaponCooldown.OnValueChanged += OnWeaponCooldownValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        HasFired.OnValueChanged -= OnHasFiredValueChanged;
        WeaponCooldown.OnValueChanged -= OnWeaponCooldownValueChanged;
    }

    private void Awake()
    {
        handler = GetComponent<InputHandler>();
        crossHairRenderer = crossHair.gameObject.GetComponent<SpriteRenderer>();
        weaponRenderer = weapon.gameObject.GetComponent<SpriteRenderer>();
        cooldownCounterRenderer = cooldownCounter.gameObject.GetComponent<SpriteRenderer>();

        HasFired = new NetworkVariable<bool>();
        WeaponCooldown = new NetworkVariable<int>();
    }

    private void Start()
    {
        crossHairRenderer.enabled = false;
        cooldownCounterRenderer.enabled = false;
        if (IsServer) // the server must reset the cooldown
        {
            HasFired.Value = false;
            WeaponCooldown.Value = 0;
        }
    }

    private void OnEnable()
    {
        handler.OnMousePosition.AddListener(UpdateCrosshairPosition);
    }

    private void OnDisable()
    {
        handler.OnMousePosition.RemoveListener(UpdateCrosshairPosition);
    }

    private void Update()
    {
        if (HasFired.Value) // when fired, the server enables the counter and resets the cooldown timer
        {
            if (IsServer)
            {
                HasFired.Value = false;
                WeaponCooldown.Value = RELOAD_TIME - 1;
                StartCoroutine(WeaponCooldownDepletion());
            }
        }
        else
        {
            if (WeaponCooldown.Value <= 0)  // the cooldown has ended, the countdown must be hidden
            {
                cooldownCounterRenderer.enabled = false;
            }
            else // the cooldown is depleting, the countdown must be shown
            {
                cooldownCounterRenderer.enabled = true;
                cooldownCounterRenderer.sprite = numberSprite[WeaponCooldown.Value - 1];
            }
        }
    }

    #endregion

    #region Methods

    void UpdateCrosshairPosition(Vector2 input)
    {
        var worldMousePosition = Camera.main.ScreenToWorldPoint(input);
        var facingDirection = worldMousePosition - transform.position;
        var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
        if (aimAngle < 0f)
        {
            aimAngle = Mathf.PI * 2 + aimAngle;
        }

        crossHairRenderer.enabled = true;

        SetCrossHairPosition(aimAngle);
    
        UpdateWeaponOrientation();
    }

    void UpdateWeaponOrientation()
    {
        weapon.right = crossHair.position - weapon.position;

        if (crossHair.localPosition.x > 0)
        {
            weaponRenderer.flipY = false;
        }
        else
        {
            weaponRenderer.flipY = true;
        }
    }

    void SetCrossHairPosition(float aimAngle)
    {
        var x = transform.position.x + .5f * Mathf.Cos(aimAngle);
        var y = transform.position.y + .5f * Mathf.Sin(aimAngle);

        var crossHairPosition = new Vector3(x, y, 0);
        crossHair.transform.position = crossHairPosition;
    }

    #endregion

    #region Netcode Related Methods

    void OnHasFiredValueChanged(bool previous, bool current)
    {
        HasFired.Value = current;
    }

    void OnWeaponCooldownValueChanged(int previous, int current)
    {
        WeaponCooldown.Value = current;
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Depletes the weapon cooldown
    /// </summary>
    private IEnumerator WeaponCooldownDepletion()
    {
        while (WeaponCooldown.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            WeaponCooldown.Value--;
        }
    }

    #endregion
}
