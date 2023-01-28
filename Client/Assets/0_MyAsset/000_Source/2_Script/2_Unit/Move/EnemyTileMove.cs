using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTileMove : CMyUnityBase
{
    public float m_Speed = 2f;

    private MapTile m_MapTile;
    private int m_iNextIndex;

    protected override void LateSetting()
    {
        base.LateSetting();
        m_MapTile       = StageMng.Instance.MapTile;
        m_iNextIndex    = m_MapTile.Get_NextLoadIndex(transform.position);
    }

    private void Update()
    {
        float fMoveDis  = Time.deltaTime * m_Speed;
        Vector3 v3Pos   = transform.position;
        //µµÂø ÇßÀ¸¸é ²ö´Ù
        if(m_MapTile.Get_NextPos(ref m_iNextIndex, ref v3Pos, fMoveDis))
        {
            gameObject.SetActive(false);
        }
        else
        {
            transform.position = v3Pos;
        }
    }
}
