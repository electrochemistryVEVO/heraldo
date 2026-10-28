using System;
using System.Threading.Tasks;
using Heraldo.Global.Structs;
using LitMotion;
using LitMotion.Extensions;
using TMPro;
using UnityEngine;
using UnityFigmaBridge.Runtime.UI;

namespace Heraldo.UI.Components
{
    public class ObjectivePopup : MonoBehaviour
    {
        public FigmaImage checkmark;
        public TMP_Text text;
        public float offscreenOffsetY;
        private float _originalOffsetY;
        public RectTransform rectTransform;
        private MotionHandle _objectiveTween;

        private void Awake()
        {
            _originalOffsetY = transform.localPosition.y;
            //_rectTransform = (RectTransform)transform;
        }

        public void InitObjective(Objective objective)
        {
            //Special function that doesnt trigger the animation
            ShowHideCheckmark(false);
            text.text = objective.Title;
        }

        private void ShowHideCheckmark(bool show = true)
        {
            checkmark.FillColor = new Color(checkmark.FillColor.r,
                checkmark.FillColor.g,
                checkmark.FillColor.b,
                show ? 1f : 0);
        }
        
        public void ChangeObjective(Objective objective)
        {
            _originalOffsetY = rectTransform.localPosition.y;
            try
            {
                if(_objectiveTween.IsPlaying())_objectiveTween.Cancel();
                LSequence.Create()
                    .Append(LMotion.Create(_originalOffsetY,offscreenOffsetY,0.35f)
                        .WithEase(Ease.InBounce)
                        .WithOnComplete(() =>
                        {
                            text.text = "Objetivo: "+ objective.Title;
                            ShowHideCheckmark(false);
                        })
                        .WithCancelOnError()
                        .BindToLocalPositionY(rectTransform)
                        .AddTo(gameObject))
                    .Append(LMotion.Create(offscreenOffsetY,_originalOffsetY,0.35f)
                        .WithEase(Ease.OutSine)
                        .WithScheduler(MotionScheduler.FixedUpdate)
                        .BindToLocalPositionY(rectTransform)
                        .AddTo(gameObject))
                    .Run();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void ShowCompleteObjective(Objective objective)
        {
            ShowHideCheckmark(); 
            Task.Delay(500).ContinueWith(async task =>
            {
                await Awaitable.MainThreadAsync();
                ChangeObjective(objective);
            });
        }
    }
}