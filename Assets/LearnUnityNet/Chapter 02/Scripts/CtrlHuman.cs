using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    string myDesc = NetManager.GetDesc();

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            if (hit.collider.tag == "Terrain" && myDesc == desc)
            {
                MoveTo(hit.point);
                // 发送协议
                string sendStr = "Move|";
                sendStr += string.Format("{0},", myDesc);
                sendStr += string.Format("{0},", hit.point.x);
                sendStr += string.Format("{0},", hit.point.y);
                sendStr += string.Format("{0},", hit.point.z);
                NetManager.Send(sendStr);
            }
        }

        // 攻击 
        if (Input.GetMouseButtonDown(1))
        {
            if (isAttacking) return;
            if (isMoving) return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            if (hit.collider.tag == "Terrain" && myDesc == desc)
            {
                transform.LookAt(hit.point);
                Attack();

                // 发送协议
                string sendStr = "Attack|";
                sendStr += string.Format("{0},", NetManager.GetDesc());
                sendStr += string.Format("{0},", transform.eulerAngles.y);
                NetManager.Send(sendStr);

                // 攻击判定
                Vector3 lineEnd = transform.position + 0.5f * Vector3.up;
                Vector3 lineStart = lineEnd + 20 * transform.forward;
                if (Physics.Linecast(lineStart, lineEnd, out hit))
                {
                    GameObject hitObj = hit.collider.gameObject;
                    if (hitObj == gameObject)
                    {
                        return;
                    }

                    SyncHuman h = hitObj.GetComponent<SyncHuman>();
                    if (h == null)
                    {
                        return;
                    }

                    sendStr = "Hit|";
                    sendStr += string.Format("{0},", NetManager.GetDesc());
                    sendStr += string.Format("{0},", h.desc);
                    NetManager.Send(sendStr);
                }
            }
        }
    }
}