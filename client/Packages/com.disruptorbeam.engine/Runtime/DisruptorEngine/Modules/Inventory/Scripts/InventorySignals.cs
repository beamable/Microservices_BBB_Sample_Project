﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Platform.SDK;
using Core.Platform.SDK.Inventory;
using DisruptorBeam.Modules.AccountManagement; // TODO, we can share one toggle event
using DisruptorBeam.Signals;
using UnityEngine;
using UnityEngine.UI;

namespace DisruptorBeam.Modules.Inventory.Scripts
{

    [System.Serializable]
    public class InventoryUpdateArg
    {
        public List<ItemView> Inventory;
        public InventoryGroup Group;
    }

    public class InventorySignals : DeSignalTower
    {
        public ToggleEvent OnToggleInventory;

        private static bool _toggleState;


        public void ToggleInventory()
        {
            ToggleInventory(!_toggleState);
        }

        public void ToggleInventory(bool desiredState)
        {
            if (_toggleState == desiredState) return;
            _toggleState = desiredState;
            Broadcast(_toggleState, s => s.OnToggleInventory);
        }

        private void Broadcast<TArg>(TArg arg, Func<InventorySignals, DeSignal<TArg>> getter)
        {
            this.BroadcastSignal(arg, getter);
        }

    }
}