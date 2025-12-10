using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player 
{
    public class MovementPlayer : MonoBehaviour
    {
        public float moveSpeed = 125f;
        
        private Rigidbody2D rb;
        private float inverTimer = 0f;
        public static bool invertControls = false;
        
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        void Update()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            
            // Простая инверсия
            if (invertControls)
            {
                horizontal = -horizontal;
                vertical = -vertical;
            }
            
            Vector2 movement = new Vector2(horizontal, vertical).normalized;
            rb.linearVelocity = movement * moveSpeed;
            
            // Тест: переключение по клавише I
            if (Input.GetKeyDown(KeyCode.I))
            {
                invertControls = !invertControls;
                Debug.Log($"Инверсия: {invertControls}");
            }

            if (invertControls)
            {
                inverTimer += Time.deltaTime;
            }
            if (inverTimer > 3f)
            {
                invertControls = false;
                inverTimer = 0f;
            }
        }
    }
}