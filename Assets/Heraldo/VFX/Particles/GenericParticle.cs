using System;
using UnityEngine;

namespace Heraldo.VFX.Particles
{
    public class GenericParticle : MonoBehaviour
    {
        public void Start()
        {
            Emit();
        }

        public virtual void Emit() { }
    }
}