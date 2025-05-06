using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    // 玩家基础数据
    public PlayerData playerData = new PlayerData();
    // NPC动态数据
    public List<NPCLocalItem> npcLocalItems = new List<NPCLocalItem>();
    // 背包动态数据
    public List<PackageLocalItem> packageItems = new List<PackageLocalItem>();
    // 时间数据存储
    public GameTimeData gameTimeData = new GameTimeData();
}
