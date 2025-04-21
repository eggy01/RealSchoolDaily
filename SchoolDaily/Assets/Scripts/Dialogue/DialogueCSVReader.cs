
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

            if (int.TryParse(fields[1], out int no))
            {
                piece.no = no;
            }
            else
            {
                Debug.LogWarning($"无法解析索引: {fields[0]}");
            }

            //解析角色名字
            if (fields[2].Contains("(=pn)") || fields[2].Contains("主角"))
                fields[2] = fields[2].Replace("(=pn)", Settings.playerName);
            if (fields[2].Contains("主角"))
                fields[2] = fields[2].Replace("主角", Settings.playerName);

            piece.name = fields[2].Trim();


            if (spriteDict.TryGetValue(piece.name, out Sprite sprite))
            {
                piece.faceImage = sprite;
            }
            else if (piece.name == "???")
            {
                piece.faceImage = spriteDict["默认2"];
            }


            // 设置对话内容
            if (fields[4].Contains("(=pn)"))
                fields[4] = fields[4].Replace("(=pn)", Settings.playerName);
            piece.dialogueText = fields[4].Trim();

            // 设置位置（默认npc在左，主角在右）
            piece.onLeft = !fields[2].Trim().Contains(Settings.playerName);

            // 解析选项
            //fields[2].Trim().Contains(Settings.playerName) &&
            if (fields.Length > 6 && !fields[5].Equals(string.Empty))
            {
                piece.option.Clear(); // 清空现有选项

                if (!fields[5].Contains("|")) // 单选项
                {
                    if (fields[5].Contains("(=pn)"))
                        fields[5] = fields[5].Replace("(=pn)", Settings.playerName);

                    piece.option.Add(fields[5].Trim());
                }
                else // 多选项
                {
                    string[] options = fields[5].Split('|');
                    for (int i = 0; i < options.Length; i++)
                    {
                        if (options[i].Contains("(=pn)"))
                            options[i] = options[i].Replace("(=pn)", Settings.playerName);
                        piece.option.Add(options[i].Trim());
                    }
                }
            }

            // 解析表情
            if (fields.Length > 7 && !fields[6].Equals(string.Empty))
            {
                piece.emotion = fields[6].Trim();
            }

            // 解析操作，如，黑屏等
            if (fields.Length > 8 && !fields[7].Equals(string.Empty))
            {
                if (fields[7].Contains("动画:黑屏"))
                    piece.extra = 1;

                if (fields[7].Contains("移动:"))
                    if (fields[7].Contains("宿舍外"))
                        piece.MoveToPosition = "Life Scene";
            }

            //检测是否有下一条紧接着的对话
            if (fields.Length > 9 && !fields[8].Equals(string.Empty))
            {
                if (fields[8].Contains("|"))
                    piece.nextIndex = fields[8];
                else
                    piece.nextDialogueCSVFileName = fields[8];
            }

            //判断。
            if (fields.Length > 10 && !fields[9].Equals(string.Empty))
            {
                if (fields[9].Contains("first"))//该条为第一条
                    piece.isfinalNotFirst = 0;
                if (fields[9].Contains("final"))//该条为最后一条
                    piece.isfinalNotFirst = 1;
            }

            //激活剧情
            if (fields.Length > 11 && !fields[10].Equals(string.Empty))
            {
                StoryProgressManager.Instance.AddNewStory(fields[10], csvFile.name);
            }

            //任务
            if (fields.Length > 12 && !fields[11].Equals(string.Empty))
                piece.taskPID = fields[11];

            //前置条件
            if (fields.Length > 13 && !fields[12].Equals(string.Empty))
                piece.prerequisites = fields[12];

            //结算
            if (fields.Length > 14 && !fields[13].Equals(string.Empty))
                piece.award = fields[13];


            dialoguePieces.Add(piece);
        }

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

    public static TextAsset LoadCSVFromResources(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;

        TextAsset csv = Resources.Load<TextAsset>($"DialogueCSV/{fileName}");
        if (csv == null) Debug.LogError($"CSV文件加载失败: {fileName}");
        return csv;
    }
}