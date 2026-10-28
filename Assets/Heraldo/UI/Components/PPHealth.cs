using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Heraldo.UI.Components
{
    public class PPHealth : MonoBehaviour
    {
        private Volume _volume;
        private ChromaticAberration _chroma;
        private ColorAdjustments _color;
        private Vignette _vignette;

        public float startingVignetteVal = 0.3f;
        public float endingVignetteVal = 0.7f;
        
        public float startingColorVal = 0.3f;
        public float endingColorVal = 0.7f;

        public float startingChromaVal = 0;
        public float endingChromaVal = 1f;

        private MotionHandle _aberrationTween;
        private CancellationTokenSource _aberrationTweenToken = new();
        

        private void Awake()
        {
            _volume = GetComponent<Volume>();
            _vignette = (Vignette)_volume.profile.components.Find(
                val => val is Vignette);
            _color = (ColorAdjustments)_volume.profile.components.Find(
                val => val is ColorAdjustments);
            _chroma = (ChromaticAberration)_volume.profile.components.Find(
                val => val is ChromaticAberration);
        }

        

        public void ShowDamage(int dmg,int maxHealth)
        {
            if(_aberrationTween.IsPlaying())_aberrationTween.Cancel();
            _aberrationTween = LMotion.Punch.Create(startingChromaVal, endingChromaVal, 0.5f)
                .WithFrequency(1)
                .WithEase(Ease.InOutSine)
                .WithDampingRatio(0)
                .Bind(x => _chroma.intensity.Override(x))
                .AddTo(gameObject)
                .Preserve();
        }

        public void SetHealth(int health,int maxHealth)
        {
            float perc = (1 - (health * 1.0f) / maxHealth);
            float vignetteVal = startingVignetteVal 
                                + (endingVignetteVal-startingVignetteVal) * perc;
            float colorVal = startingColorVal 
                                + (endingColorVal-startingColorVal) * perc;
            _vignette.intensity.Override(vignetteVal);
            _color.contrast.Override(colorVal);
            //_volume.components = new List<VolumeComponent>() { _vignette, _color, _chroma };
        }
    }
}