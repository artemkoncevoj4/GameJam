using UnityEngine;

public interface IScreenFader
{
    void FadeIn();
    void FadeOut();
    void FadeScreen();
    void ResetFade();
    bool IsFading();
}