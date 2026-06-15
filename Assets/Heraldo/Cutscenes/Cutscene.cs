using System.Collections.Generic;
using UnityEngine;

namespace Heraldo.Cutscenes
{

    public enum CutsceneActionType
    {
        ENEMY_SPAWN,
        ALLY_SPAWN,
        CHARACTER_MOVE
    }
    [System.Serializable]
    public class CutsceneAction
    {
        public int order;
        public CutsceneActionType type;
        public string actor;
        public GameObject spawn;
        public Vector3 where;
    }
    
    [System.Serializable]
    public class CutscenePart //: UnityEngine.Object
    {
        public string author;
        public string subjectName;
        public Vector3 subjectPos;
        public string text;
        public string soundbyteId;
        public float time;

        public CutscenePart()
        {
            this.author = "";
            this.subjectName = "";
            this.subjectPos = Vector3.zero;
            this.text = "";
            this.soundbyteId = "";
            this.time = 1.0f;
        }
    }
    
    [CreateAssetMenu(fileName = "Cutscene", menuName = "Cutscenes/Cutscene", order = 0)]
    public class Cutscene : ScriptableObject
    {
        [SerializeField]
        public List<CutscenePart> parts = new List<CutscenePart>(new CutscenePart[]{new CutscenePart()});

        [SerializeField] public List<CutsceneAction> actions = new List<CutsceneAction>(new CutsceneAction[]{new CutsceneAction()});
    }
}