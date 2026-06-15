using UnityEngine;

namespace Heraldo.Global.Structs
{
    public struct DamageInfo
    {
        public int damage;
        public Vector3? enemyPos;

        public DamageInfo(int _damage,Vector3 _enemyPos)
        {
            damage = _damage;
            enemyPos = _enemyPos;
        }
    }
}