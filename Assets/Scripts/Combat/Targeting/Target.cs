using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{

    public event Action<Target> OnTargetDestroy;

    private void OnDestroy()
    {
        OnTargetDestroy?.Invoke(this);
    }

}
