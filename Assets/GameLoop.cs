using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameLoop : MonoBehaviour
{

    public GameObject humanPfb;
    [HideInInspector]
    public BaseHuman myHuman;
    public Dictionary<string, BaseHuman> otherHumans;

    // Start is called before the first frame update
    void Start()
    {
        NetManager.AddListener("Enter",OnEnter);
        NetManager.AddListener("List",OnList);
        NetManager.AddListener("Move",OnMove);
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
        NetManager.Send("List|");
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
    }

    void OnLeave(string msg)
    {
        Debug.Log("OnLeave："+msg);
    }
}
