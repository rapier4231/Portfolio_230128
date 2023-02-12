using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyBaseTag : MonoBehaviour
{
    public CMyUnityBase.EMyTag m_eTag;

    private void Awake()
    {
        UnitController unitController = gameObject.GetComponent<UnitController>();

        if(unitController == null)
        {
            KLog.Log(gameObject.name + "에 UnitController가 없습니다. - MyBaseTag 실패");
        }
        else
        {
            unitController.Set_AddMyTag(m_eTag);
        }

        Destroy(this);
    }
}
