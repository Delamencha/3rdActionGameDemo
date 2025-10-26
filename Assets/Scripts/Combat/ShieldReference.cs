using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldReference : MonoBehaviour
{

    [field: SerializeField] public Health Health { get; private set; }


	private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        BoxCollider box = col as BoxCollider;
        if (box != null)
        {
            Matrix4x4 old = Gizmos.matrix;
            Transform t = box.transform;
            Gizmos.matrix = Matrix4x4.TRS(t.position, t.rotation, t.lossyScale);
            Gizmos.DrawWireCube(box.center, box.size);
            Gizmos.matrix = old;
            return;
        }


    }

}
