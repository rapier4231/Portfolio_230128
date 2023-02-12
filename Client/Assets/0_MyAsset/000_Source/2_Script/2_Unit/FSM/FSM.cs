using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eState
{
    Idle        = eStateType.Idle,
    Walk        = eStateType.Move,
    Run         = eStateType.Move + 1,
    Dash        = eStateType.Move + 2,
    Jump        = eStateType.Jump,
    Atk         = eStateType.Atk,
    Skill       = eStateType.Atk + 1,
    eState_End  = eStateType.ETC
}

public enum eStateType
{
    Idle    = 0,
    Move    = 10,
    Jump    = 20,
    Atk     = 30,
    ETC     = 40,
}

public class FSM : CMyUnityBase
{
    public      Animator    m_Animator;     //애니메이터
    public      float       m_WalkAniSpeed = 0.7f;
    public      float       m_RunAniSpeed  = 1f;

    protected   State    m_CurrentState; //현재 상태
    public      State    CurrentState
    {
        get { return m_CurrentState; }
    }

    protected   eState   m_eCurrentState;
    public      eState   eCurrentState
    {
        get { return m_eCurrentState; }
        set { m_eCurrentState = value; }
    }

    protected   State[]  m_arrState;     //캐싱한 상태들을 담아 둘 배열

    protected override void NullCheck()
    {
        base.NullCheck();
        if (m_Animator == null)
        {
            m_Animator = gameObject.GetComponent<Animator>();
        }
    }

    protected override void Setting()
    {
        base.Setting();

        //애니메이터가 있을 때만
        if (m_Animator != null)
        {
            Setting_CachingState_Init();
            m_CurrentState = m_arrState[(int)eState.Idle];
            m_CurrentState.Init();
        }
    }

    //m_arrState에 미리 캐싱한 상태들을 담아둔다.
    virtual protected void Setting_CachingState_Init()
    {
        //애니메이터가 있을 때만
        if (m_Animator != null)
        {
            //플레이어가 아닌 친구들은 재정의해서 씁시다. base 부르지 마시구욥.
            m_arrState = new State[(int)eState.eState_End];
            m_arrState[(int)eState.Idle] = new State_Idle(this, "Idle");
            m_arrState[(int)eState.Walk] = new State_Walk(this, "WalkAndSprint");
            m_arrState[(int)eState.Run] = new State_Run(this, "WalkAndSprint");
            m_arrState[(int)eState.Dash] = new State_Dash(this, "Dash");
            m_arrState[(int)eState.Jump] = new State_Jump(this, "Jump");
            m_arrState[(int)eState.Atk] = new State_Atk(this, "RollForward");
            m_arrState[(int)eState.Skill] = new State_Skill(this, "Skill");
        }
    }
    //상태 함수 호출해주기
    virtual public void Call_StateFunc(eState _eState)
    {
        switch (_eState)
        {
            case eState.Idle:
                CurrentState.Idle();
                break;
            case eState.Walk:
                CurrentState.Walk();
                break;
            case eState.Run:
                CurrentState.Run();
                break;
            case eState.Dash:
                CurrentState.Dash();
                break;
            case eState.Jump:
                CurrentState.Jump();
                break;
            case eState.Atk:
                CurrentState.Atk();
                break;
            case eState.Skill:
                CurrentState.Skill();
                break;
            case eState.eState_End:
                break;
        }
    }
    virtual public void  Set_ChangeState(eState _eNextState ,bool _bNoFade = false)
    {
        //같은 상태면 교체 안한다.
        if(m_eCurrentState == _eNextState)
        {
            return;
        }

        m_CurrentState.Exit();

        m_CurrentState  = m_arrState[(int)_eNextState];
        m_eCurrentState = _eNextState;

        m_CurrentState.Init(_bNoFade);
    }

    //public bool Get_CheckState(System.Type _CompareState)
    //{
    //    return CurrentState.GetType() == _CompareState;
    //}

    //public bool Get_CheckState(eState _eState)
    //{
    //    return eCurrentState == _eState;
    //}

    //public bool Get_CheckStateType(eStateType _eStateType)
    //{
    //    return (int)eCurrentState / 10 == (int)_eStateType / 10;
    //}
}

/////////////////////////////////////////////////////////////////////////////////////

public class State
{
    public FSM      m_FSM;
    public string   m_strAnimName;
    public State(FSM _FSM, string _strAnimName) { m_FSM = _FSM; m_strAnimName = _strAnimName; }
    virtual public void Init(bool _bNoFade = false) { }
    virtual public void State_Update() { }
    virtual public void Exit(){ }
    virtual public void Idle(){ }
    virtual public void Walk(){ }
    virtual public void Run(){ }
    virtual public void Dash(){ }
    virtual public void Jump(){ }
    virtual public void Atk(){ }
    virtual public void Skill(){ }
    virtual public void StateEnd() { }
}

