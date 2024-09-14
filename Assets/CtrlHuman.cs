
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CtrlHuman : BaseHuman
{
    private float lastAttackTime = 0;
    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Terrain"))
                {
                    navMeshAgent.SetDestination(hit.point);
                    
                    //NetManager.Send("Enter|127.1.1.1,100,200,300,45");
                    
                    //发送Move协议
                    string sendStr = "Move|";
                    sendStr += NetManager.GetDesc() + ",";
                    sendStr += hit.point.x + ",";
                    sendStr += hit.point.y + ",";
                    sendStr += hit.point.z + ",";
                    NetManager.Send(sendStr);
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time > lastAttackTime + 1)
            {
                lastAttackTime = Time.time;
                StartCoroutine(AttackLogic());
            }
        }
    }

    IEnumerator AttackLogic()
    {
        animator.SetTrigger("Attack");
        //发送协议进行动画同步
        string sendStr = "Attack|";
        sendStr += NetManager.GetDesc() + ",";
        sendStr += transform.eulerAngles.y + ",";
        NetManager.Send(sendStr);
        
        yield return new WaitForSeconds(0.3f);
        var cols= Physics.OverlapSphere(transform.TransformPoint(0, 1, 1), 1);
        foreach (var col in cols)
        {
            if(col.gameObject==gameObject)
                continue;
            if (col.GetComponent<SyncHuman>())
            {
                
            }
        }
    }
   
}
