using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncHuman : BaseHuman
{

    public void SyncAttack(float eulerY)
    {
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, eulerY, transform.eulerAngles.z);
        animator.SetTrigger("Attack");
    }
}
