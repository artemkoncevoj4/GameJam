using UnityEngine;

namespace InteractiveObjects
{
    // Муштаков А.Ю.

    /// <summary>
    /// Базовый класс для всех рабочих станций в игре.
    /// Предоставляет основную функциональность для взаимодействия с интерактивными объектами.
    /// </summary>
    public class Workstation : InteractObject
    {
        /// <summary>
        /// Флаг, указывающий, активна ли рабочая станция в данный момент.
        /// </summary>
        protected bool isActive = false;

        /// <summary>
        /// Активирует рабочую станцию и начинает взаимодействие с ней.
        /// Должен быть переопределен в производных классах для реализации специфической функциональности.
        /// </summary>
        public virtual void UseStation()
        {
            Debug.Log("Interaction with workstation started");
            isActive = true;
        }
        
        /// <summary>
        /// Деактивирует рабочую станцию и завершает взаимодействие с ней.
        /// Должен быть переопределен в производных классах для корректного завершения работы станции.
        /// </summary>
        public virtual void ResetTable()
        {
            Debug.Log("Interaction with workstation ended");
            isActive = false;
        }
        
        /// <summary>
        /// Проверяет, активна ли рабочая станция в данный момент.
        /// </summary>
        /// <returns>Возвращает true, если станция активна, иначе false.</returns>
        public bool IsActive()
        {
            return isActive;
        }
    }
}