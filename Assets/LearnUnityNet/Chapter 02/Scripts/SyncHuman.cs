using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncHuman : BaseHuman
{

    // 同步角色会转向
    public void SyncAttack(float eulY)
    {
        transform.eulerAngles = new Vector3(0, eulY, 0);
        Attack();
    }

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }
}