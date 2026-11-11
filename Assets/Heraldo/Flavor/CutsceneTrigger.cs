using System;
using Heraldo.Character;
using Heraldo.Cutscenes;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class CutsceneTrigger : Interactable
    {
        public Cutscene cutscene;
        public override string GetDescription()
        {
            return "Ayudar a Bolognesi a pararse.";
        }
        public override void Interact(FPSController controller)
        {
            controller.TriggerPlayerInput(false);
            _cutsceneManager.PrepareCutscene(cutscene).ContinueWith(t1 => { controller.TriggerPlayerInput(true); });
            // CutsceneManager.CutsceneEnded endCutsceneAction = null;
            // endCutsceneAction = () =>
            // {
            //     _cutsceneManager.endCutscene -= endCutsceneAction;
            //     //Reactivar jugador
            //     controller.TriggerPlayerInput(true);
            // };
            // _cutsceneManager.endCutscene += endCutsceneAction;
            //Esperar a que termine la cutscene y habilitar de nuevo
        }
    }
}