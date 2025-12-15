using UnityEngine;
using UnityEngine.UI;

// ! Сгенерировано ИИ

namespace Shaders {
    /// <summary>
    /// **Компонент для отображения градиентного фона на UI-элементах.**
    /// <para>Прикрепляется к объекту с компонентом <see cref="Image"/>. Автоматически создает и применяет 
    /// специальный материал с шейдером градиента, используя заданные цвета и высоту перехода.</para>
    /// <para>Требует наличия пользовательского шейдера "UI/Gradient" в проекте.</para>
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class GradientBackground : MonoBehaviour
    {
        /// <summary>
        /// Цвет верхней части градиента.
        /// </summary>
        [SerializeField] private Color _topColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
        /// <summary>
        /// Цвет нижней части градиента.
        /// </summary>
        [SerializeField] private Color _bottomColor = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        /// <summary>
        /// Относительная высота (от 0 до 1), на которой происходит переход между <see cref="_topColor"/> 
        /// и <see cref="_bottomColor"/>.
        /// </summary>
        [SerializeField] private float _gradientHeight = 0.5f;
        
        // --- Внутренние ссылки ---
        private Image _image;
        private Material _gradientMaterial;
        
        /// <summary>
        /// Инициализация: Получает компонент <see cref="Image"/> и вызывает создание градиентного материала.
        /// </summary>
        void Start()
        {
            _image = GetComponent<Image>();
            CreateGradientMaterial();
        }
        
        /// <summary>
        /// Создает новый Material, назначает ему шейдер "UI/Gradient" и передает параметры цвета и высоты.
        /// Назначает созданный материал компоненту <see cref="Image"/>.
        /// </summary>
        private void CreateGradientMaterial()
        {
            // Ищем шейдер по имени. Это необходимо, чтобы скрипт мог работать с кастомным шейдером.
            Shader gradientShader = Shader.Find("UI/Gradient");
            
            if (gradientShader == null)
            {
                Debug.LogError("<color=red>Shader 'UI/Gradient' not found. Make sure the shader file is in the project.</color>");
                return;
            }
            
            // Создаем материал из найденного шейдера
            _gradientMaterial = new Material(gradientShader);
            
            // Передаем параметры в шейдер
            _gradientMaterial.SetColor("_TopColor", _topColor);
            _gradientMaterial.SetColor("_BottomColor", _bottomColor);
            _gradientMaterial.SetFloat("_GradientHeight", _gradientHeight);
            
            // Применяем материал к Image
            _image.material = _gradientMaterial;
            _image.color = Color.white; // Убеждаемся, что цвет Image не мешает материалу
        }
        
        /// <summary>
        /// Вызывается в редакторе (Inspector) при изменении полей.
        /// Обеспечивает немедленное обновление параметров градиента в материале без перезапуска сцены.
        /// </summary>
        void OnValidate()
        {
            // Если материал уже создан, обновляем его свойства в соответствии с новыми значениями
            if (_gradientMaterial != null)
            {
                _gradientMaterial.SetColor("_TopColor", _topColor);
                _gradientMaterial.SetColor("_BottomColor", _bottomColor);
                _gradientMaterial.SetFloat("_GradientHeight", _gradientHeight);
            }
        }
    }
}