using System.Collections.Generic;
using LitMotion;
using TMPro;
using UnityEngine;

namespace Heraldo.UI.Components
{
    public enum FeedbackType
    {
        HEALTH_GAINED,
        AMMO_GAINED,
        CAP_REACHED
    }
    public class FeedbackUI : MonoBehaviour
    {
        public GameObject feedbackTextPrefab;

        public Dictionary<FeedbackType, Color> TypeToColor = new()
        {
            { FeedbackType.HEALTH_GAINED, Color.green },
            { FeedbackType.AMMO_GAINED, Color.peru },
            { FeedbackType.CAP_REACHED, Color.red }
        };

        public void SpawnFeedbackText(FeedbackType type,string text)
        {
            TMP_Text instance = Instantiate(feedbackTextPrefab.gameObject, this.transform).GetComponent<TMP_Text>();
            instance.text = text;
            instance.color = TypeToColor[type];
            Vector2 ogPos = instance.rectTransform.anchoredPosition;
            LMotion.Create(0, 1f, 1f)
                .WithOnComplete(() => Destroy(instance))
                .Bind((val) =>
                {
                    instance.rectTransform.anchoredPosition = Vector2.Lerp(ogPos, ogPos + Vector2.up*200, val);
                    instance.alpha = 1 - val;
                })
                .AddTo(this);
        }
    }
}