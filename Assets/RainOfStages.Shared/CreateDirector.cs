using RoR2;
using RainOfStages.Proxies;
using UnityEngine;
using UnityEngine.Networking;

namespace RainOfStages
{
    public class CreateDirector : MonoBehaviour
    {
        public GameObject teleporterInstance;
        public DirectorCardProxy teleporterCardProxy;
        public WaveIntervalSetting waveIntervalSetting;



        private void Awake()
        {
            if (NetworkServer.active)
            {
                float[] minIntervals = waveIntervalSetting.moneyWaveIntervalsMin;
                float[] maxIntervals = waveIntervalSetting.moneyWaveIntervalsMax;

                if (minIntervals.Length != maxIntervals.Length)
                    throw new System.Exception("wave interval min max values are not the same length.");

                RoR2.RangeFloat[] intervals = new RangeFloat[minIntervals.Length];
                for (int i = 0; i < minIntervals.Length; i++)
                {
                    var min = minIntervals[i];
                    var max = maxIntervals[i];
                    intervals[i] = new RangeFloat { min = min, max = max };
                }

                gameObject.SetActive(false);

                var directorCore = gameObject.AddComponent<DirectorCore>();
                var sceneDirector = gameObject.AddComponent<SceneDirector>();

                sceneDirector.teleporterInstance = teleporterInstance;
                sceneDirector.teleporterSpawnCard = teleporterCardProxy.ToDirectorCard().spawnCard;

                var combatDirectorA = gameObject.AddComponent<CombatDirector>();
                combatDirectorA.moneyWaveIntervals = intervals;
                

                var combatDirectorB = gameObject.AddComponent<CombatDirector>();
                combatDirectorB.moneyWaveIntervals = intervals;

                gameObject.SetActive(true);
            }
            Destroy(this);
            Debug.Log($"Completed {GetType().Name}");
        }
    }

}