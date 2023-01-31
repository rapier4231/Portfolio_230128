using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapTile : CMyUnityBase
{
    public struct STOpenTile
    {
        public Tile OpenTile;
        public int  iDistanceF;
        public int  iDistanceG;
        public int  iDistanceH;
    }

    public int m_NumX = 0;
    public int m_NumY = 0;

    private Tile[,] m_TileArray;

    private List<Tile>  m_LoadTileList;
    private int         m_iLoadLastTileIndex;

    private Tile       m_FirstTile;
    private Tile       m_LastTile;

    //Null ���� -> ���� �����尡 ������ �ʾҴ�.
    //true -> �������� �� ���� ����Ǿ� �ִ�.
    //false -> ������ �ȵǾ� �ִ�.
    private bool?      m_bLoadConnected = null;

    protected override void NewCreate()
    {
        base.NewCreate();
        m_TileArray     = new Tile[m_NumX, m_NumY];
        m_LoadTileList  = new List<Tile>();
    }

    protected override void Setting()
    {
        base.Setting();

        for (int x = 0; x < m_NumX; x++)
        {
            for (int y = 0; y < m_NumY; y++)
            {
                m_TileArray[x, y] = transform.GetChild(((x * m_NumY) + y)).GetComponent<Tile>();
                m_TileArray[x, y].iPosX = x;
                m_TileArray[x, y].iPosY = y;
            }
        }
        m_FirstTile = m_TileArray[1, (m_NumY - 1)];
        m_LastTile = m_TileArray[(m_NumX - 2), 0];
        
        //�� ã�� �Ǵ��� Ȯ��.
        Set_GameStart();

        //�� �������� Awake���� �������ش�. ��� ����� ��� Start ���Ŀ� ����� ��.
        StageMng.Instance.MapTile = this;
    }

    public void Set_GameStart()
    {
        Thread thread = new Thread(FindLoad);
        thread.Start();
        UpdateCheckMng.Instance.Set_UpdateCheck(Get_ThreadEndCheck, Set_SwapLoadMesh);
    }

    public bool Get_ThreadEndCheck()
    {
        if(ReferenceEquals(m_bLoadConnected,null))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Set_NumXY(int _iNumX, int _iNumY)
    {
        m_NumX = _iNumX;
        m_NumY = _iNumY;
    }

    public void FindLoad()
    {
        Setting_Tile_AroundInfo();
        Setting_Load();
    }

    private void Setting_Tile_AroundInfo()
    {
        int iMaxX = m_NumX - 1;
        int iMaxY = m_NumY - 1;

        for (int x = 0; x < m_NumX; x++)
        {
            for (int y = 0; y < m_NumY; y++)
            {
                Tile.STAround stAround = new Tile.STAround();
                //Top
                //�� ���� �ƴ϶��
                if(y != iMaxY)
                {
                    stAround.TopTile = m_TileArray[x, (y + 1)];
                }
                //Left
                //�� ������ �ƴ϶��
                if(x != 0)
                {
                    stAround.LeftTile = m_TileArray[(x - 1), y];
                }
                //Bottom
                //�� �Ʒ��� �ƴ϶��
                if(y != 0)
                {
                    stAround.BottomTile = m_TileArray[x, (y - 1)];
                }
                //Right
                //�� �������� �ƴ϶��
                if(x != iMaxX)
                {
                    stAround.RightTile = m_TileArray[(x + 1), y];
                }

                m_TileArray[x, y].stAround = stAround;
            }
        }
    }

    private void Setting_Load()
    {
        //A* ���
        //F = G + H
        //�����->���������� �� Cost = ���� ���->��� �������� Cost + �޸���ƽ
        List<STOpenTile>    stOpenTileList  = new List<STOpenTile>();
        bool[,]             bCloseArray     = new bool[m_NumX, m_NumY];

        //�� ó���� ù��° Ÿ�� �־��ְ�
        AddOpenTileList(bCloseArray,stOpenTileList, m_FirstTile, null, 0);

        //A* �����ش�.
        if (khAstar(bCloseArray, stOpenTileList))
        {
            //���� ���Դٸ� �� ������ �ָ� �־��ְ�
            Tile GoalTile = stOpenTileList[stOpenTileList.Count - 1].OpenTile;

            //���� ������ Ÿ���� Ʋ�ȴٸ�
            if(!ReferenceEquals(m_LastTile, GoalTile))
            {
                m_bLoadConnected = false;
                return;
            }

            m_LoadTileList.Add(GoalTile);
            //�� Ÿ�� �±׸� �־��ش�
            GoalTile.Set_AddMyTag(EMyTag.GoalTile);

            //������ Parent�� Null�� �� ���� ã�´�
            Tile ParentTile = GoalTile.m_ParentTile;
            while (!ReferenceEquals(ParentTile, null))
            {
                m_LoadTileList.Add(ParentTile);
                ParentTile = ParentTile.m_ParentTile;
            }

            //�Ųٷ� ����� ������ -> ���� ������ �ٲ۴�.
            m_LoadTileList.Reverse();
            m_iLoadLastTileIndex = m_LoadTileList.Count - 1;
            m_bLoadConnected = true;
        }
        else
        {
            m_bLoadConnected = false;
        }
    }

    private bool khAstar(bool[,] _bCloseArray, List<STOpenTile> _stOpenTileList)
    {
        //���� F�� ���� ���� Ÿ�� �ε��� �ް�
        int iNearIndex = Find_NearOpenTileIndex(_bCloseArray, _stOpenTileList);
        //���� ���� ���� Max�� ���� ���ٴ� ��. ������. (_stOpenTileList ũ�Ⱑ 0��)
        if (iNearIndex == int.MaxValue)
        {
            return false;
        }
        //�ش� Ÿ���� �ֺ��� �˻��ؼ� OpenTileList�� �ְ�
        //���� �������� �����ϰ� (���� �������� �� ģ���� �θ� ���󰡸� �ϼ�)
        if(AddOpenTileList_AroundTile(_bCloseArray, _stOpenTileList, iNearIndex))
        {
            return true;
        }
        else
        {
            //���� �ȳ������� iNearIndex�� �ֺ� ģ���� �� �־������� OpenTileList���� ����.
            Tile NearTile = _stOpenTileList[iNearIndex].OpenTile;
            _bCloseArray[NearTile.iPosX, NearTile.iPosY] = true;
            _stOpenTileList.RemoveAt(iNearIndex);
        }

        //���� ���� �� ���� ��� ������
        return khAstar(_bCloseArray, _stOpenTileList);
    }

    private int Find_NearOpenTileIndex(bool[,]  _bCloseArray ,List<STOpenTile> _stOpenTileList)
    {
        //close ������ ��ͽ�, ���� ����Ʈ�� ������ �׳� ���� ���´�
        if (_stOpenTileList.Count == 0)
        {
            return int.MaxValue;
        }

        int iDisMini    = int.MaxValue;
        int iDisTemp    = int.MaxValue;
        int iNearIndex  = int.MaxValue;

        //���� ���� �Ÿ��� ª�� ģ���� ã�ڴ�
        int iListCount  = _stOpenTileList.Count;
        for (int i = 0; i < iListCount; i++)
        {
            iDisTemp = _stOpenTileList[i].iDistanceF;
            if (iDisMini > iDisTemp)
            {
                iDisMini    = iDisTemp;
                iNearIndex  = i;
            }
        }

        Tile nearTile = _stOpenTileList[iNearIndex].OpenTile;
        //�ٵ� ã�� �� �ֺ��� ���� close �� ģ�����?
        if (Inspect_AdjNodeClose(nearTile))
        {
            _bCloseArray[nearTile.iPosX, nearTile.iPosY] = true;
            _stOpenTileList.RemoveAt(iNearIndex);

            //��ͷ� �ٽ� ã���ش�
            return Find_NearOpenTileIndex(_bCloseArray, _stOpenTileList);
        }
        else
        {
            return iNearIndex;
        }

    }

    private void AddOpenTileList(bool[,] _bCloseArray, List<STOpenTile> _stOpenTileList, Tile _Tile, Tile _parentTile,int _iDisG)
    {
        //���� ���� Ÿ���̶�� ���´�
        if(_bCloseArray[_Tile.iPosX, _Tile.iPosY])
        {
            return;
        }

        STOpenTile stOpenTile       = new STOpenTile();
        stOpenTile.OpenTile         = _Tile;
        _Tile.m_ParentTile          = _parentTile;
        int iDisX                   = m_LastTile.iPosX - _Tile.iPosX;
        int iDisY                   = _Tile.iPosY - m_LastTile.iPosY; //Y�� ��Ʈ Ÿ���� 0��.
        stOpenTile.iDistanceH       = iDisX + iDisY;
        stOpenTile.iDistanceG       = _iDisG;
        stOpenTile.iDistanceF       = stOpenTile.iDistanceH + stOpenTile.iDistanceG;

        _stOpenTileList.Add(stOpenTile);
    }

    //���� ��带 �˻�, ������ ��尡 ���� �����ִٸ� true ��ȯ
    private bool Inspect_AdjNodeClose(Tile _Tile)
    {
        int iAroundCloseCount = 0;

        Tile.STAround stAround = _Tile.stAround;
        if (ReferenceEquals(stAround.TopTile, null) || (stAround.TopTile.m_TileType != Tile.ETileType.Low1))
        {
            ++iAroundCloseCount;
        }
        if (ReferenceEquals(stAround.LeftTile, null) || (stAround.LeftTile.m_TileType != Tile.ETileType.Low1))
        {
            ++iAroundCloseCount;
        }
        if (ReferenceEquals(stAround.BottomTile, null) || (stAround.BottomTile.m_TileType != Tile.ETileType.Low1))
        {
            ++iAroundCloseCount;
        }
        if (ReferenceEquals(stAround.RightTile, null) || (stAround.RightTile.m_TileType != Tile.ETileType.Low1))
        {
            ++iAroundCloseCount;
        }

        //���� �� ���� �� �����ִٸ�
        if(iAroundCloseCount == 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //�������� True�� �������� (��Ʈ Ÿ�� ã����)
    private bool AddOpenTileList_AroundTile(bool[,] _bCloseArray, List<STOpenTile> _stOpenTileList, int _iNearTileIndex)
    {
        Tile.STAround stAround = _stOpenTileList[_iNearTileIndex].OpenTile.stAround;

        if (ReferenceEquals(stAround.BottomTile, m_LastTile))
        {
            AddOpenTileList(_bCloseArray, _stOpenTileList, stAround.BottomTile, _stOpenTileList[_iNearTileIndex].OpenTile, _stOpenTileList[_iNearTileIndex].iDistanceG);
            return true;
        }
        if (ReferenceEquals(stAround.RightTile, m_LastTile))
        {
            AddOpenTileList(_bCloseArray, _stOpenTileList, stAround.RightTile, _stOpenTileList[_iNearTileIndex].OpenTile, _stOpenTileList[_iNearTileIndex].iDistanceG);
            return true;
        }
        if (ReferenceEquals(stAround.TopTile, m_LastTile))
        {
            AddOpenTileList(_bCloseArray, _stOpenTileList, stAround.TopTile, _stOpenTileList[_iNearTileIndex].OpenTile, _stOpenTileList[_iNearTileIndex].iDistanceG);
            return true;
        }
        if (ReferenceEquals(stAround.LeftTile, m_LastTile))
        {
            AddOpenTileList(_bCloseArray, _stOpenTileList, stAround.LeftTile, _stOpenTileList[_iNearTileIndex].OpenTile, _stOpenTileList[_iNearTileIndex].iDistanceG);
            return true;
        }

        //������� ���� �˻��ϱ� ������ �Ʒ�, ��, ��, �� ������� �ִ´�
        if (!ReferenceEquals(stAround.BottomTile, null) && (stAround.BottomTile.m_TileType == Tile.ETileType.Low1))
        {
            AddOpenTileList(_bCloseArray, _stOpenTileList, stAround.BottomTile, _stOpenTileList[_iNearTileIndex].OpenTile, _stOpenTileList[_iNearTileIndex].iDistanceG);
        }
        if (!ReferenceEquals(stAround.RightTile, null) && (stAround.RightTile.m_TileType == Tile.ETileType.Low1))
        {
            AddOpenTileList(_bCloseArray, _stOpenTileList, stAround.RightTile, _stOpenTileList[_iNearTileIndex].OpenTile, _stOpenTileList[_iNearTileIndex].iDistanceG);
        }
        if (!ReferenceEquals(stAround.TopTile, null) && (stAround.TopTile.m_TileType == Tile.ETileType.Low1))
        {
            AddOpenTileList(_bCloseArray, _stOpenTileList, stAround.TopTile, _stOpenTileList[_iNearTileIndex].OpenTile, _stOpenTileList[_iNearTileIndex].iDistanceG);
        }
        if (!ReferenceEquals(stAround.LeftTile, null) && (stAround.LeftTile.m_TileType == Tile.ETileType.Low1))
        {
            AddOpenTileList(_bCloseArray, _stOpenTileList, stAround.LeftTile, _stOpenTileList[_iNearTileIndex].OpenTile, _stOpenTileList[_iNearTileIndex].iDistanceG);
        }

        return false;
    }

    public void Set_SwapLoadMesh()
    {
        foreach(Tile LoadTile in m_LoadTileList)
        {
            LoadTile.m_TileType = Tile.ETileType.Low2;
            LoadTile.Set_Material();
        }
    }

    //�������� ��, Ư�� �������� ���� ��ġ�� �ٲ���� ��� �ѹ� �ҷ��ش�
    public int Get_NextLoadIndex(Vector3 _v3NowPos)
    {
        //���� Ÿ�� �ε����� ã�Ƽ� �� ���� �ε����� ã�´�.
        Vector3 v3TempPos;      //1f�� �⺻ ũ��
        float fTileHalfSizeX = (/*1f * */m_LoadTileList[0].transform.localScale.x * 0.5f);
        float fTileHalfSizeZ = (/*1f * */m_LoadTileList[0].transform.localScale.z * 0.5f);

        int iListCount = m_LoadTileList.Count;
        for (int i = 0; i < iListCount; i++)
        {
            v3TempPos = m_LoadTileList[i].transform.position;
            //Ÿ�� �ȿ� �ִٸ�
            if(    (v3TempPos.x - fTileHalfSizeX) <= _v3NowPos.x 
                && (v3TempPos.x + fTileHalfSizeX) >= _v3NowPos.x
                && (v3TempPos.z - fTileHalfSizeZ) <= _v3NowPos.z
                && (v3TempPos.z + fTileHalfSizeZ) >= _v3NowPos.z)
            {
                return i;
            }
        }

        //Ÿ�� �ȿ� ���ٸ� ���� ����� Ÿ���� ã���� ����
        //ù��° Ÿ���� �׳� �⺻������ �־��ش�.
        int     iMin = 0;
        float   fMinDis = Vector3.Distance(_v3NowPos, m_LoadTileList[iMin].transform.position);
        float   fTempDis;
        for (int i = 1; i < iListCount; i++)
        {
            fTempDis = Vector3.Distance(_v3NowPos, m_LoadTileList[i].transform.position);
            if(fTempDis < fMinDis)
            {
                iMin = i;
                fMinDis = fTempDis;
            }
        }

        return iMin;
    }

    //true�� ��ȯ�ϸ� �������� ���� �ߴٴ� ��.
    public bool Get_NextPos(ref int _iGoalLoadIndex, ref Vector3 _v3Pos, float _fDis)
    {
        Vector3 v3GoalPos   = m_LoadTileList[_iGoalLoadIndex].transform.position;
        float fGoalDis      = Vector3.Distance(v3GoalPos, _v3Pos);

        Vector3 v3GoalDir;
        //���� �����ϴ� �Ÿ��� �� ũ�ٸ�
        if (fGoalDis < _fDis)
        {
            //�ε��� �ϳ� �÷��ش�
            _iGoalLoadIndex += 1;

            if(_iGoalLoadIndex == m_iLoadLastTileIndex)
            {
                return true;
            }

            _v3Pos      = v3GoalPos;
            v3GoalPos   = m_LoadTileList[_iGoalLoadIndex].transform.position;
            _fDis       -= fGoalDis;
        }
        v3GoalDir   = v3GoalPos - _v3Pos;

        _v3Pos      = _v3Pos + (v3GoalDir.normalized * _fDis);
        return false;
    }

    public Vector3 Get_TilePos(int _ix, int _iy)
    {
        return m_TileArray[_ix, _iy].transform.position;
    }

    public Tile Get_Tile(int _ix, int _iy)
    {
        return m_TileArray[_ix, _iy];
    }

    //�÷��̾� �Ʒ� Ÿ�� xy�� ã�´�.
    //������ 0,0 Tile�� 0,0 Pos�� �ִ�. 
    //-0.5 ~ 0.5 -> x 0
    public void Get_UnderTile_Index(Vector3 v3Pos, ref int _ix, ref int _iy)
    {
        Vector3 v3PosTemp = v3Pos;
        float fTileScaleX = m_LastTile.transform.localScale.x;
        float fTileScaleZ = m_LastTile.transform.localScale.z;

        v3PosTemp.x += fTileScaleX * 0.5f;
        v3PosTemp.z += fTileScaleZ * 0.5f;

        _ix = (int)(v3PosTemp.x / fTileScaleX);
        _iy = (int)(v3PosTemp.z / fTileScaleZ);
    }

    public float Get_UnderTile_Height(Vector3 v3Pos)
    {
        Vector3 v3PosTemp = v3Pos;
        float fTileScaleX = m_LastTile.transform.localScale.x;
        float fTileScaleZ = m_LastTile.transform.localScale.z;

        v3PosTemp.x += fTileScaleX * 0.5f;
        v3PosTemp.z += fTileScaleZ * 0.5f;

        return m_TileArray[(int)(v3PosTemp.x / fTileScaleX), (int)(v3PosTemp.z / fTileScaleZ)].transform.position.y;
    }
}
