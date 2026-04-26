using UnityEngine;

namespace Heraldo.VFX.Particles
{
    public class BulletImpactVFX : GenericParticle
    {
        private ParticleSystem _beamLayer;
        private ParticleSystem _shardLayer;

        private void Awake()
        {
            _beamLayer = GetComponent<ParticleSystem>();
            _shardLayer = transform.GetChild(0).GetComponent<ParticleSystem>();
        }
        public override void Emit()
        {
            _beamLayer.Emit(1);
            _shardLayer.Emit(10);
        }
    }
}