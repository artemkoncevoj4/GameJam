using UnityEngine;
using TaskSystem;
using System;
using InteractiveObjects;
public class Paper : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    void Start()
    {
        // Сохраняем оригинальные значения
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void OnMouseDown()
    {
        if (Stamp2D.isStumped)
        {
            StampTable.shouldCoroutineStop = true;
        }
    }

}