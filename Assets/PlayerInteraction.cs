using UnityEngine;
using System;
using System.Collections.Generic;
using InteractiveObjects;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject interactionText;
    public float interactionRadius = 5f;
    public KeyCode interactionKey = KeyCode.E;
    public Material highlightMaterial;

    [Header("Информация (для отладки)")]
    [SerializeField] private GameObject currentNearestObject;
    [SerializeField] private List<GameObject> objectsInRange = new List<GameObject>();

    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    private CircleCollider2D triggerCollider;
    public Action OnObjectInteraction;
    private Workstation activeWorkstation = null; // Текущая активная станция

    void Awake()
    {
        SetupTriggerCollider();
    }

    void SetupTriggerCollider()
    {
        var oldColliders = GetComponents<Collider>();
        foreach (var col in oldColliders)
        {
            if (col.isTrigger) Destroy(col);
        }

        triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = interactionRadius;
        
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
        }
    }

    void Start()
    {
        //interactionText = GameObject.FindGameObjectWithTag("InteractionText");
        interactionText.SetActive(false);
    }

    void Update()
    {
        UpdateNearestObject();

        if (Input.GetKeyDown(interactionKey))
        {
            // Если есть активная станция, проверяем не нажали ли мы на неё
            if (activeWorkstation != null && currentNearestObject != null)
            {
                Workstation workstation = currentNearestObject.GetComponent<Workstation>();
                if (workstation == activeWorkstation)
                {
                    // Нажали на активную станцию - обрабатываем взаимодействие
                    InteractWithCurrentObject();
                }
                else
                {
                    // Нажали на другую станцию - закрываем текущую и открываем новую
                    if (activeWorkstation.IsActive())
                    {
                        activeWorkstation.ResetTable();
                        activeWorkstation = null;
                    }
                    InteractWithCurrentObject();
                }
            }
            else if (currentNearestObject != null)
            {
                // Нет активной станции, открываем новую
                InteractWithCurrentObject();
            }
        }
    }

    void UpdateNearestObject()
    {
        GameObject previousNearest = currentNearestObject;
        GameObject nearest = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (var obj in objectsInRange)
        {
            if (obj == null) continue;

            Vector2 offset = obj.transform.position - transform.position;
            float sqrDistance = offset.sqrMagnitude;

            if (sqrDistance < closestDistanceSqr)
            {
                closestDistanceSqr = sqrDistance;
                nearest = obj;
            }
        }

        if (nearest != previousNearest)
        {
            // Убираем подсветку со старого объекта
            if (previousNearest != null)
            {
                RestoreMaterials(previousNearest);
                
                // Если отходим от активной станции - закрываем её
                if (activeWorkstation != null && previousNearest == activeWorkstation.gameObject)
                {
                    float distance = Vector2.Distance(transform.position, previousNearest.transform.position);
                    if (distance > interactionRadius * 1.5f) // Немного больше радиуса для надежности
                    {
                        activeWorkstation.ResetTable();
                        activeWorkstation = null;
                        Debug.Log("Отходим от активной станции, закрываем окно");
                    }
                }
            }

            // Подсвечиваем новый объект
            currentNearestObject = nearest;
            if (currentNearestObject != null)
            {
                HighlightObject(currentNearestObject);
                Debug.Log($"Ближайший объект: {currentNearestObject.name}");
            }
        }
    }

    void HighlightObject(GameObject obj)
    {
        if (highlightMaterial == null) return;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            if (!originalMaterials.ContainsKey(obj))
            {
                originalMaterials[obj] = renderer.materials;
            }

            Material[] highlightMats = new Material[renderer.materials.Length];
            for (int i = 0; i < highlightMats.Length; i++)
            {
                highlightMats[i] = highlightMaterial;
            }
            renderer.materials = highlightMats;
        }
    }

    void RestoreMaterials(GameObject obj)
    {
        if (originalMaterials.ContainsKey(obj))
        {
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.materials = originalMaterials[obj];
            }
            originalMaterials.Remove(obj);
        }
    }

    void InteractWithCurrentObject()
    {
        Debug.Log($"Взаимодействуем с: {currentNearestObject.name}");

        Workstation workstation = currentNearestObject.GetComponent<Workstation>();
        
        if (workstation != null)
        {
            workstation.UseStation(); 
            
            // Если станция стала активной, запоминаем её
            if (workstation.IsActive())
            {
                activeWorkstation = workstation;
            }
            else
            {
                activeWorkstation = null;
            }
            
            OnObjectInteraction?.Invoke();
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Interactive") && !objectsInRange.Contains(other.gameObject))
        {
            objectsInRange.Add(other.gameObject);
            interactionText.SetActive(true);
            Debug.Log($"Объект вошел в зону: {other.name}");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Interactive"))
        {
            interactionText.SetActive(false);
            GameObject exitedObject = other.gameObject;
            objectsInRange.Remove(exitedObject);

            // Если выходим из зоны активной станции - закрываем её
            if (activeWorkstation != null && exitedObject == activeWorkstation.gameObject)
            {
                activeWorkstation.ResetTable();
                activeWorkstation = null;
                Debug.Log("Вышли из зоны активной станции, закрыли окно");
            }

            if (exitedObject == currentNearestObject)
            {
                RestoreMaterials(exitedObject);
                currentNearestObject = null;
            }

            originalMaterials.Remove(exitedObject);
            Debug.Log($"Объект вышел из зоны: {other.name}");
        }
    }
}