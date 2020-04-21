using RainOfStages.Proxy;
using RoR2;
using UnityEngine;

namespace RainOfStages.Interactable
{
    public class ShopTerminalProxy : MonoBehaviourProxy<ShopTerminalBehavior>
    {
        [Tooltip("The PickupDisplay component that should show which item this shop terminal is offering.")]
        public PickupDisplay pickupDisplay;
        [Tooltip("The position from which the drop will be emitted")]
        public Transform dropTransform;
        [Tooltip("Whether or not the shop terminal should drive itself")]
        public bool selfGeneratePickup;
        [Tooltip("The drop table to select a pickup index from - only works if the pickup generates itself")]
        public PickupDropTable dropTable;
        [Tooltip("The tier of items to drop - only works if the pickup generates itself and the dropTable field is empty.")]
        public ItemTier itemTier;
        public ItemTag bannedItemTag;
        [Tooltip("The velocity with which the drop will be emitted. Rotates with this object.")]
        public Vector3 dropVelocity;
        public Animator animator;
    }
}