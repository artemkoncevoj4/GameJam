using UnityEngine;
using System;
using System.Collections.Generic;
using InteractiveObjects;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки")]
    public float interactionRadius = 5f; // Радиус зоны взаимодействия
    public KeyCode interactionKey = KeyCode.E; // Клавиша для активации
    public Material highlightMaterial; // Материал для подсветки (опционально)

    [Header("Информация (для отладки)")]
    [SerializeField] private GameObject currentNearestObject; // Текущий ближайший объект
    [SerializeField] private List<GameObject> objectsInRange = new List<GameObject>(); // Список объектов в зоне

    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>(); // Кэш оригинальных материалов

    private CircleCollider2D triggerCollider;
    public Action OnObjectInteraction;

    void Start()
    {
        // Настраиваем триггер-коллайдер автоматически
        CircleCollider2D triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.radius = interactionRadius;
    }

    void Update()
    {
        // 1. Постоянно обновляем ближайший объект
        UpdateNearestObject();

        // 2. Проверяем нажатие клавиши взаимодействия
        if (Input.GetKeyDown(interactionKey) && currentNearestObject != null)
        {
            Debug.Log($"<color=red>Nearest object: {currentNearestObject}</color=red>");
            InteractWithCurrentObject();
        }
    }

    // Основной метод поиска ближайшего объекта из списка
    void UpdateNearestObject()
    {
        GameObject previousNearest = currentNearestObject;
        GameObject nearest = null;
        float closestDistanceSqr = Mathf.Infinity;

        // Перебираем все объекты в зоне, находим ближайший
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

        // Если ближайший объект изменился
        if (nearest != previousNearest)
        {
            // Убираем подсветку со старого объекта
            if (previousNearest != null)
            {
                RestoreMaterials(previousNearest);
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

    // Подсветка объекта (меняем материалы)
    void HighlightObject(GameObject obj)
    {
        if (highlightMaterial == null) return;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Сохраняем оригинальные материалы
            if (!originalMaterials.ContainsKey(obj))
            {
                originalMaterials[obj] = renderer.materials;
            }

            // Применяем материал подсветки ко всем материалам объекта
            Material[] highlightMats = new Material[renderer.materials.Length];
            for (int i = 0; i < highlightMats.Length; i++)
            {
                highlightMats[i] = highlightMaterial;
            }
            renderer.materials = highlightMats;
        }
    }

    // Восстановление оригинальных материалов
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

    // Взаимодействие с текущим ближайшим объектом
    void InteractWithCurrentObject()
    {
        Debug.Log($"Взаимодействуем с: {currentNearestObject.name}");

        Workstation workstation = currentNearestObject.GetComponent<Workstation>();
        
        if (workstation != null)
        {
            workstation.UseStation(); 
        }
    }
}
