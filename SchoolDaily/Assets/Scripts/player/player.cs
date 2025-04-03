using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player : MonoBehaviour
{

    public float speed =3;//移动速度，3m/s
    private Animator anim;
    //用来设置参数

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");//得到左右按键
        //getaxisraw 只会返回1/0/-1；
        float y = Input.GetAxisRaw("Vertical");
        Vector2 direction = new Vector2(x,y);
        
        if(direction.magnitude>0)
        {
            anim.SetBool("isWalking",true);
            anim.SetFloat("horizontal",x);
            anim.SetFloat("vertical",y);
        }
        else
        {
            anim.SetBool("isWalking",false);
        }


        transform.Translate(direction*speed*Time.deltaTime);
        //transform 用于控制和访问该对象在三维空间中的位置、旋转和缩放。
    }
}
