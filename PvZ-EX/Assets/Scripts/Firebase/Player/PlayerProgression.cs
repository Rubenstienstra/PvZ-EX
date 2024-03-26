
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Code.Scripts.Firebase.Player
{
    public class PlayerProgression : MonoBehaviour
    {
        #region Public Fields
        
        [Header("Events")]
        [Tooltip("Old Experience, New Experience")]
        public UnityEvent<long, long> onExperienceChanged;
        
        [Tooltip("Old Currency, New Currency")]
        public UnityEvent<long, long> onCurrencyChanged;
        
        [Tooltip("Old Level, New Level")]
        public UnityEvent<long, long> onLevelChanged;

        #endregion
        
        #region Private Fields

        [SerializeField] 
        private FirebaseProgressionInventory progressionInventory;

        public UiManager uiManager;

        #endregion

        #region Public Methods
        public long experienceRequired;
        public long startingExperienceRequired = 100;

        public void GiveCurrency(int currencyToGive)
        {
            var oldCurrency = progressionInventory.localUserProgression.currency;
            progressionInventory.localUserProgression.currency += currencyToGive;
            
            progressionInventory.AddingProgression(false);
            onCurrencyChanged.Invoke(oldCurrency, progressionInventory.localUserProgression.currency);
        }

        public void GiveExperience(int experienceToGive)
        {
            var oldExperience = progressionInventory.localUserProgression.xp;
            progressionInventory.localUserProgression.xp += experienceToGive;
            StartCoroutine(AttemptLevelUp());
            
            progressionInventory.AddingProgression(false);
            onExperienceChanged.Invoke(oldExperience, progressionInventory.localUserProgression.xp);
            uiManager.XpBar();
        }

        #endregion

        #region Private Methods

        private void Start()
        {
            onExperienceChanged.AddListener((long oldExperience, long newExperience) =>
            {
                Debug.Log($"Went from e {oldExperience} to {newExperience}");
            });
            
            onCurrencyChanged.AddListener((long oldCurrency, long newCurrency) =>
            {
                Debug.Log($"Went from c {oldCurrency} to {newCurrency}");
            });
            
            onLevelChanged.AddListener((long oldLevel, long newLevel) =>
            {
                Debug.Log($"Went from l {oldLevel} to {newLevel}");
            });
        }

        public IEnumerator AttemptLevelUp()
        {
            yield return new WaitUntil(() => progressionInventory.resyncedProgression);
            long crLevel = progressionInventory.localUserProgression.level;
            CalculateXPRequired();
            uiManager.XpBar();
            if (progressionInventory.localUserProgression.xp >= experienceRequired)//Does nothing if he doesn't have enough xp to level up.
            {
                progressionInventory.localUserProgression.xp -= experienceRequired;
                progressionInventory.localUserProgression.level++;
                CalculateXPRequired();
                uiManager.XpBar();
                onLevelChanged.Invoke(crLevel, progressionInventory.localUserProgression.level);
                progressionInventory.AddingProgression(false);
            }

            yield return null;
        }

        private void CalculateXPRequired(long? crLevel = null)
        {
            if(crLevel == null)
            {
                crLevel = progressionInventory.localUserProgression.level;
            }
            experienceRequired = 100 + (crLevel.Value * 100 + 50 * crLevel.Value);
            progressionInventory.localUserProgression.xpRequired = experienceRequired;
        }

        #endregion
    }
}

/*

public void AddMoney(long moneyToAdd)
    {
        localUserProgression.currency += moneyToAdd;
    }

    public void AddExperience(long xpToAdd)
    {
        localUserProgression.xp += xpToAdd;
        while (localUserProgression.xp > 1000)
        {
            LevelUp();
            localUserProgression.xp -= 1000;
        }
    }

    private void LevelUp()
    {
        localUserProgression.level++;
    }

*/