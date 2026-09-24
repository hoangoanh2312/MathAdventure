using System;
using UnityEngine;

public class GameStats : MonoBehaviour
{
    [SerializeField] private int lives = 3;
    [SerializeField] private int score;
    [SerializeField] private int gold;
    [SerializeField] private int keys;
    [SerializeField] private int requiredKeys = 1;

    public int Lives => lives;
    public int Score => score;
    public int Gold => gold;
    public int Keys => keys;
    public int RequiredKeys => requiredKeys;

    public event Action OnStatsChanged;

    private void Awake()
    {
        lives = 3;
        score = 0;
        gold = 0;
        keys = 0;
        requiredKeys = 1;
    }

    public void AddScore(int amount)
    {
        score += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnStatsChanged?.Invoke();
    }

    public void AddKey(int amount)
    {
        keys += amount;
        OnStatsChanged?.Invoke();
    }
}
