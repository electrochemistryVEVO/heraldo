using System;
using System.Linq;
using Heraldo.Character;
using Heraldo.Character.Pathfinding;
using Heraldo.UI;
using UnityEngine;

namespace Heraldo.Weapon.Holster
{
    public class GenericWpnHolster : MonoBehaviour
    {
        private static readonly int WpnEquipped = Animator.StringToHash("wpnEquipped");
        private static readonly int WpnSelected = Animator.StringToHash("wpnSelected");
        private static readonly int IsCarryingCorpse = Animator.StringToHash("isCarryingCorpse");

        private struct Holster
        {
            public int AmmoCount;

            public Holster(int ammoCount)
            {
                AmmoCount = ammoCount;
            }
        }
        //Arsenal
        private GenericWeapon[] _weapons;
        //public GameObject Weapon2;
        //public GameObject Weapon3;
        
        //Holsters
        //private Holster[] _holsters;
        
        private Inputs _inputs;
        private int _last_wpn_index = 0;
        private bool _nullWpn = false;
        private FPSController _character;
        private Animator _charAnimator;
        
        //Corpse carrying
        public GameObject corpsePrefab;
        private WeaponCorpse _corpse;
        private bool _holdingCorpse = false;
        private bool _prevHolster = false;
        
         
        
        //Debounce values
        private bool _holsterDeb = false;

        public void Awake()
        {
            _inputs = GetComponentInParent<Inputs>();
            _character = GetComponentInParent<FPSController>();
            _weapons = GetComponentsInChildren<GenericWeapon>();
            _corpse = GetComponentInChildren<WeaponCorpse>();
            _charAnimator = _character.CharacterAnimator;
            //Debug.Log(_weapons.Length);
            //_holsters = new Holster[3];
        }

        public int[] GetAmmoCounts()
        {
            return _weapons.Select(val => val.AmmoCount).ToArray();
        }

        public void SetAmmoCount(int i, int value)
        {
            _weapons[i].AmmoCount = value;
        }

        public void Update()
        {
            if (!_holdingCorpse)
            {
                //Debug.Log("Holster: " + _inputs.holster);
                if (_inputs.holster && _nullWpn)
                {
                    //Holster
                    Hold();
                    _nullWpn = false;
                }

                if (!_inputs.holster && !_nullWpn)
                {
                    //Draw
                    Draw();
                    _nullWpn = true;
                }

                if (_inputs.selectedWeapon != _last_wpn_index)
                {
                    Debug.Log("wpn "+_inputs.selectedWeapon+" selected");
                    //TODO: Animations in and out 
                    _last_wpn_index = _inputs.selectedWeapon;
                    Hold();
                    Draw();
                }
            }
            else
            {
                if (_inputs.holster != _prevHolster)
                {
                    DropCorpse();
                }
            }
            _prevHolster = _inputs.holster;
        }

        public int PickupWeapon(int ammoCount,int wpnIndex)
        {
            //Regresa cantidad de municion que ha recogido de verdad
            int realAmmoCount = _weapons[wpnIndex].AmmoCount;
            _weapons[wpnIndex].AmmoCount = Math.Min(_weapons[wpnIndex].AmmoCount + ammoCount, _weapons[wpnIndex].MaxAmmo);
            realAmmoCount = _weapons[wpnIndex].AmmoCount - realAmmoCount;
            Debug.Log("Picked up ammo: " + ammoCount);
            // if (_last_wpn_index != wpnIndex)
            // {
            //     _last_wpn_index = wpnIndex;
            //     Hold();
            //     Draw();
            // } 
            return realAmmoCount;
        }

        public void HoldCorpse()
        {
            //Switch to corpse
            _charAnimator.SetBool(IsCarryingCorpse,true);
            _holdingCorpse = true;
            Hold();
            _character.EquipWeapon(_corpse); 
        }

        public void DropCorpse()
        {
            //Generate element to pick up again
            _charAnimator.SetBool(IsCarryingCorpse,false);
            _holdingCorpse = false;
            GameObject obj = Instantiate(corpsePrefab);
            obj.transform.position = _character.transform.TransformPoint(Vector3.forward * 10);
            Hold();
            Draw();
        }

        void Hold()
        {
            //Debug.Log("Weapon holstered: "+_last_wpn_index);
            _character.EraseWeapon(); 
            _charAnimator.SetBool(WpnEquipped,false);
        }

        void Draw()
        {
            //Debug.Log("Weapon drawn: "+_last_wpn_index);
            _charAnimator.SetBool(WpnEquipped,true);
            _charAnimator.SetInteger(WpnSelected,_last_wpn_index);
            _character.EquipWeapon(_weapons[_last_wpn_index]); 
        }
    }
}