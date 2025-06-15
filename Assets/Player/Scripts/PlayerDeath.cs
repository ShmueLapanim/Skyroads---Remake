using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    public void Die(float restartDelay = 0f)
    {
        DeathEffect();
        GameManager.Instance.RestartLevel(restartDelay);
    }

    private void DeathEffect()
    {
        Destroy(gameObject);
    }
}
