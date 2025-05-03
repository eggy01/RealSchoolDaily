using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageListItem : MonoBehaviour
{
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI messageText;
    public GameObject unreadIndicator;

    public void Initialize(string name, string message, bool isUnread, System.Action onClick)
    {
        nameText.text = name;
        messageText.text = message;
        unreadIndicator.SetActive(isUnread);
        Debug.Log("设置内容" + message);

        GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetAvatar(Sprite avatar)
    {
        avatarImage.sprite = avatar;
    }
}