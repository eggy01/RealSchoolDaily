using UnityEngine;
using System.Collections.Generic;
using SchoolD.NewDialogue;
using UnityEditor;
using static SchoolD.NewDialogue.DialogueData;

[CreateAssetMenu(fileName = "NewDialogueDatabase", menuName = "Dialogue System/Dialogue Database")]
public class DialogueDatabase : ScriptableObject
{
    [SerializeField]
    private List<DialogueGraph> _dialogueGraphs = new List<DialogueGraph>();

    /// <summary>
    /// 根据名称获取对话图表
    /// </summary>
    public DialogueGraph GetGraph(string graphName)
    {
        return _dialogueGraphs.Find(graph => graph.graphName == graphName);
    }

    /// <summary>
    /// 添加或更新对话图表
    /// </summary>
    public void SaveGraph(DialogueGraph graph)
    {
        var existing = _dialogueGraphs.FindIndex(g => g.graphName == graph.graphName);
        if (existing >= 0)
        {
            _dialogueGraphs[existing] = graph;
        }
        else
        {
            _dialogueGraphs.Add(graph);
        }
    }

    /// <summary>
    /// 获取所有对话图表名称（用于编辑器下拉菜单）
    /// </summary>
    public List<string> GetAllGraphNames()
    {
        return _dialogueGraphs.ConvertAll(graph => graph.graphName);
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(DialogueDatabase))]
    public class DialogueDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 显示基础属性
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_dialogueGraphs"), true);

            // 添加快速操作按钮
            if (GUILayout.Button("创建新对话图表"))
            {
                var newGraph = new DialogueGraph
                {
                    graphName = $"Chart_{target.GetInstanceID()}_{System.DateTime.Now.Ticks}"
                };
                ((DialogueDatabase)target).SaveGraph(newGraph);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
