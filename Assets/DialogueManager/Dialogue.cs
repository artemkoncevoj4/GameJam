
using UnityEngine;

// * Идрисов Д.С

namespace DialogueManager {
        /// <summary>
        /// Представляет диалог с персонажем, содержащий его имя и последовательность предложений.
        /// </summary>
    [System.Serializable]
    public class Dialogue
    {
        /// <summary>
        /// Имя говорящего персонажа (или просто Title).
        /// </summary>
        public string name;
        /// <summary>
        /// Последовательность предложений, которые говорит персонаж.
        /// </summary>
        [TextArea(3, 10)]
        public string[] sentences;
    }
}