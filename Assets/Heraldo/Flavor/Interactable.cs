using System;
using Heraldo.Character;
using Heraldo.Global.Enums;
using Heraldo.Level;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class Interactable : MonoBehaviour
    {
        public ObjectiveID ObjetivoRelacionado = ObjectiveID.NO_OBJECTIVE;
        private ObjectiveManager _objectiveManager;

        public virtual string GetDescription()
        {
            return "Nondescript";}

        public void Awake()
        {
            _objectiveManager = GameObject.FindWithTag("ObjectiveManager").GetComponent<ObjectiveManager>();
            
        }
        
        public void Start()
        {
            if(ObjetivoRelacionado != ObjectiveID.NO_OBJECTIVE)
            {
                _objectiveManager.ChangeObjective(ObjetivoRelacionado);
                _objectiveManager.currentObjective.Position = transform.position;
            }
        }

        public void Pickup()
        {
            _objectiveManager.CompleteObjective();
        }
        //Determine if the object will delete when picked up 
        public virtual void Interact(FPSController controller) {}
    }
}