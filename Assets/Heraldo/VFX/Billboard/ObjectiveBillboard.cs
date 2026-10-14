using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

namespace Heraldo.VFX.Billboard
{
    public class ObjectiveBillboard : Billboard
    {
        public override void Update()
        { 
            base.Update();
            Vector3 distance = transform.position - player.position;
            float dist = distance.magnitude;
            text.text = MathF.Round(dist) + "m"; 
        }
    }
}