using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SchoolD.Dialogue
{
    [System.Serializable]
    public class DialoguePiece
    {
        [Header("对话详情")]
        public Sprite faceImage;//该条对话的人物图片
        public bool onLeft;//是否在左边，固定玩家在右，
        public string name;//该条对话的人物名称
        public string dialogueText;
        public bool hasToPause;//是否暂停
        public bool isDone;//该条对话是否结束
        public int index;//对话序号
        public int no;//对话顺序
    }
}

