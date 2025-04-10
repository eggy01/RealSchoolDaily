
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SchoolD.Dialogue;
public class DialogueCSVReader : MonoBehaviour
{
    public static DialogueCSVReader Instance { get; private set; }

    // 角色头像集合，按名字索引
    public Sprite[] characterSprites;
    public Dictionary<string, Sprite> spriteDict = new Dictionary<string, Sprite>();


    private void Awake()
    {
        Instance = this;
        InitalSpriteDict();
    }

    // 初始化头像字典
    private void InitalSpriteDict()
    {
        foreach (var sprite in characterSprites)
        {
            if (sprite != null && !spriteDict.ContainsKey(sprite.name))
                spriteDict.Add(sprite.name, sprite);
        }
    }

    public List<DialoguePiece> LoadDialogueData(TextAsset csvFile)
    {
        List<DialoguePiece> dialoguePieces = new List<DialoguePiece>();

        if (csvFile == null)
        {
            Debug.LogError("CSV文件未分配!");
            return dialoguePieces;
        }

        string[] lines = csvFile.text.Split('\n');//分割文件

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;//如果该行为空或者空格

            // 分割行，考虑逗号分隔但内容中可能包含逗号的情况
            string[] fields = Instance.ParseCsvLine(line);

            if (fields.Length < 4) continue; // 确保有足够字段

            DialoguePiece piece = new DialoguePiece();

            if (int.TryParse(fields[0], out int index))
            {
                piece.index = index;
            }
            else
            {
                Debug.LogWarning($"无法解析索引: {fields[0]}");
            }

            // 解析角色名字
            piece.name = fields[1].Trim();

            // 在头像匹配部分添加调试信息：
            if (spriteDict.TryGetValue(piece.name, out Sprite sprite))
            {
                piece.faceImage = sprite;
            }
            else if (piece.name == "???")
            {
                piece.faceImage = spriteDict["默认"];
            }
            // 设置对话内容
            piece.dialogueText = fields[3].Trim();

            // 设置位置（默认npc在左，主角在右）
            piece.onLeft = !fields[1].Trim().Contains("主角");

            // 解析表情和暂停标记（如果有）
            if (fields.Length > 4)
            {
                string extraData = fields[4].Trim();
                // 如果包含"（"则可能有表情说明
                if (extraData.Contains("（"))
                {
                    // 可以在这里处理表情标记，如"困惑"、"尴尬"等
                }

                // 内心独白或需要暂停的对话
                if (extraData.Contains("内心独白") || extraData.Contains("..."))
                {
                    piece.hasToPause = true;
                }
            }

            dialoguePieces.Add(piece);
        }

        Debug.Log($"成功加载{dialoguePieces.Count}条对话数据");
        return dialoguePieces;
    }

    // 处理CSV行，考虑内容中包含逗号的情况
    private string[] ParseCsvLine(string line)
    {
        List<string> fields = new List<string>();
        bool inQuotes = false;
        int startIndex = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                string field = line.Substring(startIndex, i - startIndex).Trim();
                field = field.Trim('"'); // 移除可能的引号
                fields.Add(field);
                startIndex = i + 1;
            }
        }

        // 添加最后一个字段
        string lastField = line.Substring(startIndex).Trim();
        lastField = lastField.Trim('"');
        fields.Add(lastField);

        return fields.ToArray();
    }
}