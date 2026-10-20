using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Marks a water volume that can start the player's drowning countdown.
/// </summary>
[RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
public class WaterDeathZone : MonoBehaviour
{
    private readonly HashSet<DrowningController> occupants = new HashSet<DrowningController>();

    private void Reset()
    {
        ConfigureTrigger();
    }

    private void Awake()
    {
        ConfigureTrigger();
    }

    private void ConfigureTrigger()
    {
        GetComponent<BoxCollider>().isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void OnTriggerEnter(Collider other)
    {
        DrowningController player = other.GetComponentInParent<DrowningController>();
        if (player != null && occupants.Add(player))
        {
            player.EnterWater(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DrowningController player = other.GetComponentInParent<DrowningController>();
        if (player != null && occupants.Remove(player))
        {
            player.ExitWater(this);
        }
    }

    private void OnDisable()
    {
        foreach (DrowningController player in occupants)
        {
            if (player != null)
            {
                player.ExitWater(this);
            }
        }

        occupants.Clear();
    }
}
