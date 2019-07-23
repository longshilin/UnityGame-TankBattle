using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    public GameObject humanPrefab; // 人物模型预设

    private BaseHuman myHuman;                                                               // 玩家自己
    private Dictionary<string, BaseHuman> otherHumans = new Dictionary<string, BaseHuman>(); // 其他玩家列表

    private void Start()
    {
        // 网络模块
        NetManager.AddListene("Enter", OnEnter);
        NetManager.AddListene("List", OnList);
        NetManager.AddListene("Move", OnMove);
        NetManager.AddListene("Attack", OnAttack);
        NetManager.AddListene("Leave", OnLeave);
        NetManager.AddListene("Die", OnDie);
        NetManager.Connect("127.0.0.1", 14444);

        // 添加玩家自身角色，并通知给服务器
        var obj = Instantiate(humanPrefab);
        float x = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.desc = NetManager.GetDesc();

        // 添加角色发送Enter协议请求，后进入房间的通知之前的玩家生成新玩家角色
        var pos = myHuman.transform.position;
        var eul = myHuman.transform.eulerAngles;
        var sendStr = "Enter|";
        sendStr += string.Format("{0},", NetManager.GetDesc());
        sendStr += string.Format("{0},", pos.x);
        sendStr += string.Format("{0},", pos.y);
        sendStr += string.Format("{0},", pos.z);
        sendStr += string.Format("{0}", eul.y);
        NetManager.Send(sendStr);
        Debug.Log("Client send: " + sendStr);

        // 初始化游戏时请求玩家列表发送List协议请求，后进入房间的生成房间中老玩家角色
        NetManager.Send("List|");
        Debug.Log("Client send: " + "List|");
    }

    private void OnDie(string msg)
    {
        Debug.Log("Receive Die: " + msg);

        // 解析参数
        string[] split = msg.Split(',');
        string hitDesc = split[0];

        // 自己死了
        if (hitDesc == myHuman.desc)
        {
            Debug.Log("Game Over");
            myHuman.gameObject.SetActive(false);
            return;
        }

        if (!otherHumans.ContainsKey(hitDesc)) return;

        // 敌人死了
        SyncHuman h = (SyncHuman) otherHumans[hitDesc];
        h.gameObject.SetActive(false);
    }

    private void OnAttack(string msg)
    {
        Debug.Log("Receive Attack: " + msg);

        // 解析参数
        string[] split = msg.Split(',');
        string desc = split[0];
        float eulY = float.Parse(split[1]);

        // 攻击动作
        if (!otherHumans.ContainsKey(desc)) return;
        SyncHuman h = (SyncHuman) otherHumans[desc];
        h.SyncAttack(eulY);
    }


    /// <summary>
    /// 老玩家客户端生成新进入玩家角色
    /// </summary>
    private void OnEnter(string msg)
    {
        Debug.Log("Receive Enter: " + msg);

        // 解析参数
        var split = msg.Split(',');
        var desc = split[0];
        var x = float.Parse(split[1]);
        var y = float.Parse(split[2]);
        var z = float.Parse(split[3]);
        var eulY = float.Parse(split[4]);

        // 是自己则直接返回
        if (desc == NetManager.GetDesc())
        {
            return;
        }

        // 添加一个新角色
        var obj = Instantiate(humanPrefab);
        obj.transform.position = new Vector3(x, y, z);
        obj.transform.eulerAngles = new Vector3(0, eulY, 0);
        BaseHuman h = obj.AddComponent<SyncHuman>();
        h.desc = desc;
        otherHumans.Add(desc, h);
    }

    /// <summary>
    /// 按照服务器返回的玩家列表创建所有玩家
    /// </summary>
    private void OnList(string msg)
    {
        Debug.Log("Receive List: " + msg);
        /// 解析参数
        var split = msg.Split(',');

        var count = (split.Length - 1) / 6; // 服务器返回的角色数量
        for (var i = 0; i < count; i++)
        {
            var desc = split[i * 6 + 0];
            var x = float.Parse(split[i * 6 + 1]);
            var y = float.Parse(split[i * 6 + 2]);
            var z = float.Parse(split[i * 6 + 3]);
            var eulY = float.Parse(split[i * 6 + 4]);
            var hp = int.Parse(split[i * 6 + 5]);

            // 是自己就无需再创建
            if (desc == NetManager.GetDesc())
            {
                continue;
            }

            // 添加一个角色
            var obj = Instantiate(humanPrefab);
            obj.transform.position = new Vector3(x, y, z);
            obj.transform.eulerAngles = new Vector3(0, eulY, 0);
            BaseHuman h = obj.AddComponent<SyncHuman>();
            h.desc = desc;
            otherHumans.Add(desc, h);
        }
    }

    /// <summary>
    /// 删除对应的同步角色，同时把它从同步角色列表OtherHuman中删除
    /// </summary>
    private void OnLeave(string msg)
    {
        Debug.Log("Receive Leave: " + msg);
        // 解析参数
        string[] split = msg.Split(',');
        string desc = split[0];
        // 删除角色
        if (!otherHumans.ContainsKey(desc)) return;
        BaseHuman h = otherHumans[desc];
        Destroy(h.gameObject);
        otherHumans.Remove(desc);
    }

    /// <summary>
    /// 解析服务器发来的OnMove协议，找到对应的同步角色，调用MoveTo方法同步角色位置
    /// </summary>
    /// <param name="msg"></param>
    private void OnMove(string msg)
    {
        Debug.Log("Receive Move: " + msg);
        // 解析参数
        string[] split = msg.Split(',');
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);

        if (!otherHumans.ContainsKey(desc)) return;
        // TODO 只通过网络同步其他玩家的位置，自己的位置是在本地同步之后，在发送给服务器的，这种做法需要优化

        BaseHuman h = otherHumans[desc];
        Vector3 targetPos = new Vector3(x, y, z);
        h.MoveTo(targetPos);
    }

    // Update is called once per frame
    private void Update()
    {
        NetManager.Update();
    }
}