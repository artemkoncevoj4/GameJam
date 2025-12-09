using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
namespace Shaders.ScreenEffects {
    public class Fire_text : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private string glitchText = "ERROR";
        [SerializeField] private float duration = 0.2f;

        public void Fire()
        {
            StartCoroutine(GlitchRoutine());
        }

        private System.Collections.IEnumerator GlitchRoutine()
        {
            string original = textComponent.text;
            float end = Time.time + duration;
            while (Time.time < end)
            {
                textComponent.text = glitchText;
                yield return new WaitForSeconds(0.1f);
                textComponent.text = original;
                yield return new WaitForSeconds(0.1f);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Fire();
            }
        }
    }
}