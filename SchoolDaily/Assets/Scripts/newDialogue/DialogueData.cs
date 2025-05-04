using System.Collections.Generic;
using UnityEngine;

namespace SchoolD.NewDialogue
{
    public class DialogueData : MonoBehaviour
    {
        [System.Serializable]
        public class DialogueGraph
        {
            public string graphName;
            public DialogueNode startNode;
            public List<DialogueNode> nodes = new List<DialogueNode>();

            public DialogueNode GetNode(string nodeID) => nodes.Find(n => n.nodeID == nodeID);
        }

        [System.Serializable]
        public class DialogueNode
        {
            [Header("基础设置")]
            public string nodeID;
            public CharacterInfo character;
            [TextArea(3, 5)] public string dialogueText;
            public Expression expression;

            [Header("流程控制")]
            public string autoNextNode;
            public bool markAsCompleted = true;

            [Header("交互选项")]
            public List<DialogueChoice> choices = new List<DialogueChoice>();

            [Header("效果控制")]
            public List<DialogueEffect> effects = new List<DialogueEffect>();

            [Header("奖励设置")]
            public string rewardString; // 兼容您原有的奖励字符串格式
            public List<ItemReward> itemRewards = new List<ItemReward>(); // 结构化物品奖励


        }

        [System.Serializable]
        public class ItemReward
        {
            public string itemID;
            public int amount = 1;
        }

        [System.Serializable]
        public class DialogueChoice
        {
            [Tooltip("选项显示文本")]
            public string choiceText;

            [Tooltip("跳转的目标节点ID")]
            public string nextNode;

            [Tooltip("显示条件")]
            public List<Condition> conditions = new List<Condition>();

            [Tooltip("自定义按钮预制体（可选）")]
            public GameObject customButtonPrefab;
        }

        [System.Serializable]
        public class CharacterInfo
        {
            [Header("基础信息")]
            public string characterID;
            public string displayName;
            public bool isPlayer = false; // 玩家角色固定右侧

            [Header("视觉设置")]
            public Sprite defaultPortrait;
            public Vector2 portraitOffset = Vector2.zero;
            public Color nameColor = Color.white;
            public Expression defaultExpression;

            [Header("音频设置")]
            public AudioClip voiceProfile; // 角色默认语音
        }

        [System.Serializable]
        public class Expression
        {
            public string expressionID;
            public Sprite sprite;
            public Vector2 offset;
            public AudioClip soundEffect;

            [Range(0.1f, 2f)]
            public float duration = 0.5f;
        }

        // [System.Serializable]
        // public class DialogueEffect
        // {
        //     public EffectType type;

        //     [Header("通用参数")]
        //     public string targetParameter;
        //     public float duration = 0.5f;

        //     [Header("特殊参数")]
        //     public string timeValue; // 用于TimeSkip
        //     public string positionName; // 用于PlayerAutoMoveto
        //     public AudioClip soundClip; // 用于PlaySound
        // }
        public class DialogueEffect
        {
            public EffectType type;
            public string parameters; // 可以存储JSON或特定格式字符串
            public float duration = 0.5f; // 默认持续时间
                                          //public string showText = "";
        }
        public enum EffectType
        {
            None,
            BlackScreen,//黑屏
            TimeSkip,//时间跳转
            SceneTransition,//场景调整
            ShowText, // 新增文本显示类型
            PlaySound,// 新增声音播放类型
            MultipleEffects, // 用于组合多个效果
            PlayerAutoMoveto,
            RandomEvent, // 随机事件类型
            AddNewChat // 新增聊天效果类型
        }

        [System.Serializable]
        public class DialogueReward
        {
            public RewardType type;
            public string itemID;
            public int amount = 1;
            public string questID;
        }

        public enum RewardType
        {
            Item,
            Money,
            QuestProgress,
            UnlockAchievement
        }

        // 条件系统（与之前保持兼容）
        [System.Serializable]
        public class Condition
        {
            public string conditionString; // 原始条件字符串如"Favorability.林风.>=.30"

            public bool IsMet() => ConditionSystem.Check(conditionString);
        }

        // #if UNITY_EDITOR
        //     // 编辑器增强
        //     [UnityEditor.CustomPropertyDrawer(typeof(DialogueEffect))]
        //     public class DialogueEffectDrawer : UnityEditor.PropertyDrawer
        //     {
        //         public override void OnGUI(Rect position, UnityEditor.SerializedProperty property, GUIContent label)
        //         {
        //             // 实现自定义编辑器绘制...
        //         }
        //     }
        // #endif
    }
}