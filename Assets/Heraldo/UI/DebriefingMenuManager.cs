using Heraldo.Global.SO;
using Heraldo.Global.Structs;
using TMPro;
using UnityEngine;
using UnityFigmaBridge.Runtime.UI;

namespace Heraldo.UI
{
    public class DebriefingMenuManager : RetryMenuManager
    {
        public DebriefingList debriefings;
        public TMP_Text indexText;
        public TMP_Text descriptionText;
        public TMP_Text tipsText;
        public TMP_Text titleText;
        public FigmaImage background;
        
        public override void Start()
        {
            base.Start();
            Debriefing debriefToLoad = debriefings.debriefings.Find((e) => e.sceneName == _scnToLoad);
            indexText.text = $"CAPÍTULO {debriefToLoad.index}";
            descriptionText.text = debriefToLoad.description;
            tipsText.text = debriefToLoad.tips;
            titleText.text = debriefToLoad.title;
            background.sprite = debriefToLoad.background;
        }
    }
}