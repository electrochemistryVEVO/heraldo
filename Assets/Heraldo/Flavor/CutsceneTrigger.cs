using System;
using Heraldo.Character;
using Heraldo.Cutscenes;
using UnityEngine;

namespace Heraldo.Flavor
{
    public class CutsceneTrigger : Interactable
    {
        public CutsceneManager manager;
        public override void Interact(FPSController controller)
        {
            controller.TriggerPlayerInput(false);
            manager.StartCutscene();
            CutsceneManager.CutsceneEnded endCutsceneAction = null;
            endCutsceneAction = () =>
            {
                manager.endCutscene -= endCutsceneAction;
                //Reactivar jugador
                controller.TriggerPlayerInput(true);
            };
            manager.endCutscene += endCutsceneAction;
            //Esperar a que termine la cutscene y habilitar de nuevo
        }
    }
}