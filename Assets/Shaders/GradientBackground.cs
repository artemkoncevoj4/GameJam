using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GradientBackground : MonoBehaviour
{
    [SerializeField] private Color _topColor = new Color(0.1f, 0.1f, 0.2f, 0.9f);
    [SerializeField] private Color _bottomColor = new Color(0.05f, 0.05f, 0.1f, 0.95f);
    [SerializeField] private float _gradientHeight = 0.5f;
    
    private Image _image;
    private Material _gradientMaterial;
    
    void Start()
    {
        _image = GetComponent<Image>();
        CreateGradientMaterial();
    }
    
    private void CreateGradientMaterial()
{
    // Ищем шейдер по имени, указанному в файле
    Shader gradientShader = Shader.Find("UI/Gradient");
    
    if (gradientShader == null)
    {
        Debug.LogError("Shader 'UI/Gradient' not found. Make sure the shader file is in the project.");
        return;
    }
    // Создаем материал из найденного шейдера
    _gradientMaterial = new Material(gradientShader);
    _gradientMaterial.SetColor("_TopColor", _topColor);
    _gradientMaterial.SetColor("_BottomColor", _bottomColor);
    _gradientMaterial.SetFloat("_GradientHeight", _gradientHeight);
    
    _image.material = _gradientMaterial;
    _image.color = Color.white;
}
    
    void OnValidate()
    {
        if (_gradientMaterial != null)
        {
            _gradientMaterial.SetColor("_TopColor", _topColor);
            _gradientMaterial.SetColor("_BottomColor", _bottomColor);
            _gradientMaterial.SetFloat("_GradientHeight", _gradientHeight);
        }
    }
}