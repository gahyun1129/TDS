using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Solo.MOST_IN_ONE
{
    public class HapticsManager : MonoBehaviour
    {

        #region Singleton
        public static HapticsManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            Initialize();
        }
        #endregion

        void Initialize()
        {
            MOST_HapticFeedback.HapticsEnabled = true;
        }

        public void SoftImpactHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.SoftImpact);
        }

        public void AttackImpactHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.MediumImpact);
        }

        public void SelectionHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.Selection);
        }

        public void SuccessHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.Success);
        }

        public void WarningHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.Warning);
        }

        public void FailureHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.Failure);
        }

        public void LightImpactHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.LightImpact);
        }

        public void MediumImpactHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.MediumImpact);
        }

        public void HeavyImpactHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.HeavyImpact);
        }

        public void RigidImpactHaptic()
        {
            MOST_HapticFeedback.Generate(MOST_HapticFeedback.HapticTypes.RigidImpact);
        }
    }
}
