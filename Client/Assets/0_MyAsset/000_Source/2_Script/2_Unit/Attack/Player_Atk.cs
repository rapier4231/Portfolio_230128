using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Atk : Attack
{
    private bool m_bMove = false;

    public override void Set_Atk(CMyUnityBase.EMyTag _eMyTag)
    {
        base.Set_Atk(_eMyTag);
        //어택 애니메이션!
        m_UnitController.CurrentState.Atk();
        StartCoroutine(AtkAnim());
        StartCoroutine(AtkMove(transform.forward,2f));
    }

    IEnumerator AtkAnim()
    {
        m_bMove = true;

        while (true)
        {
            yield return null;

            if (m_UnitController.m_Animator.GetCurrentAnimatorStateInfo(0).IsName("RollForward"))
            {
                break;
            }
        }

        WaitForSeconds Wait = new WaitForSeconds(0.1f);
        while (true)
        {
            yield return Wait;

            if (m_UnitController.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
            {
                break;
            }
            else if(m_UnitController.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.7f)
            {
                //임의로 추가 - 구를 때 마지막엔 앞으로 나가는게 조금 이상해 보여서
                m_bMove = false;
            }
        }

        m_bMove = false;
        m_UnitController.CurrentState.Idle();
    }

    IEnumerator AtkMove(Vector3 _v3Dir, float _fSpeed)
    {
        while(m_bMove)
        {
            m_UnitController.m_Move.MoveFunc(_v3Dir, _fSpeed);
            yield return null;
        }
    }


    //기본 공격
    public override void UnitAtk(UnitController _unitController)
    {
        base.UnitAtk(_unitController);

        _unitController.Set_Hit(m_UnitController.m_Info.m_fPower, transform.forward,5f);
    }
}
