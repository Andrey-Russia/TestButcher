using UnityEngine;

public class WealthChangeZone : MonoBehaviour
{
    [SerializeField] private int amount = 20;

    [Tooltip("Общий объект для взаимоисключающих вариантов ворот.")]
    [SerializeField] private WealthChangeZoneGroup group;

    private bool used;

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (used)
        {
            return;
        }

        PlayerMovement player =
            other.GetComponentInParent<PlayerMovement>();

        if (player == null || !player.IsRunning)
        {
            return;
        }

        PlayerWealth wealth = player.GetComponent<PlayerWealth>();

        if (wealth == null)
        {
            return;
        }

        if (group != null && !group.TryUse())
        {
            return;
        }

        used = true;
        wealth.Add(amount);
    }
}