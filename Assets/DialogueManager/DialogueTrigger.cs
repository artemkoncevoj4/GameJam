using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueManager {
    public class DialogueTrigger : MonoBehaviour
    {
        public Dialogue dialogue;
        private bool hasTriggered = false;
        
        void Start()
        {
            Debug.Log("[DialogueTrigger] Start() called on " + gameObject.name);
            
            if (!hasTriggered)
            {
                Debug.Log("[DialogueTrigger] Triggering dialogue...");
                TriggerDialogue();
                hasTriggered = true;
            }
            else
            {
                Debug.Log("[DialogueTrigger] Dialogue already triggered, skipping");
            }
        }
        
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