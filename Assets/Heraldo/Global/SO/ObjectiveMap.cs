using System.Collections.Generic;
using Heraldo.Global.Enums;
using Heraldo.Global.Structs;
using UnityEngine;

namespace Heraldo.Global.SO
{
    [CreateAssetMenu(fileName = "ObjectiveMap", menuName = "Objective List", order = 0)]
    public class ObjectiveMap : ScriptableObject
    {
        [SerializeField] public List<Objective> Objectives;

        public Objective FindObjective(ObjectiveID id)
        {
            return Objectives.Find(val => val.ID == id);
        }
    }
}