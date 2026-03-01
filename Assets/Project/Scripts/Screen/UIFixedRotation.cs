using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFixedRotation : MonoBehaviour
{
    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}