using UnityEngine;

namespace Player 
{
    // Муштаков А.Ю.

    /// <summary>
    /// Управляет перемещением игрока по сцене, включая поддержку инверсии управления.
    /// Обрабатывает ввод с клавиатуры и применяет движение к Rigidbody2D компоненту.
    /// </summary>
    public class MovementPlayer : MonoBehaviour
    {
        /// <summary>
        /// Базовая скорость перемещения игрока.
        /// </summary>
        public float moveSpeed = 125f;
        
        private Rigidbody2D rb;
        private float inverTimer = 0f;
        
        /// <summary>
        /// Статический флаг, указывающий, активна ли инверсия управления.
        /// При значении true направления управления меняются на противоположные.
        /// </summary>
        public static bool invertControls = false; // Мне было лень норм логику делать, поэтому static (в общем игрок один и инверсия может быть только одна)
        
        /// <summary>
        /// Инициализирует компонент Rigidbody2D при запуске.
        /// </summary>
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        /// <summary>
        /// Обрабатывает ввод с клавиатуры и обновляет движение игрока каждый кадр.
        /// Управляет таймером инверсии контроля и предоставляет тестовое переключение по клавише I.
        /// </summary>
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

            // Таймер для автоматического отключения инверсии через 3 секунды
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