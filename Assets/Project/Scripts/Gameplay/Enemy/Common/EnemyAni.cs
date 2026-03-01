using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAni : MonoBehaviour
{
    public void AniEvent()
    {
        GetComponentInParent<Enemy>().AniEvent();
    }
}
