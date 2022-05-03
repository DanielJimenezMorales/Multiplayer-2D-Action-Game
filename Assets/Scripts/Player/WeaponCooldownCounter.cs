using System.Collections.Generic;
using UnityEngine;

public class WeaponCooldownCounter : MonoBehaviour
{
    [SerializeField] List<Sprite> numberSprite = new List<Sprite>(2);
    SpriteRenderer spriteRenderer;
    int cooldownCounter;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        spriteRenderer.enabled = false;
    }

    public void ResetCooldown(int reloadTime)
    {
        cooldownCounter = reloadTime--;
        spriteRenderer.sprite = numberSprite[--cooldownCounter];
        spriteRenderer.enabled = true;
    }

    public void DepleteCooldown()
    {
        if (cooldownCounter > 0)
            spriteRenderer.sprite = numberSprite[--cooldownCounter];
        else
            spriteRenderer.enabled = false;
    }
}
