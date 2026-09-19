using UnityEngine;

public class WealthChangeZoneGroup : MonoBehaviour
{
    private bool used;

    public bool TryUse()
    {
        if (used)
        {
            return false;
        }

        used = true;
        return true;
    }
}