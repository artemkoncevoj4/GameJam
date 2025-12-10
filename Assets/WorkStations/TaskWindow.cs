using UnityEngine;
using TaskSystem;

namespace InteractiveObjects
{
    public class SubmitStation : Workstation
    {
        [Header("Настройки сдачи")]
        [SerializeField] private AudioClip _submitSound;
        [SerializeField] private ParticleSystem _submitEffect;
        
        public string GetInteractionHint()
        {
            return "Сдать документ (E)";
        }

        public bool CanInteract()
        {
            return TaskManager.Instance != null && TaskManager.Instance.IsTaskActive;
        }
        
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
                PlayFailEffects();
                return;
            }
            
            // Сдаем документ
            TaskManager.Instance.SubmitDocument();
            
            // Эффекты
            PlaySubmitEffects();
            
            Debug.Log("Документ отправлен на проверку");
        }
        
        public override void ResetTable()
        {
            // Простая станция не имеет состояния
        }
        
        private void PlaySubmitEffects()
        {
            if (_submitSound != null)
                AudioSource.PlayClipAtPoint(_submitSound, transform.position);
            
            if (_submitEffect != null)
                _submitEffect.Play();
        }
        
        private void PlayFailEffects()
        {
            // Просто звук ошибки
            if (_submitSound != null)
            {
                AudioSource audioSource = GetComponent<AudioSource>();
                if (audioSource != null)
                    audioSource.PlayOneShot(_submitSound);
            }
        }
    }
}