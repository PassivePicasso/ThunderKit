using UnityEngine;

namespace RainOfStages.Proxies
{
    [CreateAssetMenu(menuName = "ROR2/WaveIntervalSetting")]
    public class WaveIntervalSetting : ScriptableObject
    {
        [SerializeField]
        public float[] moneyWaveIntervalsMin;
        [SerializeField]
        public float[] moneyWaveIntervalsMax;

    }
}