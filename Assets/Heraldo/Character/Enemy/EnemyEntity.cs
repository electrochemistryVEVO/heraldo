using UnityEngine;

namespace Heraldo.Character.Enemy
{
    public class EnemyEntity : MonoBehaviour
    {
        public void QueueFree()
        {
            foreach (var child in GetComponentsInChildren<Transform>())
            {
                Destroy(child.gameObject);   
            }
        }
    }
}