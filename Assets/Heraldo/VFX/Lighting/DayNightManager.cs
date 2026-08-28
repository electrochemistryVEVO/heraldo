using System;
using UnityEngine;

namespace Heraldo.VFX.Lighting
{
    public class DayNightManager : MonoBehaviour
    {
        public Material cubemapMaterial;
        public Material sunMaterial;
        public string hora;
        
        
        private void Awake()
        {
            cubemapMaterial = RenderSettings.skybox;
            RenderSettings.skybox.Lerp(cubemapMaterial,sunMaterial, 0.5f);
            
        }

        private void Update()
        {
            DynamicGI.UpdateEnvironment();
        }
    }
}