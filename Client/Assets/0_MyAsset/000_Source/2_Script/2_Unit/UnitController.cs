using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : FSM
{
    //ȸ�� �����ִ� �ݿ�ƾ ��ü
    public RotationCoroutine m_RotationCoroutine;

    protected override void NullCheck()
    {
        base.NullCheck();
        if (m_RotationCoroutine == null)
        {
            m_RotationCoroutine = GetComponent<RotationCoroutine>();
            if (m_RotationCoroutine == null)
            {
                KLog.Log(gameObject.name + "�� RotationCoroutine ��ũ��Ʈ�� �� �پ� �ֽ��ϴ�.");
            }
        }
    }
}
