using System;
using Heraldo.Character.Enemy;
using Heraldo.Global.Enums;
using StarterAssets;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Heraldo.Weapon
{
    public abstract class GenericWeapon : MonoBehaviour
    {
        
        [Header("Game Logic")]
        public float ReloadTime;
        public float Range;
        public int MaxAmmo;
        public int Damage;
        public int AmmoCount;
        public bool IsAlly = true;
        public Vector2 MaxMinRecoil;
        public bool Enabled;
        public abstract TimelineID ShootTimelineId();
        protected NPCHearingManager _hearingManager;
        
        [Header("Visual Weapon")]
        public GameObject ViewModel;
        

        private Renderer[] _viewModelRenderers;

        private bool _isMelee = false;

        public virtual void Awake()
        {
            if (MaxAmmo < 0 || AmmoCount < 0) _isMelee = true;
            _viewModelRenderers = ViewModel.GetComponentsInChildren<Renderer>();
            _hearingManager = GameObject.FindWithTag("NPCSensesManager").GetComponent<NPCHearingManager>();
        }

        private Vector3 Spread(Vector3 direction, float spreadMultiplier)
        {
            Vector2 spreadSeed = (Random.insideUnitCircle
                                  * (MaxMinRecoil.y - MaxMinRecoil.x)
                                  + new Vector2(MaxMinRecoil.x, MaxMinRecoil.x))
                                 * spreadMultiplier;
            return Vector3.ProjectOnPlane(new Vector3(spreadSeed.x,spreadSeed.y,0),direction);
        }

        public bool Fire(Vector3 position,Vector3 direction,float spreadMultiplier=0)
        {
            if (!Enabled) return false;
            Debug.Log("Ammo: " + AmmoCount);
            int actualAmmo = AmmoCount;
            if(!_isMelee)AmmoCount = Math.Max(AmmoCount-1,0); 
            if(actualAmmo>0 || _isMelee)
            {
                FireAt(position, direction + Spread(direction, spreadMultiplier));
                return true;
            }
            return false;
        }

        public void ShowHide(bool show = true)
        {
            foreach(Renderer renderer in _viewModelRenderers)
            {
                renderer.enabled = show;
            }
            //_viewModelRenderer.enabled = show;
            //if(!show)ViewModel.transform.SetParent(transform);
        }
        protected virtual void FireAt(Vector3 position,Vector3 direction){}
    }
}