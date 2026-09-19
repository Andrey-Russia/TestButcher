using UnityEngine;

public class GameOutcomeZone : MonoBehaviour
{
    [SerializeField] private bool isFinish;

    private GameManager gameManager;

    public void Initialize(GameManager manager)
    {
        gameManager = manager;
    }

    private void Reset()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameManager == null)
        {
            return;
        }


        if (other.GetComponentInParent<PlayerMovement>() == null)
        {
            return;
        }

        if (isFinish)
        {
            gameManager.Win();
        }
        else
        {
            gameManager.Lose();
        }
    }
}