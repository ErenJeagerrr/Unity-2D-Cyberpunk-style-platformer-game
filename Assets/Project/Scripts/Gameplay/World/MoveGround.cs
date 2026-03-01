using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGround : MonoBehaviour
{
    [Header("移动设置")]
    public float Speed = 3f;         
    public float MoveDistance = 5f;    
    public float WaitTime = 0.5f;      

    private Vector3 startPos;         
    private Vector3 endPos;           
    private Vector3 targetPos;        
    private bool isWaiting;           

    private void Start()
    {
        startPos = transform.position;
        endPos = startPos + Vector3.right * MoveDistance;
        targetPos = endPos;
    }

    private void FixedUpdate()
    {
        if (isWaiting) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, Speed * Time.fixedDeltaTime);

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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.transform.position.y > transform.position.y)
        {
            collision.transform.SetParent(this.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
            DontDestroyOnLoad(collision.gameObject); 
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * MoveDistance);
    }
}