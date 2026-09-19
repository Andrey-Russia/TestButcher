using UnityEngine;

namespace ButchersGames
{
    public class Level : MonoBehaviour
    {
        [SerializeField] private Transform playerSpawnPoint;

        public Transform PlayerSpawnPoint => playerSpawnPoint;

        private void OnDrawGizmos()
        {
            if (playerSpawnPoint == null)
            {
                return;
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(playerSpawnPoint.position, 0.3f);

            Gizmos.DrawRay(playerSpawnPoint.position, playerSpawnPoint.forward * 2f);
        }
    }
}