
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.Scripts.Achievements {
    [CreateAssetMenu(fileName = "Achievement Info", menuName = "Achievements")]
    public class AchievementInfo : ScriptableObject {
        public string title = "";
        public string description = "";
        public int rarity;
        public int id;
        public float maxValue;
        public int progress;

        private void Awake() {
            rarity = CheckRequirements();
            Debug.Log(title + " rarity: " + rarity);
        }
        public int CheckRequirements() {
            float threshold = maxValue / 4f;
            if (progress < threshold) {
                return 0; // Criterion not met
            } else if (progress < 2f * threshold) {
                return 1;
            } else if (progress < 3f * threshold) {
                return 2;
            } else if (progress < 4f * threshold) {
                return 3;
            } else if (progress >= maxValue) {
                return 4;
            }
            return 0;
        }
    }
}
