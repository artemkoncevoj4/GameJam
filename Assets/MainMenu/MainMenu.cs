using UnityEngine;
using UnityEngine.SceneManagement;
namespace MainMenu{
  public class MainMenu : MonoBehaviour
  {
      public void PlayGame()
      {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
      }

      // Эта функция закрывает игру
      public void QuitGame()
      {
          Debug.Log("Игра закрылась!"); 
          Application.Quit();
      }
  }
}