using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Contains all the functionality of a weapon with
/// aiming support and a cooldown timer
/// </summary>
public class Weapon : MonoBehaviour
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
    public bool HasFired { get; set; }
    public int WeaponCooldown { get; set; }

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        handler = GetComponent<InputHandler>();
        crossHairRenderer = crossHair.gameObject.GetComponent<SpriteRenderer>();
        weaponRenderer = weapon.gameObject.GetComponent<SpriteRenderer>();
        cooldownCounterRenderer = cooldownCounter.gameObject.GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        crossHairRenderer.enabled = false;
        cooldownCounterRenderer.enabled = false;
        HasFired = false;
        WeaponCooldown = 0;
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
        if (HasFired) // when fired, enable the counter and reset the cooldown timer
        {
            HasFired = false;
            cooldownCounterRenderer.enabled = true;
            WeaponCooldown = 2;
            StartCoroutine(WeaponCooldownDepletion());
        }
        else
        {
            if (WeaponCooldown <= 0) // the cooldown has ended
                cooldownCounterRenderer.enabled = false;
            else // the cooldown is depleting
                cooldownCounterRenderer.sprite = numberSprite[WeaponCooldown - 1];
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

    #region Coroutines

    /// <summary>
    /// Resets the weapon cooldown and progressively depletes it
    /// </summary>
    private IEnumerator WeaponCooldownDepletion()
    {
        yield return new WaitForSeconds(1f);
        WeaponCooldown--;
        yield return new WaitForSeconds(1f);
        WeaponCooldown--;
    }

    #endregion

}
