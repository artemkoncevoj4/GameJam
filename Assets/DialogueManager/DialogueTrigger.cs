using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueManager {
    public class DialogueTrigger : MonoBehaviour
    {
        public Dialogue dialogue;
        
        // Переменная hasTriggered теперь не нужна, т.к. диалог не будет 
        // запускаться автоматически, и нам не нужно проверять, был ли он запущен.
        
        // МЕТОД START() УДАЛЕН, чтобы диалог не запускался при загрузке сцены.
        
        public void TriggerDialogue()
        {
            Debug.Log("[DialogueTrigger] Looking for DialogueManager...");
            DialogueManager manager = FindObjectOfType<DialogueManager>();
            
            if (manager != null)
            {
                Debug.Log("[DialogueTrigger] DialogueManager found: " + manager.gameObject.name);
                
                if (!manager.IsDialogueActive())
                {
                    Debug.Log("[DialogueTrigger] Starting dialogue...");
                    manager.StartDialogue(dialogue);
                }
                else
                {
                    Debug.LogWarning("[DialogueTrigger] Dialogue is already active, cannot start new one");
                }
            }
            else
            {
                Debug.LogError("[DialogueTrigger] DialogueManager not found in scene!");
            }
        }
    }
}