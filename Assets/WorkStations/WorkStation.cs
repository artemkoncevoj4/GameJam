using UnityEngine;
using System.Collections.Generic;
using Player;
using TaskSystem;
using System;
namespace InteractiveObjects
{
    public class Workstation : InteractObject
    {
        protected bool isActive = false;

        public virtual void UseStation()
        {
            Debug.Log("Interaction with workstation started");
            isActive = true;
        }
        
        public virtual void ResetTable()
        {
            Debug.Log("Interaction with workstation ended");
            isActive = false;
        }
        
        public bool IsActive()
        {
            return isActive;
 
        }
    }
}