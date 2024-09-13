using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseHuman : MonoBehaviour
{
    protected Animator animator;

    protected NavMeshAgent navMeshAgent;

    protected Camera mainCam;
    private static readonly int Speed = Animator.StringToHash("Speed");

    public string desc;
    // Start is called before the first frame update
    protected void Start()
    {
        mainCam = Camera.main;
        navMeshAgent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    protected void Update()
    {
        if (navMeshAgent)
        {
            animator.SetFloat(Speed,navMeshAgent.speed);
        }
       
       
    }
}
