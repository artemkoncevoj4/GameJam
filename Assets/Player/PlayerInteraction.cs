using UnityEngine;
using System;
using System.Collections.Generic;
using InteractiveObjects;

namespace Player {
    // Муштаков А.Ю.

    //! Фактически весь код был сгенерирован ИИ с небольшими изменениями

    /// <summary>
    /// Управляет взаимодействием игрока с интерактивными объектами в окружении.
    /// Обеспечивает обнаружение объектов в радиусе, их подсветку и обработку взаимодействия.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Настройки")]
        /// <summary>
        /// Текстовый элемент UI, отображающий подсказку для взаимодействия.
        /// </summary>
        public GameObject interactionText;
        
        /// <summary>
        /// Радиус обнаружения интерактивных объектов вокруг игрока.
        /// </summary>
        public float interactionRadius = 5f;
        
        /// <summary>
        /// Клавиша для взаимодействия с объектами.
        /// </summary>
        public KeyCode interactionKey = KeyCode.E;
        
        /// <summary>
        /// Материал для подсветки выбранного объекта.
        /// </summary>
        public Material highlightMaterial;

        [Header("Информация (для отладки)")]
        [SerializeField] private GameObject currentNearestObject;
        [SerializeField] private List<GameObject> objectsInRange = new List<GameObject>();

        private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
        private CircleCollider2D triggerCollider;
        
        /// <summary>
        /// Событие, возникающее при взаимодействии с объектом.
        /// </summary>
        public Action OnObjectInteraction;
        
        private Workstation activeWorkstation = null; // Текущая активная станция

        /// <summary>
        /// Инициализирует коллайдер для обнаружения объектов при создании компонента.
        /// </summary>
        void Awake()
        {
            SetupTriggerCollider();
        }

        /// <summary>
        /// Настраивает триггерный коллайдер для обнаружения интерактивных объектов.
        /// Удаляет старые триггерные коллайдеры и добавляет новый CircleCollider2D.
        /// </summary>
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

        /// <summary>
        /// Инициализирует начальное состояние компонента, скрывая текстовую подсказку.
        /// </summary>
        void Start()
        {
            //interactionText = GameObject.FindGameObjectWithTag("InteractionText");
            interactionText.SetActive(false);
        }

        /// <summary>
        /// Обрабатывает обновление состояния каждый кадр: обновляет ближайший объект и обрабатывает ввод для взаимодействия.
        /// </summary>
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

        /// <summary>
        /// Определяет ближайший интерактивный объект среди объектов в радиусе взаимодействия.
        /// Управляет подсветкой объектов и сбросом активной станции при отдалении.
        /// </summary>
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

        /// <summary>
        /// Подсвечивает указанный объект с помощью highlightMaterial.
        /// Сохраняет оригинальные материалы объекта для последующего восстановления.
        /// </summary>
        /// <param name="obj">Объект для подсветки.</param>
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

        /// <summary>
        /// Восстанавливает оригинальные материалы объекта.
        /// </summary>
        /// <param name="obj">Объект, материалы которого нужно восстановить.</param>
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

        /// <summary>
        /// Выполняет взаимодействие с текущим ближайшим объектом.
        /// Если объект является рабочей станцией, активирует её и управляет состоянием активной станции.
        /// </summary>
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

        /// <summary>
        /// Вызывается при входе другого коллайдера в триггерную зону.
        /// Добавляет интерактивный объект в список объектов в радиусе и отображает подсказку.
        /// </summary>
        /// <param name="other">Коллайдер вошедшего объекта.</param>
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Interactive") && !objectsInRange.Contains(other.gameObject))
            {
                objectsInRange.Add(other.gameObject);
                interactionText.SetActive(true);
                Debug.Log($"Объект вошел в зону: {other.name}");
            }
        }

        /// <summary>
        /// Вызывается при выходе другого коллайдера из триггерной зоны.
        /// Удаляет объект из списка, скрывает подсказку и сбрасывает активную станцию при необходимости.
        /// </summary>
        /// <param name="other">Коллайдер вышедшего объекта.</param>
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
}