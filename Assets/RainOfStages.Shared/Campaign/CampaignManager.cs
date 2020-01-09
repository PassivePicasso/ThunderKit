using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RainOfStages.Campaign
{
    [RequireComponent(typeof(Image))]
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignDefinition[] LoadedCampaigns;

        Image image;
        public CampaignDefinition SelectedCampaign { get; private set; }
        private int campaignIndex = 0;
        // Start is called before the first frame update
        void Start()
        {
            image = GetComponent<Image>();
            SelectedCampaign = LoadedCampaigns[campaignIndex];
        }

        public void Next()
        {
            campaignIndex++;
            SelectedCampaign = LoadedCampaigns[campaignIndex];
            image.sprite = Sprite.Create(SelectedCampaign.previewTexture, new Rect(0, 0, 100, 100), Vector2.zero);
        }
        public void Prev()
        {
            campaignIndex--;
            SelectedCampaign = LoadedCampaigns[campaignIndex];
            image.sprite = Sprite.Create(SelectedCampaign.previewTexture, new Rect(0, 0, 100, 100), Vector2.zero);
        }
    }
}
