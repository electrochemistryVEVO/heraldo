using System;
using Heraldo.Character;
using Heraldo.Cutscenes;
using Heraldo.Global.Enums;
using Heraldo.Level;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class Interactable : MonoBehaviour
    {
        public ObjectiveID ObjetivoRelacionado = ObjectiveID.NO_OBJECTIVE;
        public int WeaponPrerequisite = -1;
        public ObjectiveID ObjectivePrerequisite = ObjectiveID.NO_OBJECTIVE;
        protected ObjectiveManager _objectiveManager;
        protected CutsceneManager _cutsceneManager;

        public virtual void OnHover() { }

        public virtual void OnLeave() { }

        public virtual string GetDescription()
        {
            return "Nondescript";}

        public void Awake()
        {
            _objectiveManager = GameObject.FindWithTag("ObjectiveManager").GetComponent<ObjectiveManager>();
            _cutsceneManager = GameObject.FindWithTag("CutsceneManager").GetComponent<CutsceneManager>();
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
            if(ObjectivePrerequisite == ObjectiveID.NO_OBJECTIVE 
               || _objectiveManager.currentObjective.ID == ObjectivePrerequisite)
                _objectiveManager.CompleteObjective();
        }
        //Determine if the object will delete when picked up 
        public virtual void Interact(FPSController controller) {}
    }
}