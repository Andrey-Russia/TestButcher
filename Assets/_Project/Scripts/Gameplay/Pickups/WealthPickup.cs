using UnityEngine;

public class WealthPickup : MonoBehaviour
{
    [SerializeField] private int value = 10;

    private bool collected;

    private void Reset()
    {
        SphereCollider trigger = GetComponent<SphereCollider>();
        trigger.isTrigger = true;
        trigger.radius = 0.45f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected)
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

        collected = true;
        wealth.Add(value);

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}