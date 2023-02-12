using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Info : CMyUnityBase
{
    /*protected*/ public float m_fHp        = 1f;
    /*protected*/ public float m_fMaxHp     = 1f;
    /*protected*/ public float m_fWeight    = 2f;
    /*protected*/ public float m_fPower     = 1f;
    /*protected*/ public float m_fCritical  = 10f;
    /*protected*/ public float m_fDefense   = 0f;
    /*protected*/ public float m_fAvoid     = 10f; //È¸ÇÇÀ²

    protected override void Setting()
    {
        base.Setting();

        //CSVReader.Read("Unit/Info");


    }
}
