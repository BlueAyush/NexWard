
using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public enum PickupType
    {
        Experience,
        Scrap
    }

    [Header("Pickup Settings")]
    [SerializeField] private PickupType pickupType;
    [SerializeField] private int amount = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerResources player =
            other.GetComponentInParent<PlayerResources>();

        if (player == null)
            return;

        if (pickupType == PickupType.Experience)
        {
            player.AddXP(amount);
        }
        else if (pickupType == PickupType.Scrap)
        {
            player.AddScrap(amount);
        }

        Destroy(gameObject);
    }
}
