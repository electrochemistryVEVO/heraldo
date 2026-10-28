using System;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace Heraldo.UI.Components
{
    public class WpnLoading : MonoBehaviour
    {
        //A falling bullet for the rifle. A falling machete for the machete.
        public RectTransform FallingWeapon;
        public Vector3 fallingWpnDisappearOffset;
        public Vector3 fallingWpnAppearOffset;
        public Vector3 initialWpnOffset;
        private Vector3 _initialEulerAngles;

        private void Start()
        {
            _initialEulerAngles = FallingWeapon.eulerAngles;
        }

        public void ToggleAmmoLoad(bool loading)
        {
            if (!loading)
            {
                FallingWeapon.eulerAngles = _initialEulerAngles;
                LMotion.Create(fallingWpnDisappearOffset,initialWpnOffset, 0.35f)
                    .WithEase(Ease.OutSine)
                    .BindToAnchoredPosition3D(FallingWeapon)
                    .AddTo(gameObject);
            }
            else
            {
                FallingWeapon.eulerAngles = _initialEulerAngles;
                LMotion.Create(initialWpnOffset,fallingWpnAppearOffset, 0.35f)
                    .WithEase(Ease.OutSine)
                    .BindToAnchoredPosition3D(FallingWeapon)
                    .AddTo(gameObject);
                LMotion.Create(_initialEulerAngles.z, 330, 0.35f)
                    .WithEase(Ease.OutExpo)
                    .BindToEulerAnglesZ(FallingWeapon)
                    .AddTo(gameObject);
            }
        }
    }
}