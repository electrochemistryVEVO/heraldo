using System;
using JetBrains.Annotations;

namespace Heraldo.Global.Structs
{
    [Serializable]
    public struct GameSaveData
    {
        public string lastScene;
        public int playerHealth;
        public int rifleAmmo;
        [CanBeNull] public string timeString;

        public GameSaveData(string lastScene,int playerHealth,int rifleAmmo, string timeString = null)
        {
            this.lastScene = lastScene;
            this.playerHealth = playerHealth;
            this.rifleAmmo = rifleAmmo;
            this.timeString = timeString;
        }

        public void SetTimeString(string val)
        {
            timeString = val;
        }
    }
}