using UnityEngine;
using System.Collections.Generic;
using Player;
using TaskSystem;
using System;
namespace InteractiveObjects
{
    public class Workstation : InteractObject
    {

        public virtual void UseStation()
        {
            Debug.Log("Interaction with workstation started");

        }
    }
}