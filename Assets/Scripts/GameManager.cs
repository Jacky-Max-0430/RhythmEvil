
using UnityEngine;

// GameManager.cs
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    //public UIManager uiManager;

    void Awake() => Instance = this;

    public void PlayerTakeDamage(int damage)
    {
        PlayerController.Instance.health -= damage;
        if (PlayerController.Instance.health <= 0) GameOver();
    }

   

    //public void OnEnemyDefeated() => CheckCompletion();

    public void GameOver()
    { }
}