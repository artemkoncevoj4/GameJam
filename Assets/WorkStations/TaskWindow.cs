using UnityEngine;
using TaskSystem;

namespace InteractiveObjects
{
    // Муштаков А.Ю.

    /// <summary>
    /// Станция для сдачи документов, производная от базового класса Workstation.
    /// Отвечает за обработку сдачи документов на проверку и взаимодействие с TaskManager.
    /// </summary>
    public class SubmitStation : Workstation
    {
        [Header("Настройки сдачи")]
        [SerializeField] private AudioClip _submitSound; 
        [SerializeField] private ParticleSystem _submitEffect; 
        
        /// <summary>
        /// Возвращает текстовую подсказку для взаимодействия со станцией.
        /// </summary>
        /// <returns>Строка с текстом подсказки.</returns>
        public string GetInteractionHint() //TODO Должна быть как диалоговое окно снизу при приближении к станции (не реализовано)
        {
            return "Сдать документ (E)";
        }
        
        /// <summary>
        /// Выполняет взаимодействие со станцией: отправляет текущий документ на проверку через TaskManager.
        /// </summary>
        public override void UseStation()
        {
            if (TaskManager.Instance == null)
            {
                Debug.LogError("TaskManager не найден!");
                return;
            }
            
            if (!TaskManager.Instance.IsTaskActive)
            {
                Debug.Log("Нет активного задания");
                return;
            }
            
            // Сдаем документ
            TaskManager.Instance.SubmitDocument();
            
            Debug.Log("Документ отправлен на проверку");
        }
        
        /// <summary>
        /// Сбрасывает состояние станции. 
        /// В данной реализации не требует дополнительных действий, так как станция не имеет сложного состояния.
        /// </summary>
        public override void ResetTable()
        {
            // Простая станция не имеет состояния
        }
    }
}