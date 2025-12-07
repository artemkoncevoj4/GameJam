using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BunnyManager : MonoBehaviour
{
    [SerializeField] private Bunny _bunny;
    
    void Start()
    {
        // Подписка на события GameCycle
        if (GameCycle.Instance != null)
        {
            GameCycle.Instance.OnRabbitAppearing += OnRabbitAppear;
            GameCycle.Instance.OnRabbitLeaving += OnRabbitLeave;
        }
    }
    
    void OnDestroy()
    {
        if (GameCycle.Instance != null)
        {
            GameCycle.Instance.OnRabbitAppearing -= OnRabbitAppear;
            GameCycle.Instance.OnRabbitLeaving -= OnRabbitLeave;
        }
    }
    
    private void OnRabbitAppear()
    {
        if (_bunny != null && !_bunny.IsActive)
        {
            _bunny.Appear();
        }
    }
    
    private void OnRabbitLeave()
    {
        if (_bunny != null && _bunny.IsActive)
        {
            _bunny.Leave();
        }
    }
    
    // Для тестирования (вызывать из кнопки UI)
    public void TestSpawnBunny()
    {
        if (_bunny != null)
        {
            _bunny.Appear();
        }
    }
}
