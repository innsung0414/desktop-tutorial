using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Counts how long the player stays in water and ends the run if the timer expires.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class DrowningController : MonoBehaviour
{
    [SerializeField, Min(1f)] private float secondsToDrown = 10f;

    private readonly HashSet<WaterDeathZone> waterZones = new HashSet<WaterDeathZone>();
    private CharacterController characterController;
    private float secondsInWater;
    private bool isDead;

    public bool IsDead => isDead;
    public bool IsInWater => waterZones.Count > 0;
    public float SecondsRemaining => Mathf.Max(0f, secondsToDrown - secondsInWater);

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (isDead || waterZones.Count == 0)
        {
            return;
        }

        secondsInWater += Time.deltaTime;
        if (secondsInWater >= secondsToDrown)
        {
            Die();
        }
    }

    public void EnterWater(WaterDeathZone zone)
    {
        if (!isDead && zone != null)
        {
            waterZones.Add(zone);
        }
    }

    public void ExitWater(WaterDeathZone zone)
    {
        if (zone == null)
        {
            return;
        }

        waterZones.Remove(zone);
        if (!isDead && waterZones.Count == 0)
        {
            secondsInWater = 0f;
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        waterZones.Clear();

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        DisablePlayerControl<FirstPersonLook>();
        DisablePlayerControl<FirstPersonMovement>();

        // FishingGameController lives on the separate "Fishing Systems" object,
        // so find it in the scene instead of looking for it on the Player.
        FishingGameController fishingController = FindFirstObjectByType<FishingGameController>();
        if (fishingController != null)
        {
            fishingController.StopAllCoroutines();
            fishingController.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("The player drowned.", this);
    }

    private void DisablePlayerControl<T>() where T : Behaviour
    {
        T control = GetComponent<T>();
        if (control != null)
        {
            control.enabled = false;
        }
    }

}
