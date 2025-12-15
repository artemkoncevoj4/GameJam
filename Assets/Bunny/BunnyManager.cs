using UnityEngine;


namespace Bunny {
    // Муштаков А.Ю.

    /// <summary>
    /// Управляет жизненным циклом и поведением основного объекта кролика (<see cref="Bunny"/>) 
    /// в ответ на события игрового цикла (<c>GameCycle</c>).
    /// </summary>
    public class BunnyManager : MonoBehaviour
    {
        /// <summary>
        /// Ссылка на основной объект кролика в сцене. Назначается через инспектор.
        /// </summary>
        [SerializeField] private Bunny _bunny;
        
        /// <summary>
        /// Вызывается при запуске сцены. Подписывается на события появления и исчезновения кролика
        /// от глобального <c>GameCycle</c>.
        /// </summary>
        void Start()
        {
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnRabbitAppearing += OnRabbitAppear;
                GameCycle.Instance.OnRabbitLeaving += OnRabbitLeave;
            }
        }
        
        /// <summary>
        /// Вызывается при уничтожении объекта. Отписывается от событий <c>GameCycle</c>
        /// для предотвращения утечек памяти.
        /// </summary>
        void OnDestroy()
        {
            if (GameCycle.Instance != null)
            {
                GameCycle.Instance.OnRabbitAppearing -= OnRabbitAppear;
                GameCycle.Instance.OnRabbitLeaving -= OnRabbitLeave;
            }
        }
        
        /// <summary>
        /// Обработчик события появления кролика. Активирует поведение кролика, вызывая его метод <c>Appear</c>.
        /// </summary>
        private void OnRabbitAppear()
        {
            if (_bunny != null && !_bunny.IsActive)
            {
                _bunny.Appear();
            }
        }
        
        /// <summary>
        /// Обработчик события исчезновения кролика. Деактивирует поведение кролика, вызывая его метод <c>Leave</c>.
        /// </summary>
        private void OnRabbitLeave()
        {
            if (_bunny != null && _bunny.IsActive)
            {
                _bunny.Leave();
            }
        }
        
    }
}