using System;
using Heraldo.Level;
using UnityEngine;

namespace Heraldo.VFX.Billboard
{
    public class AmmoBoxBillboard : Billboard
    {
        public AmmoBox AmmoBox;

        public override void Awake()
        {
            base.Awake();
            AmmoBox = GetComponent<AmmoBox>();
        }
        public override void Update()
        {
            base.Update();
            
            text.text = "Mun.: " + AmmoBox.AmmoPool;
        }
    }
}