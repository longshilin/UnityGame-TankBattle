using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    protected bool isMoving = false; // 是否正在移动
    private Vector3 targetPosition;  // 移动目标点
    public float speed = 1.2f;       // 移动速度
    private Animator animator;       // 动画组件
    public string desc = "";         // 描述


    // 移动到某一处
    public void MoveTo(Vector3 pos)
    {
        targetPosition = pos;
        isMoving = true;
        animator.SetBool("isMoving", true);
    }

    // 移动Update
    public void MoveUpdate()
    {
        if (isMoving == false)
        {
            return;
        }

        Vector3 pos = transform.position;
        transform.position = Vector3.MoveTowards(pos, targetPosition, speed * Time.deltaTime);
        transform.LookAt(targetPosition);
        if (Vector3.Distance(pos, targetPosition) < 0.05f)
        {
            isMoving = false;
            animator.SetBool("isMoving", false);
        }
    }

    public void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        MoveUpdate();
    }
}