using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonBase<T> : MonoBehaviour
where T : class
{
    public static T Instance;
    public virtual void Init()
    {
        Instance = this as T;
    }
}
