using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{
    public GameObject myBag;
    public float speed = 3;//移动速度，3m/s
    private Animator anim;

    private bool inputDisable;

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
        if (inputDisable == false)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            Vector2 direction = new Vector2(x, y);

            if (direction.magnitude > 0)
            {
                anim.SetBool("isWalking", true);
                
                if (x != 0) // 如果有水平输入
                {
                    anim.SetFloat("horizontal", x);
                    anim.SetFloat("vertical", 0); // 清除垂直动画参数
                }
                else // 仅垂直输入
                {
                    anim.SetFloat("horizontal", 0); // 清除水平动画参数
                    anim.SetFloat("vertical", y);
                }
            }
            else
            {
                anim.SetBool("isWalking", false);
            }

            transform.Translate(direction * speed * Time.deltaTime);
        }
        OpenMybag();
    }

    void OpenMybag(){
        if(Input.GetKeyDown(KeyCode.B))
        {
            myBag.SetActive(!myBag.activeInHierarchy);
        }
    }
}
