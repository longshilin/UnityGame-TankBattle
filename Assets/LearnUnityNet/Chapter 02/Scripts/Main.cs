using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject humanPrefab; // 人物模型预设

    BaseHuman myHuman;
    Dictionary<string, BaseHuman> otherHumans; // 人物列表

    void Start()
    {
        // 网络模块
        NetManager.AddListene("Enter", OnEnter);
        NetManager.AddListene("Move", OnMove);
        NetManager.AddListene("Leave", OnLeave);
        NetManager.Connect("127.0.0.1", 14444);

        // 添加一个角色
        GameObject obj = Instantiate(humanPrefab);
        float x = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.desc = NetManager.GetDesc();

        // 发送协议
        Vector3 pos = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;
        string sendStr = "Enter|";
        sendStr += string.Format("{0},", NetManager.GetDesc());
        sendStr += string.Format("{0},", pos.x);
        sendStr += string.Format("{0},", pos.y);
        sendStr += string.Format("{0},", pos.z);
        sendStr += string.Format("{0},", eul.y);
        NetManager.Send(sendStr);
    }

    private void OnLeave(string msg)
    {
        Debug.Log("OnLeave" + msg);
    }

    private void OnMove(string msg)
    {
        Debug.Log("OnMove" + msg);
    }

    private void OnEnter(string msg)
    {
        Debug.Log("OnEnter" + msg);
    }

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();
    }
}