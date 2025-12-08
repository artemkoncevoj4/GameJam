using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractiveObjects
{
    abstract public class InteractObject : MonoBehaviour
    {
        private static Dictionary<int, InteractObject> _objectDatabase = new Dictionary<int, InteractObject>();
        private static int _idCounter = 1;

        [Header("Object")]
        [SerializeField] private Transform _item_placement;

        public string Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int ID { get; private set; }

        private bool _is_visible = false;
        private bool _is_picked = false;

        public void InitializeObject(string objType, string name, string description)
        {
            ID = _idCounter++;
            Type = objType;
            Name = name;
            Description = description;

            if (!_objectDatabase.ContainsKey(ID))
            {
                _objectDatabase.Add(ID, this);
                Debug.Log($"Объект '{Name}' зарегистрирован с ID: {ID}");
            }
        }

        void Start()
        {
            if (ID == 0)
            {
                ID = _idCounter++;
                Type = "Default";
                Name = gameObject.name;
                Description = "Автоматически созданный объект";

                if (!_objectDatabase.ContainsKey(ID))
                {
                    _objectDatabase.Add(ID, this);
                }
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