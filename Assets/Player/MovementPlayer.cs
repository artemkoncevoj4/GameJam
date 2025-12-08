using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Player {
    public class MovementPlayer : MonoBehaviour
    {
        public float moveSpeed = 125f;
        private Rigidbody2D rb;
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        void Update()
        {
            Vector2 rawInput = new Vector2(
                Input.GetAxisRaw("Horizontal"), 
                Input.GetAxisRaw("Vertical")
            ).normalized;
            rb.linearVelocity = rawInput * moveSpeed;
        }
    }
}