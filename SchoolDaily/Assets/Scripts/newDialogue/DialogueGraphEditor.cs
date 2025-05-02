// using System.Collections;
// using System.Collections.Generic;
// using UnityEditor;
// using UnityEngine;
// using static SchoolD.NewDialogue.DialogueData;

// #if UNITY_EDITOR
// [CustomEditor(typeof(DialogueGraph))]
// public class DialogueGraphEditor : Editor
// {
//     private DialogueGraph graph;
//     private Vector2 scrollPos;

//     void OnEnable()
//     {
//         graph = (DialogueGraph)target;
//     }

//     public override void OnInspectorGUI()
//     {
//         // 绘制基础属性
//         EditorGUILayout.LabelField("Dialogue Graph", EditorStyles.boldLabel);
//         graph.graphName = EditorGUILayout.TextField("Graph Name", graph.graphName);

//         // 节点列表
//         scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
//         for (int i = 0; i < graph.nodes.Count; i++)
//         {
//             DrawNodeEditor(graph.nodes[i]);
//         }
//         EditorGUILayout.EndScrollView();

//         // 添加新节点按钮
//         if (GUILayout.Button("Add New Node"))
//         {
//             graph.nodes.Add(new DialogueNode());
//         }
//     }

//     void DrawNodeEditor(DialogueNode node)
//     {
//         EditorGUILayout.BeginVertical("box");

//         // 节点ID
//         node.nodeID = EditorGUILayout.TextField("Node ID", node.nodeID);

//         // 角色选择
//         node.character = (CharacterInfo)EditorGUILayout.ObjectField("Character", node.character, typeof(CharacterInfo), false);

//         // 多行文本输入
//         node.dialogueText = EditorGUILayout.TextArea(node.dialogueText, GUILayout.Height(60));

//         EditorGUILayout.EndVertical();
//     }
// }
// #endif
