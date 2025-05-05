using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private Animator animator;
    private float horizontalInput;
    private float verticalInput;
    private bool isMoving;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 获取输入
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // 检查是否有移动输入
        isMoving = Mathf.Approximately(horizontalInput, 0.0f) && Mathf.Approximately(verticalInput, 0.0f) ? false : true;

        // 更新动画参数
        animator.SetBool("isWalking", isMoving);
        animator.SetFloat("horizontal", horizontalInput);
        animator.SetFloat("vertical", verticalInput);

        // 移动角色
        MoveCharacter();
    }

    void MoveCharacter()
    {
        Vector3 movement = new Vector3(horizontalInput, verticalInput);
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}