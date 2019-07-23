using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseHuman : MonoBehaviour
{
    protected bool isMoving = false;            // 是否正在移动
    private Vector3 targetPosition;             // 移动目标点
    public float speed = 1.2f;                  // 移动速度
    private Animator animator;                  // 动画组件
    public string desc = "";                    // 描述
    internal bool isAttacking = false;          // 是否正在攻击
    internal float attackTime = float.MinValue; // 攻击持续时间

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

    // 攻击动作
    public void Attack()
    {
        isAttacking = true;
        attackTime = Time.time;
        animator.SetBool("isAttacking", true);
    }

    // 攻击Update
    public void AttackUpdate()
    {
        if (!isAttacking) return;
        if (Time.time - attackTime < 1.2f) return;
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    public void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        MoveUpdate();
        AttackUpdate();
    }
}