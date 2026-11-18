using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Heraldo.UI.Components
{
    public enum MarkerType
    {
        OBJECTIVE,
        OBJECTIVE_INTERACT,
        HEALTH,
        AMMO
    }
    public struct Marker
    {
        public Vector3 position;
        public bool enabled;

        public Marker(MarkerType markerType,int shaderAngleId) : this()
        {
            this.markerType = markerType;
            this.position = Vector3.zero;
            this.enabled = false;
            this.shaderAngleId = shaderAngleId;
        }

        public MarkerType markerType;
        public int shaderAngleId;
    }
    public class CompassUI : MonoBehaviour
    {
        public GameObject needle;
        public Dictionary<MarkerType,Marker> markers = new()
        {
            { MarkerType.AMMO , new Marker(MarkerType.AMMO,Shader.PropertyToID("_AmmoAngle"))},
            { MarkerType.OBJECTIVE , new Marker(MarkerType.OBJECTIVE,Shader.PropertyToID("_ObjectiveAngle"))},
            { MarkerType.HEALTH , new Marker(MarkerType.HEALTH,Shader.PropertyToID("_HealthAngle"))},
            { MarkerType.OBJECTIVE_INTERACT , new Marker(MarkerType.OBJECTIVE_INTERACT,Shader.PropertyToID("_ObjectiveInteractAngle"))}
        };
        private static readonly int Angle = Shader.PropertyToID("_Angle");
        private Image _compass;
        private Texture2D _displayTxt;

        private void Awake()
        {
            _compass = GetComponent<Image>();
        }

        private void Start()
        {
            
        }

        void SetIconsPositions(MarkerType type, Vector3 pos,Quaternion rot)
        {
            var temp = markers[type];
            var front = rot.eulerAngles.y;
            if (temp.enabled)
            {
                var dist = Quaternion.LookRotation(pos - temp.position).eulerAngles.y;
                //Debug.Log("[COMPASS] Current angle: "+ (dist - front));
                var dif = front - dist;
                _compass.material.SetFloat(temp.shaderAngleId,dif>0 ? front-dist-360 : front-dist);
            }
            else
            {
                _compass.material.SetFloat(temp.shaderAngleId,front+180);
            }
        }

        public void Active(bool active)
        {
            gameObject.SetActive(active);
            needle.SetActive(active);
        }

        void SetCompassOrientation(Quaternion playerRotation)
        {
            var angle = playerRotation.eulerAngles.y;
            _compass.material.SetFloat(Angle,angle);
        }

        public void SetMarkerPosition(MarkerType type, Vector3 pos)
        {
            Marker marker;
            if (markers.TryGetValue(type,out marker))
            {
                marker.position = pos;
                marker.enabled = true;
                if (type == MarkerType.OBJECTIVE)
                {
                    var temp = markers[MarkerType.OBJECTIVE_INTERACT];
                    temp.enabled = false;
                    markers[MarkerType.OBJECTIVE_INTERACT] = temp;
                }
                if (type == MarkerType.OBJECTIVE_INTERACT)
                {
                    var temp = markers[MarkerType.OBJECTIVE];
                    temp.enabled = false;
                    markers[MarkerType.OBJECTIVE] = temp; 
                }

                markers[type] = marker;
            }
        }

        public void UpdateByPlayerTransform(Vector3 position, Quaternion rotation)
        {
            foreach (var key in markers.Keys)
            {
                SetIconsPositions(key,position,rotation);
            }
            SetCompassOrientation(rotation);
        }
    }
}