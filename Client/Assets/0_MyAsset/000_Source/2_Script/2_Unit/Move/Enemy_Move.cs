using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Move : Move
{
    private MapTile m_MapTile;
    private int     m_iNextIndex;

    public override bool bMoveCC
    {
        set 
        {
            m_bMoveCC = value; 

            if(!m_bMoveCC)
            {
                m_iNextIndex = m_MapTile.Get_NextLoadIndex(transform.position);
            }
        }
        get { return m_bMoveCC; }
    }

    protected override void LateSetting()
    {
        base.LateSetting();
        m_MapTile       = StageMng.Instance.MapTile;
        m_iNextIndex    = m_MapTile.Get_NextLoadIndex(transform.position);
    }

    private void Update()
    {
        if (!m_bMoveCC)
        {
            //µµÂø ÇßÀ¸¸é ²ö´Ù
            if (m_MapTile.Get_NextPos(ref m_iNextIndex, transform.position, m_Speed, this))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
