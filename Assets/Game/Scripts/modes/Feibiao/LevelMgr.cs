
using Assets.Scripts.common;
using Assets.Scripts.config;
using System.Collections.Generic;
using static Assets.Game.Scripts.modes.Feibiao.ExtraLevelDifficulty;

namespace Assets.Game.Scripts.modes.Feibiao
{
    class LevelMgr
    {

        public static string GetBundleName(int level)
        {
            GameMode currentMode = GameManager.Instance.currMode.id;
            if (currentMode == null) return GetDefaultBundleName(level);

            switch (currentMode)
            {
                case GameMode.NewYear:
                    return "NewYearTheme";
                case GameMode.ShengXiao:
                    return "ShengXiaoTheme";
                case GameMode.NeZha:
                    return "NeZhaTheme";
                case GameMode.NvShen:
                    return "NvShenTheme";
                case GameMode.Spring:
                    return "SpringTheme";
                case GameMode.FiveOne:
                    return "FiveOneTheme";
                default:
                    return GetDefaultBundleName(level);
            }
        }

        private static string GetDefaultBundleName(int level)
        {
            int lvIndex = GetLevelIndex(level);
            if (lvIndex <= 30) return "Levels1";
            if (lvIndex <= 60) return "Levels2";
            if (lvIndex <= 100) return "Levels3";
            if (lvIndex <= 200) return "Levels4";
            if (lvIndex <= 300) return "Levels5";
            if (lvIndex <= 400) return "Levels6";
            if (lvIndex <= 500) return "Levels7";
            if (lvIndex <= 600) return "Levels8";
            if (lvIndex <= 700) return "Levels9";
            if (lvIndex <= 800) return "Levels10";
            if (lvIndex <= 900) return "Levels11";
            return "Levels12";
        }

        public static int GetLevelIndex(int level)
        {
            int lvIndex = level;
            if (level > ConstVal.ROPE_MAX_LEVEL)
            {
                lvIndex = ((level - 1) % ConstVal.ROPE_MAX_LEVEL) + 1;
            }
            return lvIndex;
        }


        public static string GetPrefabName(int level)
        {
            GameMode currentMode = GameManager.Instance.currMode.id;
            if (currentMode == null) return GetDefaultPrefabName(level);

            switch (currentMode)
            {
                case GameMode.NewYear:
                case GameMode.ShengXiao:
                case GameMode.NeZha:
                case GameMode.NvShen:
                case GameMode.Spring:
                case GameMode.FiveOne:
                    return level.ToString();
                default:
                    return GetDefaultPrefabName(level);
            }
        }

        private static string GetDefaultPrefabName(int level)
        {
            int lvIndex = GetLevelIndex(level);
            List< string > levelOrder = LevelsConfig.LevelOrder; //RemoteConfig.Instance.LevelOrder ?? LevelsConfig.LevelOrder;
            string resId = levelOrder[lvIndex];
            return string.IsNullOrEmpty(resId) ? lvIndex.ToString() : resId;
        }

        public static string GetPaintOrder(int level)
        {
            GameMode currentMode = GameManager.Instance.currMode.id;
            if (currentMode == null) return GetDefaultPaintOrder(level);

            switch (currentMode)
            {
                case GameMode.NewYear:
                case GameMode.ShengXiao:
                case GameMode.NeZha:
                case GameMode.NvShen:
                case GameMode.Spring:
                case GameMode.FiveOne:
                    return level.ToString();
                default:
                    return GetDefaultPaintOrder(level);
            }
        }

        private static string GetDefaultPaintOrder(int level)
        {
            int lvIndex = GetLevelIndex(level);
            List<string> paintOrder = LevelsConfig.PaintOrder; //RemoteConfig.Instance.PaintOrder ?? LevelsConfig.PaintOrder;
            string resId = paintOrder[lvIndex];
            return string.IsNullOrEmpty(resId) ? lvIndex.ToString() : resId;
        }

        //LevelDifficulty
        public static List<DifficultyRange> GetLevelDifficultyConfig(int level)
        {
            GameMode modeID = GameManager.Instance.currMode.id;
            if (modeID != GameMode.Feibiao)
            {
                return ExtraLevelDifficulty.Ranges;
            }

            //window['RemoteLevelConfig']
            // 远程  RemoteConfig.Instance.LevelDifficulty ??

            DifficultyConfig difficultyConfig = LevelsConfig.LevelDifficulty;
            if(difficultyConfig != null)
            {
                return difficultyConfig["normal"];
            }
            return difficultyConfig["level"];
        }
    }
}
