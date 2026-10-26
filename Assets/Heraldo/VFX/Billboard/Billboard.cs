using System;
using TMPro;
using UnityEngine;

namespace Heraldo.VFX.Billboard
{
    public class Billboard : MonoBehaviour
    {
        public Transform player; 
        public float sizeOnScreen = 100f;
        private Camera _camera;
        public TMP_Text text;

        public virtual void Awake()
        {
            //_camera = GameObject.FindWithTag("_MainCamera").GetComponent<Camera>();  
        }

        public virtual void Update()
        {
            if (_camera is null)
            {
                _camera = GameObject.FindWithTag("_MainCamera").GetComponent<Camera>();
                if (_camera != null)
                {
                    //player = _camera.transform;
                    Debug.Log("Camara conseguida");
                }
                else
                {
                    throw new Exception("No camera for billboards");
                }
            }

            if (player == null)
            {
                player = GameObject.FindWithTag("Player").transform;
                if (player != null) Debug.Log("Jugador conseguido");
                else throw new Exception("No player for billboard");
            }
            
            transform.LookAt(new Vector3(player.position.x,transform.position.y,player.position.z));

            Vector3 distance = transform.position - player.position;
            float dist = distance.magnitude;
            
            Vector3 a = _camera.WorldToScreenPoint(transform.position);
            a.z = dist;
            Vector3 b = new Vector3(a.x, a.y + sizeOnScreen, a.z);

            Vector3 aa = _camera.ScreenToWorldPoint(a);
            Vector3 bb = _camera.ScreenToWorldPoint(b);

            //float worldHeight = (aa - bb).magnitude;
            float _distance = Vector3.Dot(
                transform.position - _camera.transform.position,
                _camera.transform.forward
            ); 
            //_distance = Mathf.Max(_distance, _camera.nearClipPlane);
 
            float worldHeight = sizeOnScreen *
                                (2f * _distance *
                                 Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad)) /
                                _camera.pixelHeight; 
           // text.rectTransform.localScale = Vector3.one * worldHeight;
           transform.localScale = Vector3.one * worldHeight;
           //text.transform.localScale = Vector3.one*MathF.Sqrt(dist);
        }
    }
}