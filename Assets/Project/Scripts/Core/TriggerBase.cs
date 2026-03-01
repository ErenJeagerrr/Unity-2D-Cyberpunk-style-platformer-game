using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerBase : MonoBehaviour
{
    [Header("Detection Settings")]
    public Transform CheckCenterPos;
    public float CheckRadius;

    [Header("UI Settings")]
    public string InputContent;
    public bool SingleUse = true;

    private bool InRange;
    private bool OutRange;
    protected bool IsTrigger;
    protected int TriggerCount;

    private void Start()
    {
        InRange = false;
        OutRange = true;
        IsTrigger = false;
        TriggerCount = 0;
    }

    private void Update()
    {
        if (IsTrigger)
        {
            return;
        }

        Collider2D player = Physics2D.OverlapCircle(
        CheckCenterPos != null ? CheckCenterPos.position : transform.position,
        CheckRadius,
        LayerMask.GetMask("Player")
);


        if (player != null && !InRange)
        {
            InRange = true;
            OutRange = false;
            UIService.Instance.ShowPanel<TipPanel>().Init(InputContent);
            EnterRandius();
        }

        if (InRange)
        {
            if (Input.GetKeyDown(KeyCode.E) && !PlayerSystem.Instance.IsPause)
            {
                TriggerEvent();
            }
        }

        if (player == null && !OutRange)
        {
            InRange = false;
            OutRange = true;
            UIService.Instance.HidePanel<TipPanel>();
            ExitRadius();
        }
    }

    public virtual void TriggerEvent()
    {
        if (SingleUse)
        {
            IsTrigger = true;
            UIService.Instance.HidePanel<TipPanel>();
        }
        TriggerCount++;
    }

    public void MyDestoy()
    {
        if (InRange)
        {
            UIService.Instance.HidePanel<TipPanel>();
        }
        Destroy(gameObject);
    }

    public virtual void EnterRandius() { }
    public virtual void ExitRadius() { }

    private void OnDrawGizmos()
    {
        if (CheckCenterPos != null)
        {
            Gizmos.DrawWireSphere(CheckCenterPos.position, CheckRadius);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, CheckRadius);
        }
    }
}