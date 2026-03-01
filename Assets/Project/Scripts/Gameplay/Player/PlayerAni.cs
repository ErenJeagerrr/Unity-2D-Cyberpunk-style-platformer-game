using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAni : MonoBehaviour
{
    public void AniEvent()
    {
        GetComponentInParent<Player>().AniEvent();
    }
}
