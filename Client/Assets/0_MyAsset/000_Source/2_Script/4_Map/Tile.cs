using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(Tile))]

public class Tile_Editor : Editor
{
    private void OnSceneGUI()
    {
        Tile cTile = (Tile)target;
        cTile.Set_Material();
    }
}
#endif 

public class Tile : CMyUnityBase
{
    public enum ETileType
    {
        Low1,
        Low2,
        LowX,
        High1 = 10,
        High2,
        HighX,
        ETileType_End = 20
    }

    public struct STAround
    {
        public Tile TopTile;
        public Tile LeftTile;
        public Tile BottomTile;
        public Tile RightTile;
    }

    [Range(1, 3)]
    public int          m_StageNum = 1;
    public ETileType    m_TileType = ETileType.ETileType_End;

    private int m_iPosX = 0;
    private int m_iPosY = 0;
    public int iPosX
    {
        get { return m_iPosX; }
        set { m_iPosX = value; }
    }
    public int iPosY
    {
        get { return m_iPosY; }
        set { m_iPosY = value; }
    }

    private MeshFilter  m_MeshFilter;
    private float       m_fHighPosY = 0.8f;

    private BoxCollider m_BoxCollider;

    private STAround m_stAround;
    public STAround stAround
    {
        get { return m_stAround; }
        set { m_stAround = value; }
    }

    [HideInInspector]
    public Tile m_ParentTile;

    //StageMapCreate���� Init ���� �� ģ���� Init�� ���� �� �� ���� �� X
    public override void Awake()
    {
        Set_AddBoxCol();
    }

    public override void Start()
    {
        base.Start();
    }

    protected override void Setting()
    {
        base.Setting();
        Set_Pos();
        Set_Material();
    }

    public void Set_Material()
    {
        m_MeshFilter = GetComponent<MeshFilter>();
        if(m_MeshFilter == null)
        {
            Debug.Log("Ÿ���� MeshFilter ������Ʈ�� �����ϴ�.");
            return;
        }

        int iHight = ((int)m_TileType / 10);

        if(iHight == 0)
        {
            Vector3 v3Pos = transform.localPosition;
            v3Pos.y = 0f;
            transform.localPosition = v3Pos;
        }
        else if(iHight == 1)
        {
            Vector3 v3Pos           = transform.localPosition;
            v3Pos.y                 = m_fHighPosY;
            transform.localPosition = v3Pos;
        }
        else if (iHight >= 2)
        {
            Debug.Log("Ÿ�� Ÿ���� ETileType_End�̰ų� �����Դϴ�. (Low�� High ���� �մϴ�.");
            return;
        }

        int iType = (int)m_TileType - (iHight * 10);

        string strAfterPath = m_StageNum.ToString() + "_" + iHight.ToString() + "_" + iType.ToString();

        m_MeshFilter.mesh = Resources.Load<Mesh>("Tile/Mesh/" + strAfterPath);
        
        Set_AddBoxCol();
    }

    public void Set_Pos()
    {
        Vector3 v3Pos = transform.localPosition;
        v3Pos.x = (float)m_iPosX;
        v3Pos.z = (float)m_iPosY;
        transform.localPosition = v3Pos;
    }

    public void Set_AddBoxCol()
    {
        m_BoxCollider = GetComponent<BoxCollider>();
        if(m_BoxCollider== null)
        {
            m_BoxCollider = gameObject.AddComponent<BoxCollider>();
        }

        Vector3 v3Center    = Vector3.zero;
        Vector3 v3Size      = Vector3.zero;

        int iHight = ((int)m_TileType / 10);
        int iType = (int)m_TileType - (iHight * 10);
        v3Size.x = transform.localScale.x;
        v3Size.y = transform.localScale.y;
        v3Size.z = transform.localScale.z;
        //���ع��� ���� ũ�� ���ش� (Low�� 0.8 ���Ƽ� 2���ϸ� 0.2�ۿ� �ȿö���ϱ� Ȯ���ϰ� �ø��� ����)
        if (iType == 2)
        {
            v3Size.y *= 2;
        }

        //���� �� �̶��
        v3Center.y -= (v3Size.y * 0.5f);
        if (iHight == 2)
        { 
            v3Center.y += m_fHighPosY; 
        }

        if (iType == 2)
        {
            v3Center.y += v3Size.y * 0.5f;
            if (iHight == 0)
            {
                v3Center.y += m_fHighPosY;
            }
        }
        Setting_BoxCol(v3Center, v3Size);
    }

    private void Setting_BoxCol(Vector3 _v3Center, Vector3 _v3Size)
    {
        m_BoxCollider.center    = _v3Center;
        m_BoxCollider.size      = _v3Size;
    }
}
