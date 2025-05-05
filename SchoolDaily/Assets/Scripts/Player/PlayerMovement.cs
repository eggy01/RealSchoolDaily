using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;
    [SerializeField]
    private float speed = 15;
    [SerializeField]
    private AudioClip walkAudio;
    private Animator anim;
    private bool inputDisable;
    public bool IsPaused { get; private set; }

    private void Awake()
    {
        Instance = this;
        anim = GetComponent<Animator>();
    }
    void OnEnable()
    {
        EventHandler.MoveToPositionEvent += moveToPosition;
    }
    void OnDisable()
    {
        EventHandler.MoveToPositionEvent -= moveToPosition;
    }
    void moveToPosition(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    public void HandleMovement()
    {
        if (inputDisable || IsPaused) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 direction = new Vector2(x, y);

        UpdateAnimation(direction);
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void UpdateAnimation(Vector2 direction)
    {
        bool isWalking = direction.magnitude > 0;
        anim.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            //AllAudioManager.Instance.PlaySFX("Footstep", "DirtStep", loop: true);
            anim.SetFloat("horizontal", direction.x);
            anim.SetFloat("vertical", direction.y);
        }
        AllAudioManager.Instance.StopCategory("Footstep", fadeOut: true);
    }

    public void SetPause(bool pause)
    {
        IsPaused = pause;
        if (pause)
        {
            anim.SetBool("isWalking", false);
            anim.SetFloat("horizontal", 0);
            anim.SetFloat("vertical", 0);
            anim.enabled = false;
        }
        else
        {
            anim.enabled = true;
        }
    }

    public void SetInputDisable(bool state)
    {
        inputDisable = state;
    }
}