class State_Idle : State
{
    public State_Idle(FSM _FSM, string _strAnimName) : base(_FSM, _strAnimName) { }
    override public void Init(bool _bNoFade = false)
    {
        m_FSM.eCurrentState = eState.Idle;
        if (_bNoFade)
        {
            m_FSM.m_Animator.Play(m_strAnimName);
        }
        else
        {
            m_FSM.m_Animator.CrossFade(m_strAnimName, 0.2f);
        }
    }
    override public void State_Update() { }
    override public void Exit() { }
    override public void Idle() 
    {

    }
    override public void Walk() 
    {
        m_FSM.Set_ChangeState(eState.Walk);
    }
    override public void Run() 
    {
        m_FSM.Set_ChangeState(eState.Run);
    }
    override public void Dash()
    {
    }
    override public void Jump() 
    {
        m_FSM.Set_ChangeState(eState.Jump);
    }
    override public void Atk()
    {
        m_FSM.Set_ChangeState(eState.Atk);
    }
    override public void Skill()
    {
    }
    override public void StateEnd() { }

}
class State_Walk : State
{
    public State_Walk(FSM _FSM, string _strAnimName) : base(_FSM, _strAnimName) { }
    override public void Init(bool _bNoFade = false) 
    {
        m_FSM.eCurrentState = eState.Walk;
        if(_bNoFade)
        {
            m_FSM.m_Animator.Play(m_strAnimName);
        }
        else
        {
            m_FSM.m_Animator.CrossFade(m_strAnimName, 0.2f);
        }
        m_FSM.m_Animator.SetFloat("Speed", m_FSM.m_WalkAniSpeed);
    }
    override public void State_Update() { }
    override public void Exit() { }
    override public void Idle() 
    {
        m_FSM.Set_ChangeState(eState.Idle);
    }
    override public void Walk() { }
    override public void Run() 
    {
        m_FSM.Set_ChangeState(eState.Run);
    }
    override public void Dash() { }
    override public void Jump() 
    {
        m_FSM.Set_ChangeState(eState.Jump);
    }
    override public void Atk() 
    {
        m_FSM.Set_ChangeState(eState.Atk);
    }
    override public void Skill() { }
    override public void StateEnd() { }
}
class State_Run : State
{
    public State_Run(FSM _FSM, string _strAnimName) : base(_FSM, _strAnimName) { }
    override public void Init(bool _bNoFade = false) 
    {
        m_FSM.eCurrentState = eState.Run;
        if(_bNoFade)
        {
            m_FSM.m_Animator.Play(m_strAnimName);
        }
        else
        {
            m_FSM.m_Animator.CrossFade(m_strAnimName, 0.2f);
        }
        m_FSM.m_Animator.SetFloat("Speed", m_FSM.m_RunAniSpeed);
    }
    override public void State_Update() { }
    override public void Exit() { }
    override public void Idle() 
    {
        m_FSM.Set_ChangeState(eState.Idle);
    }
    override public void Walk() 
    {
        m_FSM.Set_ChangeState(eState.Walk);
    }
    override public void Run() { }
    override public void Dash() { }
    override public void Jump() 
    {
        m_FSM.Set_ChangeState(eState.Jump);
    }
    override public void Atk() 
    {
        m_FSM.Set_ChangeState(eState.Atk);
    }
    override public void Skill() { }
    override public void StateEnd() { }
}
class State_Dash : State
{
    public State_Dash(FSM _FSM, string _strAnimName) : base(_FSM, _strAnimName) { }
    override public void Init(bool _bNoFade = false) { }
    override public void State_Update() { }
    override public void Exit() { }
    override public void Idle() { }
    override public void Walk() { }
    override public void Run() { }
    override public void Dash() { }
    override public void Jump() { }
    override public void Atk() { }
    override public void Skill() { }
    override public void StateEnd() { }
}
class State_Jump : State
{
    public State_Jump(FSM _FSM, string _strAnimName) : base(_FSM, _strAnimName) { }
    override public void Init(bool _bNoFade = false) 
    {
        m_FSM.eCurrentState = eState.Jump;
        if(_bNoFade)
        {
            m_FSM.m_Animator.Play(m_strAnimName);
        }
        else
        {
            m_FSM.m_Animator.CrossFade(m_strAnimName, 0.2f);
        }
    }
    override public void State_Update() { }
    override public void Exit() 
    {
    }
    override public void Idle() 
    {
        m_FSM.Set_ChangeState(eState.Idle);
    }
    override public void Walk() 
    {
    }
    override public void Run() 
    {
    }
    override public void Dash() { }
    override public void Jump() { }
    override public void Atk() 
    {
        m_FSM.Set_ChangeState(eState.Atk);
    }
    override public void Skill() { }
    override public void StateEnd() { }
}
class State_Atk : State
{
    public State_Atk(FSM _FSM, string _strAnimName) : base(_FSM, _strAnimName) { }
    override public void Init(bool _bNoFade = false) 
    {
        m_FSM.eCurrentState = eState.Atk;
        if(_bNoFade)
        {
            m_FSM.m_Animator.Play(m_strAnimName);
        }
        else
        {
            m_FSM.m_Animator.CrossFade(m_strAnimName, 0.2f);
        }
    }
    override public void State_Update() { }
    override public void Exit() { }
    override public void Idle() 
    {
        if(m_FSM.m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            m_FSM.Set_ChangeState(eState.Idle, true);
        }
    }
    override public void Walk() 
    {
    }
    override public void Run() 
    {
    }
    override public void Dash() { }
    override public void Jump() 
    {
    }
    override public void Atk() { }
    override public void Skill() { }
    override public void StateEnd() { }
}
class State_Skill : State
{
    public State_Skill(FSM _FSM, string _strAnimName) : base(_FSM, _strAnimName) { }
    override public void Init(bool _bNoFade = false) { }
    override public void State_Update() { }
    override public void Exit() { }
    override public void Idle() { }
    override public void Walk() { }
    override public void Run() { }
    override public void Dash() { }
    override public void Jump() { }
    override public void Atk() { }
    override public void Skill() { }
    override public void StateEnd() { }
}