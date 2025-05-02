// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using SchoolD.NewDialogue.DialogueData;
// using static SchoolD.NewDialogue.DialogueData;

// #if UNITY_EDITOR
// [CustomEditor(typeof(DialogueGraph))]
// public class DialogueGraphEditor : Editor
// {
//   private DialogueGraph graph;
//   private Vector2 scrollPos;
//   private bool showNodes = true; // 新增：折叠开关

//   private void OnEnable()
//   {
//     // 安全类型转换（改用 as 避免异常）
//     graph = target as DialogueGraph;
//     if (graph == null)
//     {
//       Debug.LogError("当前对象不是 DialogueGraph 类型！");
//       return;
//     }

//     // 初始化节点列表（防止空引用）
//     if (graph.nodes == null)
//     {
//       graph.nodes = new List<DialogueNode>();
//     }
//   }

//   public override void OnInspectorGUI()
//   {
//     if (graph == null) return;

//     // 启用撤销记录
//     serializedObject.Update();

//     // 显示不可编辑的脚本引用
//     EditorGUI.BeginDisabledGroup(true);
//     EditorGUILayout.ObjectField("脚本", MonoScript.FromScriptableObject(graph), typeof(DialogueGraph), false);
//     EditorGUI.EndDisabledGroup();

//     // 图表基础设置
//     EditorGUILayout.Space();
//     EditorGUILayout.LabelField("对话图表设置", EditorStyles.boldLabel);
//     graph.graphName = EditorGUILayout.TextField("图表名称", graph.graphName);

//     // 节点折叠区域
//     EditorGUILayout.Space();
//     showNodes = EditorGUILayout.Foldout(showNodes, $"对话节点 ({graph.nodes.Count})", true);
//     if (showNodes)
//     {
//       scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
//       for (int i = 0; i < graph.nodes.Count; i++)
//       {
//         DrawNodeEditor(graph.nodes[i], i);
//       }
//       EditorGUILayout.EndScrollView();

//       // 节点管理按钮
//       EditorGUILayout.BeginHorizontal();
//       if (GUILayout.Button("添加节点"))
//       {
//         Undo.RecordObject(graph, "添加对话节点");
//         graph.nodes.Add(new DialogueNode()
//         {
//           nodeID = "node_" + System.Guid.NewGuid().ToString("N").Substring(0, 6), // 生成唯一ID
//           dialogueText = "输入对话内容..."
//         });
//         EditorUtility.SetDirty(graph);
//       }

//       if (GUILayout.Button("清空所有") && graph.nodes.Count > 0)
//       {
//         if (EditorUtility.DisplayDialog("警告", "确定要删除所有节点吗？", "确定", "取消"))
//         {
//           Undo.RecordObject(graph, "清空节点");
//           graph.nodes.Clear();
//           EditorUtility.SetDirty(graph);
//         }
//       }
//       EditorGUILayout.EndHorizontal();
//     }

//     serializedObject.ApplyModifiedProperties();
//   }

//   private void DrawNodeEditor(DialogueNode node, int index)
//   {
//     EditorGUILayout.BeginVertical("Box");

//     // 节点标题栏（带删除按钮）
//     EditorGUILayout.BeginHorizontal();
//     EditorGUILayout.LabelField($"节点 {index + 1}", EditorStyles.boldLabel);

//     // 删除按钮
//     if (GUILayout.Button("×", GUILayout.Width(20)))
//     {
//       Undo.RecordObject(graph, "删除节点");
//       graph.nodes.RemoveAt(index);
//       EditorUtility.SetDirty(graph);
//       return; // 立即退出防止后续UI报错
//     }
//     EditorGUILayout.EndHorizontal();

//     // 节点属性
//     node.nodeID = EditorGUILayout.TextField("节点ID", node.nodeID);
//     node.character = (CharacterInfo)EditorGUILayout.ObjectField("角色", node.character, typeof(CharacterInfo), false);

//     // 多行文本区域
//     EditorGUILayout.LabelField("对话文本");
//     node.dialogueText = EditorGUILayout.TextArea(node.dialogueText, GUILayout.MinHeight(80));

//     EditorGUILayout.EndVertical();
//   }
// }
// #endif
