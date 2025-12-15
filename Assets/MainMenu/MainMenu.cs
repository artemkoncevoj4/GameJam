using UnityEngine;
using UnityEngine.SceneManagement;

namespace MainMenu
{
    // Муштаков А.Ю.

    /// <summary>
    /// Управляет функциональностью главного меню игры.
    /// Обрабатывает запуск новой игры и выход из приложения.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        /// <summary>
        /// Запускает игру, загружая следующую сцену в порядке сборки.
        /// Использует порядок сцен в Build Settings для определения следующей сцены.
        /// </summary>
        public void PlayGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        /// <summary>
        /// Завершает работу приложения.
        /// В редакторе Unity выводит сообщение в консоль, в собранной версии закрывает приложение.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("<color=yellow>Игра закрылась!</color>"); 
          
            Application.Quit();
        }
    }
}