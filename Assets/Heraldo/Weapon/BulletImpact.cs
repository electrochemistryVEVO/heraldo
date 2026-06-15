using System.Collections;
using UnityEngine;

namespace Heraldo.Weapon
{
    public class BulletImpact : MonoBehaviour
    {
        public GameObject bulletPrefab;
        public GameObject bulletFleshPrefab;

        private IEnumerator DestroyParticle(GameObject particle)
        {
            yield return new WaitForSeconds(2.0f);
            Destroy(particle);
        }
        
        public void Impact(Vector3 target,Vector3 normal)
        {
            var obj = Instantiate(bulletPrefab, 
                target, 
                Quaternion.LookRotation(normal,Vector3.up)
                );
            //var emitter = obj.GetComponent<ParticleSystem>();
            //emitter.Emit(1);
            StartCoroutine(DestroyParticle(obj));
        }

        public void FleshImpact(Vector3 target, Vector3 normal)
        {
            //VFX
            var obj = Instantiate(bulletFleshPrefab, 
                target, 
                Quaternion.LookRotation(normal,Vector3.up)
            );
            StartCoroutine(DestroyParticle(obj));
        }
    }
}