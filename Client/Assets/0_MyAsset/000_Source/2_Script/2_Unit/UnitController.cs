using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitController : FSM
{
    //회전 시켜주는 콜우틴 객체
    public RotationCoroutine m_RotationCoroutine;

    protected override void NullCheck()
    {
        base.NullCheck();
        if (m_RotationCoroutine == null)
        {
            m_RotationCoroutine = GetComponent<RotationCoroutine>();
            if (m_RotationCoroutine == null)
            {
                KLog.Log(gameObject.name + "에 RotationCoroutine 스크립트가 안 붙어 있습니다.");
            }
        }
    }
}
