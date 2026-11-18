using Heraldo.Character;
using Heraldo.Cutscenes;

namespace Heraldo.Flavor
{
    public class CutsceneTrigger : Interactable
    {
        public string description = "Ayudar a Bolognesi a pararse.";
        public Cutscene cutscene;
        public override string GetDescription()
        {
            return description;
        }
        public override async void Interact(FPSController controller)
        {
            if (_cutsceneManager.CutsceneMode) return;

            await _cutsceneManager.PrepareCutscene(cutscene);
        }
    }
}
