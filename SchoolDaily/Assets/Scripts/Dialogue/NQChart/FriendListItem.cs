using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FriendListItem : MonoBehaviour
{
    public Image avatarImage;
    public TextMeshProUGUI nameText;
    public GameObject unreadIndicator;

    public void Initialize(string name, bool isUnread, System.Action onClick)
    {
        nameText.text = name;
        unreadIndicator.SetActive(isUnread);

        GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetAvatar(Sprite avatar)
    {
        avatarImage.sprite = avatar;
    }
}