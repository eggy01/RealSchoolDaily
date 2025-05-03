using UnityEngine;
using UnityEngine.UI;

public class RadarChart : Graphic
{
    [SerializeField] private float maxValue = 100;
    [SerializeField] private float size = 200;
    [SerializeField] private PlayerInformation playerInfo;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 center = rectTransform.rect.center;
        float angleStep = 360f / 6;

        // 获取并限制数值范围
        float[] values = new float[6]
        {
            Mathf.Clamp(playerInfo.playerData.fame, 0, maxValue),         // 声望下限0
            Mathf.Clamp(playerInfo.playerData.morality, -Mathf.Infinity, maxValue), // 道德无下限
            Mathf.Clamp(playerInfo.playerData.intelligence, 0, maxValue),
            Mathf.Clamp(playerInfo.playerData.comprehension, 0, maxValue),
            Mathf.Clamp(playerInfo.playerData.talent, 0, maxValue),
            Mathf.Clamp(playerInfo.playerData.society, 0, maxValue)
        };

        // 添加中心点（索引6）
        vh.AddVert(center, color, Vector2.zero);

        // 绘制顶点
        for (int i = 0; i < 6; i++)
        {
            float angle = 120 - angleStep * i; // 调整起始角度
            float clampedValue = Mathf.Max(values[i], 0); // 负值强制为0
            float radius = size * (clampedValue / maxValue);

            Vector2 pos = center + new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;

            vh.AddVert(pos, color, Vector2.zero);
        }

        // 连接三角形（注意顶点索引从1开始）
        for (int i = 0; i < 6; i++)
        {
            int current = i + 1; // 外围顶点从1开始
            int next = (i + 1) % 6 + 1;
            vh.AddTriangle(current, next, 0); // 0是中心点
        }
    }

    public void RefreshChart()
    {
        SetVerticesDirty();
    }
}