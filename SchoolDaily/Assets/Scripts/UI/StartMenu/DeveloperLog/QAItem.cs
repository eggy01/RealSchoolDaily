using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QAItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI answerText;

    public void Initialize(string question, string answer)
    {
        questionText.text = $"Q: {question}";
        answerText.text = $"A: {answer}";
    }
}