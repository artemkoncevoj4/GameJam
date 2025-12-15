
using UnityEngine;

// * Идрисов Д.С

namespace DialogueManager {
    /// <summary>
    /// Компонент, отвечающий за запуск определенного диалога (<see cref="Dialogue"/>) 
    /// через единственный экземпляр <see cref="DialogueManager"/> в сцене.
    /// </summary>
    public class DialogueTrigger : MonoBehaviour
    {
        /// <summary>
        /// Объект диалога, который будет запущен при вызове <see cref="TriggerDialogue"/>.
        /// </summary>
        public Dialogue dialogue;
        
        /// <summary>
        /// Ищет активный <see cref="DialogueManager"/> в сцене и, если ни один диалог не активен, 
        /// начинает заданный диалог.
        /// </summary>
        public void TriggerDialogue()
        {
            Debug.Log("[DialogueTrigger] Looking for DialogueManager...");
            
            // Находим DialogueManager в сцене
            DialogueManager manager = FindAnyObjectByType<DialogueManager>();
            
            if (manager != null)
            {
                Debug.Log("<color=green>[DialogueTrigger] DialogueManager found: </color>" + manager.gameObject.name);
                
                // Проверяем, активен ли уже диалог
                if (!manager.IsDialogueActive())
                {
                    Debug.Log("<color=cyan>[DialogueTrigger] Starting dialogue...</color>");
                    manager.StartDialogue(dialogue);
                }
                else
                {
                    Debug.LogWarning("<color=red>[DialogueTrigger] Dialogue is already active, cannot start new one</color>");
                }
            }
            else
            {
                Debug.LogError("<color=red>[DialogueTrigger] DialogueManager not found in scene!</color>");
            }
        }
    }
}