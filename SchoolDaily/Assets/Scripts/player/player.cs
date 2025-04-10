using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{

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
        if (inputDisable == false)//玩家不能移动
        {
            float x = Input.GetAxisRaw("Horizontal");//得到左右按键
                                                     //getaxisraw 只会返回1/0/-1；
            float y = Input.GetAxisRaw("Vertical");
            Vector2 direction = new Vector2(x, y);

            if (direction.magnitude > 0)
            {
                anim.SetBool("isWalking", true);
                anim.SetFloat("horizontal", x);
                anim.SetFloat("vertical", y);
            }
            else
            {
                anim.SetBool("isWalking", false);
            }


            transform.Translate(direction * speed * Time.deltaTime);
            //transform 用于控制和访问该对象在三维空间中的位置、旋转和缩放。
        }

    }

}
