using UnityEngine;

//* Идрисов Д.С
/// <summary>
/// Единый интерфейс для управления фейд-эффектом экрана.
/// </summary>
public interface IScreenFader
{
    void FadeIn();
    void FadeOut();
    void FadeScreen();
    void ResetFade();
    bool IsFading();
}