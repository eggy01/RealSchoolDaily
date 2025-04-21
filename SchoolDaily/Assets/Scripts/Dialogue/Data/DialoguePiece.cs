using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SchoolD.Dialogue
{
        [System.Serializable]
        public class DialoguePiece
        {
                [Header("对话详情")]
                public string belongToCSVFileName;
                public Sprite faceImage;//该条对话的人物图片
                public bool onLeft;//是否在左边，固定玩家在右，
                public string name;//该条对话的人物名称
                public string dialogueText;
                public bool hasToPause;//是否暂停
                public bool isDone;//该条对话是否结束
                public int index;//对话索引//用于跳转对话
                public int no;//一段对话的顺序

                public List<string> option;//选项列表

                public string emotion;//表情
                public int isfinalNotFirst = -1;//为最后一条
                                                //-1为正常，0为第一条，1为最后一条
                public string nextDialogueCSVFileName;//紧接下一条剧情文件名字

                public string nextIndex;//跳转到序号//用于一个csv文本内

                public int extra;//动画等额外信息    黑屏表示1

                public string activeDialogue;//激活剧情

                public string taskPID;//任务

                public string MoveToPosition;

                [Header("触发条件")]
                public string prerequisites; // 示例："Favorability.林风.>=.30;ItemOwned.门票.>=.1"
                public string award;//奖励

                /// <summary>
                /// 检查是否满足所有前置条件
                /// </summary>
                public bool IsConditionsMet()
                {
                        return ConditionSystem.CheckAll(prerequisites);
                }



                // 构造函数
                public DialoguePiece()
                {
                        option = new List<string>(); // 初始化选项列表
                }
        }
}

