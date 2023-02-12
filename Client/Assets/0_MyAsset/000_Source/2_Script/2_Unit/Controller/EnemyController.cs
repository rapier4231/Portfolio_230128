using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : UnitController
{
    private Enemy_Move m_Enemy_Move;

    protected override void NullCheck()
    {
        if (m_Enemy_Move == null)
        {
            m_Enemy_Move = GetComponent<Enemy_Move>();
            if (m_Enemy_Move == null)
            {
                m_Enemy_Move = gameObject.AddComponent<Enemy_Move>();
                KLog.Log(gameObject.name + "에 Enemy_Move 스크립트가 안 붙어 있습니다.");
            }
        }

        if (m_Info == null)
        {
            m_Info = GetComponent<Info>();
            if (m_Info == null)
            {
                m_Info = gameObject.AddComponent<Info>();
                KLog.Log(gameObject.name + "에 Info 스크립트가 안 붙어 있습니다.");
            }
        }

        if (m_PushMoveCoroutine == null)
        {
            m_PushMoveCoroutine = GetComponent<PushMoveCoroutine>();
            if (m_PushMoveCoroutine == null)
            {
                m_PushMoveCoroutine = gameObject.AddComponent<PushMoveCoroutine>();
                KLog.Log(gameObject.name + "에 PushMoveCoroutine 스크립트가 안 붙어 있습니다.");
            }
        }
    }
}

