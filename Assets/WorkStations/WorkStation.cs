using UnityEngine;
using System.Collections.Generic;
using Player;
using TaskSystem;
using System;
namespace InteractiveObjects
{
    public abstract class Workstation : InteractObject
    {
        

        public void UseStation()
        {
            Debug.Log("Interaction with workstation started");

        }
    }
}