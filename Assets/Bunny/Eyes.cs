using UnityEngine;

public class Eyes : MonoBehaviour
{
    [SerializeField] private Transform _appearPoint_Window;
    private SpriteRenderer _spriteRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!Bunny.Bunny.Peeking)
            {
                transform.position = _appearPoint_Window.position;
                transform.rotation = _appearPoint_Window.rotation;
            }
    }

    // Update is called once per frame
    void Update()
    {

        if (Bunny.Bunny.Peeking)
        {
            SetVisible(true);
        }
        else
        {
            SetVisible(false);
        }
    }
    private void SetVisible(bool visible)
        {
            if (_spriteRenderer == null) return;
            _spriteRenderer.enabled = visible;
        
            // Можно включить/выключить все дочерние объекты
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(visible);
            }
        }
}
