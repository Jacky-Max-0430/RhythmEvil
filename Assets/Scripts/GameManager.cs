
using UnityEngine;

// GameManager.cs
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    //public UIManager uiManager;

    void Awake() => Instance = this;

   

   

    //public void OnEnemyDefeated() => CheckCompletion();

    public void GameOver()
    { }
}