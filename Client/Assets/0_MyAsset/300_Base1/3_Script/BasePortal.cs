using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasePortal : CMyUnityBase
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            SceneMng.Instance.Set_LoadSceneAsync(ESceneType.Stage1);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneMng.Instance.Set_LoadSceneAsync(ESceneType.Stage1);
        }
    }
}
