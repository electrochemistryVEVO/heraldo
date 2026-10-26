using System;

namespace Heraldo.Global.Structs
{
    [Serializable]
    public struct GameSaveData
    {
        public string lastScene;

        public GameSaveData(string lastScene)
        {
            this.lastScene = lastScene;
        }
    }
}