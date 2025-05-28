using Assets.Scripts.manager;
using Spine;


namespace Assets.Game.Scripts.modes
{
    public class LocalFbData
    {
        public int level;
        public int selectedPainting;
    }

    public class LocalFdjData
    {
        public int cishu;
    }
    public class GameData:SingletonBase<GameData>
    {


        /**
         * 飞镖主玩法数据
         */
        public LocalFbData GetFeibiaoData()
        {
            LocalFbData localFbData = new LocalFbData();
            localFbData.level = 0;
            localFbData.selectedPainting = 0;
            return localFbData;
        }

        public void SetFeibiaoData(LocalFbData data)
        {

        }

        public LocalFdjData GetMianfeidaojuData()
        {
            LocalFdjData data = new LocalFdjData();
            data.cishu = 1;
            return data;
        }

        public void SetMianfeidaojuData(LocalFdjData data)
        {

        }
    }
}
