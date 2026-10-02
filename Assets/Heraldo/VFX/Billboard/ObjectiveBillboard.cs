using System;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

namespace Heraldo.VFX.Billboard
{
    public class ObjectiveBillboard : MonoBehaviour
    {
        public Transform player;
        public TMP_Text text;
        public float sizeOnScreen = 100f;
        private Camera _camera;

        private void Awake()
        {
            _camera = GameObject.FindWithTag("_MainCamera").GetComponent<Camera>();
        }

        private void Update()
        {
            transform.LookAt(new Vector3(player.position.x,transform.position.y,player.position.z));

            Vector3 distance = transform.position - player.position;
            float dist = distance.magnitude;
            text.text = MathF.Round(dist) + "m";
            Vector3 a = _camera.WorldToScreenPoint(transform.position);
            a.z = dist;
            Vector3 b = new Vector3(a.x, a.y + sizeOnScreen, a.z);

            Vector3 aa = _camera.ScreenToWorldPoint(a);
            Vector3 bb = _camera.ScreenToWorldPoint(b);
            
            
            text.rectTransform.localScale = Vector3.one * (aa - bb).magnitude;
            
            //text.transform.localScale = Vector3.one*MathF.Sqrt(dist);
        }
    }
}