using System.Collections.Generic;

namespace Assets.Game.Scripts.modes.Feibiao
{
    public class LevelsConfig
    {
        public static DifficultyConfig LevelDifficulty = new DifficultyConfig
        {
            ["normal"] = new List<DifficultyRange>
                {
                    new DifficultyRange(0, 0),
                    new DifficultyRange(0.05, 4),
                    new DifficultyRange(0.1, 4),
                    new DifficultyRange(0.2, 5),
                    new DifficultyRange(0.3, 6),
                    new DifficultyRange(0.4, 7),
                    new DifficultyRange(0.5, 8),
                    new DifficultyRange(0.6, 9),
                    new DifficultyRange(0.7, 9),
                    new DifficultyRange(1, 10)
                }
        };
        public static List<string> LevelOrder;
        public static List<string> PaintOrder;
}

    // 定义难度配置字典类型
    public class DifficultyConfig : Dictionary<string, List<DifficultyRange>>
    {
        
    }

    // 额外难度配置
    public class ExtraLevelDifficulty
    {
        public static readonly List<DifficultyRange> Ranges = new List<DifficultyRange>
        {
            new DifficultyRange(0, 0),
            new DifficultyRange(0.05, 4),
            new DifficultyRange(0.1, 4),
            new DifficultyRange(0.2, 5),
            new DifficultyRange(0.3, 6),
            new DifficultyRange(0.4, 7),
            new DifficultyRange(1, 10)
        };
    }

    public class DifficultyRange
    {
        public double Min { get; set; }
        public double Max { get; set; }

        public DifficultyRange(double min, double max)
        {
            Min = min;
            Max = max;
        }
    }
}
