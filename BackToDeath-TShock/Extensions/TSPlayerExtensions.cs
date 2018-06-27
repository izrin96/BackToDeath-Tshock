﻿using TShockAPI;

namespace BackToDeath_TShock.Extensions
{
    public static class TSPlayerExtensions
    {
        public static PlayerInfo GetPlayerInfo(this TSPlayer tsplayer)
        {
            if (!tsplayer.ContainsData(PlayerInfo.KEY))
                tsplayer.SetData(PlayerInfo.KEY, new PlayerInfo());
            return tsplayer.GetData<PlayerInfo>(PlayerInfo.KEY);
        }
    }
}
