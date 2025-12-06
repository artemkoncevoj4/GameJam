using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace InteractiveObjects
{
    abstract public class InteractObject : MonoBehaviour
    {
        [SerializeField]
        private int id = 1;
        public int ID => id;
        public string State {get; protected set;} = "Недоступен";


        public abstract void Interact();
        public virtual void OnInteractionStarted()
        {
            Debug.Log($"Interaction started with {gameObject.name}");
            State = "Interacting";
        }
        protected virtual void Start()
        {
            if (id == -1)
            {
            Debug.LogWarning($"{gameObject.name} does not have an ID set!", this);
            }
        }

    }
}

