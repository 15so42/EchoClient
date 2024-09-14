using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Timer = System.Threading.Timer;

public class GameLoop : MonoBehaviour
{

    public GameObject humanPfb;
    [HideInInspector]
    public BaseHuman myHuman;
    public Dictionary<string, BaseHuman> otherHumans=new Dictionary<string, BaseHuman>();

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddListener("Enter",OnEnter);
        NetManager.AddListener("List",OnList);
        NetManager.AddListener("Move",OnMove);
        NetManager.AddListener("Attack",OnAttack);
        NetManager.AddListener("Leave",OnLeave);
        NetManager.Connect("127.0.0.1",8888);
        
        //声称自己的角色
        GameObject obj = (GameObject)Instantiate(humanPfb);
        float x = Random.Range(-5, 5);
        float z = Random.Range(-5, 5);
        obj.transform.position = new Vector3(x, 0, z);
        myHuman = obj.AddComponent<CtrlHuman>();
        myHuman.name = NetManager.GetDesc();
        
        //发送协议
        Vector3 pos = myHuman.transform.position;
        Vector3 eul = myHuman.transform.eulerAngles;
        string sendStr = "Enter|";
        sendStr += NetManager.GetDesc() + ",";
        sendStr += pos.x + ",";
        sendStr += pos.y + ",";
        sendStr += pos.z + ",";
        sendStr += eul.y;
        NetManager.Send(sendStr);
        
        //发送list协议请求玩家列表
        SendListRequest();

    }
    // 使用异步方法代替线程
    public async void SendListRequest()
    {
        await Task.Delay(1000);  // 等待1秒
        NetManager.Send("List|");  // 发送请求
    }
    
    
  

    // Update is called once per frame
    void Update()
    {
        NetManager.Update();   
    }

    void OnEnter(string msg)
    {
        Debug.Log("OnEnter："+msg);

        string[] spilt = msg.Split(",");
        string desc = spilt[0];

        float x = float.Parse(spilt[1]);
        float y = float.Parse(spilt[2]);
        float z = float.Parse(spilt[3]);
        float eulerY = float.Parse(spilt[4]);
        
        if(desc==NetManager.GetDesc())
            return;

        GameObject obj = (GameObject)Instantiate(humanPfb);
        obj.transform.position = new Vector3(x, y, z);
        obj.transform.eulerAngles = new Vector3(0, eulerY, 0);
        BaseHuman h = obj.AddComponent<SyncHuman>();
        h.desc = desc;
        otherHumans.Add(desc,h);
        
    }

    void OnList(string msg)
    {
        Debug.Log("OnList"+msg);

        string[] split = msg.Split(',');
        int count = (split.Length - 1) / 6;
        for (int i = 0; i < count; i++)
        {
            string desc = split[i * 6 + 0];
            float x = float.Parse(split[i * 6 + 1]);
            float y = float.Parse(split[i * 6 + 2]);
            float z = float.Parse(split[i * 6 + 3]);
            float eulerY = float.Parse(split[i * 6 + 4]);
            float hp = float.Parse(split[i * 6 + 5]);
            
            if(desc==NetManager.GetDesc())
                continue;

            GameObject obj = (GameObject)Instantiate(humanPfb);
            obj.transform.position = new Vector3(x, y, z);
            obj.transform.eulerAngles = new Vector3(0, eulerY, 0);
            BaseHuman h = obj.AddComponent<SyncHuman>();
            h.desc = desc;
            otherHumans.Add(desc,h);
        }
    }


    void OnMove(string msg)
    {
        Debug.Log("OnMove："+msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        float x = float.Parse(split[1]);
        float y = float.Parse(split[2]);
        float z = float.Parse(split[3]);

        if (!otherHumans.ContainsKey(desc))
        {
            return;
        }

        BaseHuman h = otherHumans[desc];
        Vector3 targetPos = new Vector3(x, y, z);
        h.MoveTo(targetPos);
    }

    void OnLeave(string msg)
    {
        Debug.Log("OnLeave："+msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        
        if(!otherHumans.ContainsKey(desc))
            return;

        BaseHuman h = otherHumans[desc];
        Destroy(h.gameObject);
        otherHumans.Remove(desc);
    }
    
    void OnAttack(string msg)
    {
        Debug.Log("OnAttack："+msg);
        string[] split = msg.Split(',');
        string desc = split[0];
        float eulerY = float.Parse(split[1]);
        
        if(!otherHumans.ContainsKey(desc))
            return;

        SyncHuman h = otherHumans[desc] as SyncHuman;
        h.SyncAttack(eulerY);
        
    }
}
