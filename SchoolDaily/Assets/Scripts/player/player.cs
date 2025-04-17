using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public GameObject myBag;
    public float speed = 3;
    private Animator anim;
    private bool inputDisable;
    private bool isPaused;

    private void OnEnable()
    {
        EventHandler.BeforeScenUnLoadEvent += OnBeforeSceneUnLoadEvent;
        EventHandler.AfterScenLoadEvent += OnAfterSceneLoadEvent;
        EventHandler.MoveToPositionEvent += OnMoveToPositionEvent;

    }
    private void OnDisable()
    {
        EventHandler.BeforeScenUnLoadEvent -= OnBeforeSceneUnLoadEvent;
        EventHandler.AfterScenLoadEvent -= OnAfterSceneLoadEvent;
        EventHandler.MoveToPositionEvent -= OnMoveToPositionEvent;

    }

    private void OnBeforeSceneUnLoadEvent()
    {
        inputDisable = true;
    }

    private void OnAfterSceneLoadEvent()
    {
        inputDisable = false;
    }

    private void OnMoveToPositionEvent(Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }

    //用来设置参数

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isPaused) // 只在非暂停状态下处理输入
        {
            HandleMovement();
        }
        OpenMybag();
        Store();
    }

    void HandleMovement()
    {
        if (inputDisable) return;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector2 direction = new Vector2(x, y);

        UpdateAnimation(direction);
        transform.Translate(direction * speed * Time.deltaTime);
    }

    void UpdateAnimation(Vector2 direction)
    {
        bool isWalking = direction.magnitude > 0;
        anim.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            anim.SetFloat("horizontal", direction.x);
            anim.SetFloat("vertical", direction.y);
        }
    }

    private void Store()
    {
        // 当玩家在范围内且按下E键时
        if (Input.GetKeyDown(KeyCode.T))
        {
            ShopUI.Instance.ShowShop();
            Debug.Log("T");
        }
    }
    void OpenMybag()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        myBag.SetActive(isPaused); // 确保状态同步
        Time.timeScale = isPaused ? 0 : 1;
    }
}
