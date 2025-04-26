using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class player : MonoBehaviour
{
    public static player Instance;
    public GameObject myBag;
    public GameObject store;
    public float speed = 3;
    private Animator anim;
    private bool inputDisable;
    public bool isPaused;

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
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 初始化其他组件
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isPaused) // 只在非暂停状态下处理输入
        {
            HandleMovement();
        }
        OpenMybag();
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

    // 唯一的外部控制接口
    public void SetPause(bool pause)
    {
        isPaused = pause;

        // 处理动画
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

    public void TogglePause()
    {
        bool newState = !myBag.activeSelf;

        // 关闭商店
        if (newState && store.activeSelf)
        {
            ShopUI.Instance.CloseShop();
        }

        myBag.SetActive(newState);

        // 更新暂停状态
        bool shouldPause = myBag.activeSelf || store.activeSelf;
        PauseManager.Instance.SetPauseState(shouldPause);

        // 更新玩家暂停状态
        isPaused = shouldPause;
    }

    void OpenMybag()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            TogglePause();
        }
    }
}
