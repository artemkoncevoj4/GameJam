using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractiveObjects
{
    abstract public class InteractObject : MonoBehaviour
    {
        private static Dictionary<int, InteractObject> _objectDatabase = new Dictionary<int, InteractObject>();
        private static int _idCounter = 1000;

        [Header("Object Data")]
        [SerializeField] private int _itemDataId = -1;

        [Header("Object Placement")]
        [SerializeField] private Transform _item_placement;
        [SerializeField] private GameObject _visualObject;

        private ItemData _jsonData;

        public string Type => _jsonData?.type ?? "Default";
        public string Name => _jsonData?.name ?? gameObject.name;
        public string Description => _jsonData?.description ?? "No description";
        public string Category => _jsonData?.category ?? "Unknown";
        public int ID { get; private set; }

        private bool _is_visible = false;
        private bool _is_picked = false;

        void Start()
        {
            ID = _idCounter++;
            
            if (_itemDataId >= 0 && ItemDatabase.Instance != null)
            {
                _jsonData = ItemDatabase.Instance.GetItem(_itemDataId);
            } 

            if (!_objectDatabase.ContainsKey(ID))
            {
                _objectDatabase.Add(ID, this);
                Debug.Log($"Объект '{Name}' зарегистрирован с ID: {ID}");
            }

            if (_jsonData == null)
            {
                Debug.LogWarning($"Для объекта {gameObject.name} не найдены данные в JSON (ID: {_itemDataId})");
            }
        }

        

        public static bool DestroyObjectByID(int id)
        {
            if (_objectDatabase.TryGetValue(id, out InteractObject obj))
            {
                _objectDatabase.Remove(id);
                Destroy(obj.gameObject);
                Debug.Log($"Объект '{obj.Name}' (ID: {id}) удален.");
                return true;
            }
            Debug.LogWarning($"Не удалось найти объект с ID {id} для удаления.");
            return false;
        }

        public void DestroySelf()
        {
            DestroyObjectByID(this.ID);
        }

        // ..
        public virtual void Interact()
        {
            Debug.Log($"Взаимодействие с объектом: {Name} (ID: {ID})");
        }

        void OnDestroy()
        {
            if (_objectDatabase.ContainsKey(ID))
            {
                _objectDatabase.Remove(ID);
                Debug.Log($"Объект {Name} удален из базы данных");
            }
        }

        public static InteractObject GetObjectByID(int id)
        {
            _objectDatabase.TryGetValue(id, out InteractObject obj);
            return obj;
        }

        public static List<InteractObject> GetAllObjects()
        {
            return new List<InteractObject>(_objectDatabase.Values);
        }
    }
}