using RoR2;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace RainOfStages.Interactable
{
    [Serializable]
    public class PurchaseEvent : UnityEvent<Interactor>
    {

    }
    public abstract class PurchaseInteractionProxy : MonoBehaviour
    {
        public bool available = true;
        [Tooltip("The unlockable that a player must have to be able to interact with this terminal.")]
        public string requiredUnlockable = "";
        public string displayNameToken;
        public string contextToken;
        public CostTypeIndex costType;
        public int cost;
        public bool automaticallyScaleCostWithDifficulty;
        public bool ignoreSpherecastForInteractability;
        public string[] purchaseStatNames;
        public bool setUnavailableOnTeleporterActivated;

        public float displayDistance;
        public bool disableHologramRotation;
        public Transform hologramPivot;


        // Start is called before the first frame update
        public virtual void Awake()
        {
            Debug.Log("Constructing PurchaseInteraction");
            var pi = this.gameObject.AddComponent<PurchaseInteraction>();
            pi.available = available;
            pi.requiredUnlockable = requiredUnlockable;
            pi.displayNameToken = displayNameToken;
            pi.contextToken = contextToken;
            pi.cost = cost;
            pi.costType = costType;
            pi.automaticallyScaleCostWithDifficulty = automaticallyScaleCostWithDifficulty;
            pi.ignoreSpherecastForInteractability = ignoreSpherecastForInteractability;
            pi.purchaseStatNames = purchaseStatNames;
            pi.setUnavailableOnTeleporterActivated = setUnavailableOnTeleporterActivated;
            pi.onPurchase = new RoR2.PurchaseEvent();
            pi.onPurchase.AddListener(new UnityAction<Interactor>(OnPurchase));

        }

        public abstract void OnPurchase(Interactor interactor);
    }
}