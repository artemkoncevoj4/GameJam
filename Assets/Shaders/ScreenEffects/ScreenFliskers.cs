using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class ScreenFliskers : MonoBehaviour
{
    [SerializeField] private Image flicker_black;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private int num_flickers = 8;

    public void Start_flickers()
    {
        StartCoroutine(Flickers());
    }

    private System.Collections.IEnumerator Flickers()
    {
        float interval = duration / num_flickers;
        for (int i = 0; i < num_flickers; i++)
        {
            SetAlpha(1f);
            yield return new WaitForSeconds(interval * 0.8f);
            SetAlpha(0f);
            yield return new WaitForSeconds(interval * 0.8f);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color c = flicker_black.color;
        c.a = alpha;
        flicker_black.color = c;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Start_flickers();
        }

    }
}
