using System;
using Heraldo.Flavor;
using Heraldo.Global.Enums;
using UnityEngine;

namespace Heraldo.Global.Structs
{
    [Serializable]
    public struct Objective
    {
        public ObjectiveID ID;
        public ObjectiveID nextObjectiveId;
        public string Title;
        public Vector3 Position;
        public bool Completed;
        public Interactable interactable;
    }
}