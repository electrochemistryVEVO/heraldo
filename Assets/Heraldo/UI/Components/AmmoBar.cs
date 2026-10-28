using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityFigmaBridge.Runtime.UI;

namespace Heraldo.UI.Components
{
    public class AmmoBar : MonoBehaviour
    {
        public GameObject gaugePiecePrefab;
        private FigmaImage _image;
        private FigmaImage[] _gaugePartList;
        private int _currentAmmo;

        private void Awake()
        {
            _image = GetComponent<FigmaImage>();
            _gaugePartList = GetComponentsInChildren<FigmaImage>()
                .Where(val => val.gameObject != gameObject)
                .ToArray();
        }

        public void ShowOrHide(bool show)
        {
            _image.enabled = show;
            for (int i = 0; i < (!show ? _gaugePartList.Length : _currentAmmo); i++)
            {
                _gaugePartList[i].enabled = show;
            }
        }

        public void SetAmmo(int ammo)
        {
            if (ammo < 0)
            {
                ShowOrHide(false);
                return;
            }

            _currentAmmo = ammo;
            for (int i = 0; i < _gaugePartList.Length; i++)
            {
                _gaugePartList[i].enabled = (i < ammo);
            }
        }
    }
}