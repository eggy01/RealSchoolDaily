using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class npcExpressionOffset : MonoBehaviour
{
    public static npcExpressionOffset Instance { get; private set; }
    public Vector2[] expressionOffsets; // 每个NPC对应的表情偏移

    private void Awake()
    {
        Instance = this;
    }

    private int GetnpcIndex(string npcName)
    {
        int npcIndex = -1;
        switch (npcName)
        {
            case "弗洛":
                npcIndex = 0;
                break;
            case "林风":
                npcIndex = 1;
                break;

        }
        return npcIndex;
    }
    public void UpdateExpression(Image expressionImage, string npcName, Sprite expression)
    {
        if (npcName.Equals("???"))
        {
            if (expression.name.Contains("林风"))
                npcName = "林风";
        }

        expressionImage.sprite = expression;
        expressionImage.SetNativeSize();
        expressionImage.rectTransform.anchoredPosition = expressionOffsets[GetnpcIndex(npcName)];
    }

    public Sprite LoadEmotionSprite(string emotionName, string npcName)//加载表情图片
    {
        if (npcName.Equals(Settings.playerName))
            npcName = "主角";
        if (npcName.Equals("???"))
        {
            if (emotionName.Contains("林风"))
                npcName = "林风";
        }


        Sprite[] allEmotionSprites = Resources.LoadAll<Sprite>("Characters/" + npcName);

        if (allEmotionSprites == null || allEmotionSprites.Length == 0)
        {
            Debug.LogError("加载 Sprite Sheet 失败！请检查路径和资源是否存在。" + npcName);
        }
        // 2. 查找表情
        Sprite emotionSprite = System.Array.Find(allEmotionSprites, sprite => sprite.name == emotionName);
        return emotionSprite;
    }
}
