using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSpike : MonoBehaviour
{
    public enum MoveType
    {
        Horizontal, 
        Vertical   
    }

    [Header("外观设置")]
    public float RotateSpeed = 300f;  

    [Header("移动设置")]
    public MoveType Type = MoveType.Horizontal; 
    public float MoveDistance = 5f;   
    public float MoveSpeed = 3f;    
    public float WaitTime = 0.5f;   

    [Header("伤害设置")]
    public int Damage = 20;           

    private Vector3 startPos;        
    private Vector3 endPos;           
    private Vector3 targetPos;         
    private bool isWaiting;    

    private void Start()
    {
        startPos = transform.position;

        if (Type == MoveType.Horizontal)
        {
            endPos = startPos + Vector3.right * MoveDistance; 
        }
        else
        {
            endPos = startPos + Vector3.up * MoveDistance;   
        }

        targetPos = endPos;
    }

    private void Update()
    {
        transform.Rotate(0, 0, RotateSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (isWaiting) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, MoveSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            StartCoroutine(WaitAndSwitch());
        }
    }

    IEnumerator WaitAndSwitch()
    {
        isWaiting = true;
        yield return new WaitForSeconds(WaitTime);

        if (targetPos == endPos)
            targetPos = startPos;
        else
            targetPos = endPos;

        isWaiting = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IHurt hurt = other.GetComponent<IHurt>();
            if (hurt != null)
            {
                hurt.Hurt(transform, Damage);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red; 
            Vector3 dest = transform.position;

            if (Type == MoveType.Horizontal)
                dest += Vector3.right * MoveDistance;
            else
                dest += Vector3.up * MoveDistance;

            Gizmos.DrawLine(transform.position, dest);
            Gizmos.DrawWireSphere(dest, 0.3f);
        }
    }
}