using Assets.Scripts.common;
using System.Collections.Generic;

namespace Assets.Game.Scripts
{
    public static class GameModeJson
    {
        public static readonly List<GameModeProperty> MODE_LIST = new List<GameModeProperty>
        {
            new GameModeProperty
            {
                id = GameMode.None,
                sceneName = "NormalMode",
                bundleName = "NormalMode",
                explain = "普通模式",
                videoCnt = 0
            },
            new GameModeProperty
            {
                id = GameMode.Feibiao,
                sceneName = "Game",
                bundleName = "Feibiao",
                explain = "毛线主玩法",
                videoCnt = 0
            },
            new GameModeProperty
            {
                id = GameMode.NewYear,
                sceneName = "Game",
                bundleName = "Feibiao",
                explain = "新年主题",
                videoCnt = 0
            },
            new GameModeProperty
            {
                id = GameMode.ShengXiao,
                sceneName = "Game",
                bundleName = "Feibiao",
                explain = "生肖主题",
                videoCnt = 0
            },
            new GameModeProperty
            {
                id = GameMode.NeZha,
                sceneName = "Game",
                bundleName = "Feibiao",
                explain = "哪吒主题",
                videoCnt = 0
            },
            new GameModeProperty
            {
                id = GameMode.Maoxian,
                sceneName = "mxpx",
                bundleName = "mxpx",
                explain = "毛线排序",
                videoCnt = 0,
                showInMore = true
            },
            new GameModeProperty
            {
                id = GameMode.ELSFK,
                sceneName = "elsfk",
                bundleName = "elsfk",
                explain = "俄罗斯方块",
                videoCnt = 0,
                showInMore = true
            },
            new GameModeProperty
            {
                id = GameMode.Brick,
                sceneName = "brick",
                bundleName = "brick",
                explain = "方块消除",
                videoCnt = 0,
                showInMore = true
            },
            new GameModeProperty
            {
                id = GameMode.NvShen,
                sceneName = "Game",
                bundleName = "Feibiao",
                explain = "女神主题",
                videoCnt = 0
            },
            new GameModeProperty
            {
                id = GameMode.Spring,
                sceneName = "Game",
                bundleName = "Feibiao",
                explain = "踏春主题",
                videoCnt = 0
            },
            new GameModeProperty
            {
                id = GameMode.RopeChanRao,
                sceneName = "RopeChao",
                bundleName = "RopeChanRao",
                explain = "绳子谜题",
                videoCnt = 0,
                showInMore = true
            },
            new GameModeProperty
            {
                id = GameMode.FiveOne,
                sceneName = "Game",
                bundleName = "Feibiao",
                explain = "五一主题",
                videoCnt = 0
            }
        };

        public static List<GameModeProperty> GetModeList()
        {
            return MODE_LIST;
        }

        public static GameModeProperty GetMode(GameMode mode)
        {
            return MODE_LIST.Find(x => x.id == mode);
        }
    }
}
