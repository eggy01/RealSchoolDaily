using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SchoolD.NewDialogue.DialogueData;

namespace SchoolD.NewDialogue
{
    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        [Header("核心组件")]
        public DialogueDatabase database;
        public NewDialogueUI ui;

        [Header("设置")]
        public bool autoLoadFirstGraph = true;
        public string defaultGraphName = "Opening";

        // 运行时状态
        private DialogueGraph currentGraph;
        private DialogueNode currentNode;
        private Coroutine dialogueCoroutine;
        private HashSet<string> completedNodes = new HashSet<string>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            if (autoLoadFirstGraph)
            {
                LoadDialogueGraph(defaultGraphName);
            }
        }

        /// <summary>
        /// 加载对话图表
        /// </summary>
        public void LoadDialogueGraph(string graphName, bool resetProgress = false)
        {
            if (resetProgress)
            {
                completedNodes.Clear();
            }

            if (dialogueCoroutine != null)
            {
                StopCoroutine(dialogueCoroutine);
            }

            currentGraph = database.GetGraph(graphName);
            if (currentGraph == null)
            {
                Debug.LogError($"对话图表不存在: {graphName}");
                return;
            }

            currentNode = currentGraph.startNode;
            dialogueCoroutine = StartCoroutine(RunDialogueGraph());
        }

        /// <summary>
        /// 继续当前对话
        /// </summary>
        public void ContinueDialogue()
        {
            if (currentNode != null && currentNode.choices.Count == 0)
            {
                MoveToNextNode(currentNode.autoNextNode);
            }
        }

        /// <summary>
        /// 跳转到指定节点
        /// </summary>
        public void JumpToNode(string nodeID)
        {
            var node = currentGraph.GetNode(nodeID);
            if (node != null)
            {
                currentNode = node;
                if (dialogueCoroutine != null)
                {
                    StopCoroutine(dialogueCoroutine);
                }
                dialogueCoroutine = StartCoroutine(RunDialogueGraph());
            }
            else
            {
                Debug.LogError($"节点不存在: {nodeID}");
            }
        }

        private IEnumerator RunDialogueGraph()
        {
            while (currentNode != null)
            {
                // 标记节点为已完成
                if (currentNode.markAsCompleted)
                {
                    completedNodes.Add(currentNode.nodeID);
                }

                // 显示当前节点
                yield return StartCoroutine(ui.ShowDialogueNode(currentNode));

                // 执行节点效果（使用您提供的EffectExecutor）
                yield return ExecuteNodeEffects(currentNode);

                // 处理奖励
                ProcessRewards(currentNode);

                // 如果是带选项的节点，等待外部调用JumpToNode
                if (currentNode.choices.Count > 0)
                {
                    yield break;
                }

                // 自动继续到下一个节点
                MoveToNextNode(currentNode.autoNextNode);
            }

            OnDialogueEnd();
        }

        private void MoveToNextNode(string nextNodeID)
        {
            if (string.IsNullOrEmpty(nextNodeID))
            {
                currentNode = null;
                return;
            }

            currentNode = currentGraph.GetNode(nextNodeID);
            if (currentNode == null)
            {
                Debug.LogError($"下一个节点不存在: {nextNodeID}");
                currentNode = null;
            }
        }

        private IEnumerator ExecuteNodeEffects(DialogueNode node)
        {
            if (node.effects != null && node.effects.Count > 0)
            {
                // 直接使用您提供的EffectExecutor
                yield return DialogueEffectExecutor.Instance.ExecuteEffects(node.effects);
            }
        }

        private void ProcessRewards(DialogueNode node)
        {
            // 直接调用您的RewardManager处理奖励字符串
            if (!string.IsNullOrEmpty(node.rewardString))
            {
                RewardManager.Instance.ApplyRewards(node.rewardString);
            }

            // 同时处理物品奖励列表
            if (node.itemRewards != null && node.itemRewards.Count > 0)
            {
                foreach (var item in node.itemRewards)
                {
                    RewardManager.Instance.AddItem(item.itemID, item.amount);
                }
            }
        }

        private void OnDialogueEnd()
        {
            Debug.Log($"对话结束: {currentGraph?.graphName}");
            ui.Hide();
            currentGraph = null;
            currentNode = null;

            // 触发对话结束事件
            EventHandler.CallOnDialogueEnd(currentGraph?.graphName);
        }

        /// <summary>
        /// 检查节点是否已完成
        /// </summary>
        public bool IsNodeCompleted(string nodeID)
        {
            return completedNodes.Contains(nodeID);
        }

        /// <summary>
        /// 重置指定对话图的进度
        /// </summary>
        public void ResetProgress(string graphName)
        {
            if (currentGraph != null && currentGraph.graphName == graphName)
            {
                completedNodes.Clear();
            }
        }
    }

}