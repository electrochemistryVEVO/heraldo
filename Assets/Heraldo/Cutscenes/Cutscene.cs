using System.Collections.Generic;
using Heraldo.Global.Enums;
using UnityEngine;

namespace Heraldo.Cutscenes
{

    public enum CutsceneActionType
    {
        ENEMY_SPAWN,
        ALLY_SPAWN,
        CHARACTER_MOVE,
        CHARACTER_FOLLOW_ROUTINE,
        CHARACTER_CALCULATE_PATH,
        CHARACTER_ROTATE_TOWARDS,
        CHARACTER_SPECIAL_ANIMATION,
        CHARACTER_SPEAK,
        PLAYER_ROTATE_TOWARDS,
        PARTICLE_SPAWN,
        WAIT,
        FADE_TO_BLACK,
        FADE_FROM_BLACK,
        ADVANCE_SPEECH
    }
    [System.Serializable]
    public struct CutsceneAction
    {
        public int order;
        public CutsceneActionType type;
        public int who; //Actor index
        public GameObject what;
        public int where; //Spot index
        public float howMuch;
    }
    
    [System.Serializable]
    public struct CutscenePart //: UnityEngine.Object
    {
        public int actor;
        public string text;
        public string soundbyteId;
    }
    
    [System.Serializable]
    [CreateAssetMenu(fileName = "Cutscene", menuName = "Cutscenes/Cutscene", order = 0)]
    public class Cutscene : ScriptableObject
    {
        public CutsceneID ID = CutsceneID.INITIAL_CUTSCENE;
        
        [SerializeField]
        public List<CutscenePart> parts = new List<CutscenePart>(new CutscenePart[]{new CutscenePart()});

        [SerializeField] public List<CutsceneAction> actions = new List<CutsceneAction>(new CutsceneAction[]{new CutsceneAction()});
    }
    
    [CreateAssetMenu(fileName = "CutsceneList", menuName = "Cutscenes/CutsceneList", order = 0)]
    public class CutsceneList : ScriptableObject
    {
        [SerializeField] public List<Cutscene> cutscenes = new();

        public Cutscene FindCutscene(CutsceneID id)
        {
            return cutscenes.Find(val => val.ID == id);
        }
    }
}