using System;
using UnityEngine;

namespace ButchersGames
{
    public class LevelManager : MonoBehaviour
    {
        private const string SavedLevelKey = "RunRich.LevelIndex";

        [SerializeField] private bool editorMode;
        [SerializeField] private LevelsList levels;

        public int CurrentLevelIndex;

        public Level CurrentLevel { get; private set; }

        public event Action<Level> OnLevelLoaded;

        public void Init()
        {
            int index = editorMode ? CurrentLevelIndex : PlayerPrefs.GetInt(SavedLevelKey, 0);

            SelectLevel(index);
        }

        public void SelectLevel(int index)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (levels == null || levels.lvls == null || levels.lvls.Count == 0)
            {
                Debug.LogError("LevelManager: список уровней пуст.", this);
                return;
            }

            int count = levels.lvls.Count;
            int correctedIndex = ((index % count) + count) % count;
            Level prefab = levels.lvls[correctedIndex];

            if (prefab == null || prefab.PlayerSpawnPoint == null)
            {
                Debug.LogError("LevelManager: у уровня отсутствует prefab или точка старта.", this);
                return;
            }

            if (CurrentLevel != null)
            {
                CurrentLevel.gameObject.SetActive(false);
                Destroy(CurrentLevel.gameObject);
            }

            CurrentLevelIndex = correctedIndex;
            CurrentLevel = Instantiate(prefab, transform);

            PlayerPrefs.SetInt(SavedLevelKey, CurrentLevelIndex);
            PlayerPrefs.Save();

            OnLevelLoaded?.Invoke(CurrentLevel);
        }

        public void RestartLevel()
        {
            SelectLevel(CurrentLevelIndex);
        }

        public void NextLevel()
        {
            SelectLevel(CurrentLevelIndex + 1);
        }

        public void PrevLevel()
        {
            SelectLevel(CurrentLevelIndex - 1);
        }
    }
}