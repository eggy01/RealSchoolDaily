using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SchoolD.Dialogue
{
    [CreateAssetMenu(fileName = "DialogueData", menuName = "Dialogue/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        public List<DialoguePiece> dialoguePieces;
    }
}

