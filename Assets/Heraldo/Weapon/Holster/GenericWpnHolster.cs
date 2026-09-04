using System;
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
        
         
        
        //Debounce values
        private bool _holsterDeb = false;

        public void Awake()
        {
            _inputs = GetComponentInParent<Inputs>();
            _character = GetComponentInParent<FPSController>();
            _weapons = GetComponentsInChildren<GenericWeapon>();
            _charAnimator = _character.CharacterAnimator;
            //Debug.Log(_weapons.Length);
            //_holsters = new Holster[3];
        }

        public void Update()
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

        public void PickupWeapon(int ammoCount,int wpnIndex)
        {
            _weapons[wpnIndex].AmmoCount = Math.Min(_weapons[wpnIndex].AmmoCount + ammoCount, _weapons[wpnIndex].MaxAmmo); 
            Debug.Log("Picked up ammo: " + ammoCount);
            if (_last_wpn_index != wpnIndex)
            {
                _last_wpn_index = wpnIndex;
                Hold();
                Draw();
            }
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