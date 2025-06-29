using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private int score = 0;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score}");
    }
    
    public int GetScore()
    {
        return score;
    }
} 