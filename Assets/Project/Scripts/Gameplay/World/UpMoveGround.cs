using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalMoveGround : MonoBehaviour
{
    public enum MoveDirection
    {
        Up,   
        Down  
    }

    [Header("移动设置")]
    public MoveDirection Direction = MoveDirection.Up; 
    public float MoveDistance = 5f;   
    public float Speed = 2f;      
    public float WaitTime = 1f;     

    private Vector3 startPos;        
    private Vector3 endPos;         
    private Vector3 targetPos;       
    private bool isWaiting;     

    private void Start()
    {
        startPos = transform.position;

        if (Direction == MoveDirection.Up)
        {
            endPos = startPos + Vector3.up * MoveDistance;
        }
        else
        {
            endPos = startPos + Vector3.down * MoveDistance;
        }

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
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.transform.position.y > transform.position.y)
            {
                collision.transform.SetParent(this.transform);
            }
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
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.cyan; 
            Vector3 dest = transform.position;

            if (Direction == MoveDirection.Up)
                dest += Vector3.up * MoveDistance;
            else
                dest += Vector3.down * MoveDistance;

            Gizmos.DrawLine(transform.position, dest);
            Gizmos.DrawWireSphere(dest, 0.5f);
        }
    }
}