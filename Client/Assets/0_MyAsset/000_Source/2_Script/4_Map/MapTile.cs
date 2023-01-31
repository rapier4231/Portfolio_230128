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

    //Null 상태 -> 아직 쓰레드가 끝나지 않았다.
    //true -> 시작점과 끝 점이 연결되어 있다.
    //false -> 연결이 안되어 있다.
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
        
        //길 찾기 되는지 확인.
        Set_GameStart();

        //매 스테이지 Awake에서 설정해준다. 고로 사용은 모두 Start 이후에 사용할 것.
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
                //맨 위가 아니라면
                if(y != iMaxY)
                {
                    stAround.TopTile = m_TileArray[x, (y + 1)];
                }
                //Left
                //맨 왼쪽이 아니라면
                if(x != 0)
                {
                    stAround.LeftTile = m_TileArray[(x - 1), y];
                }
                //Bottom
                //맨 아래가 아니라면
                if(y != 0)
                {
                    stAround.BottomTile = m_TileArray[x, (y - 1)];
                }
                //Right
                //맨 오른쪽이 아니라면
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
        //A* 사용
        //F = G + H
        //출발지->목적지까지 총 Cost = 현재 노드->출발 지점까지 Cost + 휴리스틱
        List<STOpenTile>    stOpenTileList  = new List<STOpenTile>();
        bool[,]             bCloseArray     = new bool[m_NumX, m_NumY];

        //맨 처음엔 첫번째 타일 넣어주고
        AddOpenTileList(bCloseArray,stOpenTileList, m_FirstTile, null, 0);

        //A* 돌려준다.
        if (khAstar(bCloseArray, stOpenTileList))
        {
            //길이 나왔다면 맨 마지막 애를 넣어주고
            Tile GoalTile = stOpenTileList[stOpenTileList.Count - 1].OpenTile;

            //만약 마지막 타일이 틀렸다면
            if(!ReferenceEquals(m_LastTile, GoalTile))
            {
                m_bLoadConnected = false;
                return;
            }

            m_LoadTileList.Add(GoalTile);
            //골 타일 태그를 넣어준다
            GoalTile.Set_AddMyTag(EMyTag.GoalTile);

            //마지막 Parent가 Null일 때 까지 찾는다
            Tile ParentTile = GoalTile.m_ParentTile;
            while (!ReferenceEquals(ParentTile, null))
            {
                m_LoadTileList.Add(ParentTile);
                ParentTile = ParentTile.m_ParentTile;
            }

            //거꾸로 뒤집어서 시작점 -> 끝점 순서로 바꾼다.
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
        //가장 F가 작은 오픈 타일 인덱스 받고
        int iNearIndex = Find_NearOpenTileIndex(_bCloseArray, _stOpenTileList);
        //만약 받은 수가 Max면 길이 없다는 뜻. 끝낸다. (_stOpenTileList 크기가 0임)
        if (iNearIndex == int.MaxValue)
        {
            return false;
        }
        //해당 타일의 주변을 검사해서 OpenTileList에 넣고
        //만약 끝났으면 리턴하고 (가장 마지막에 들어간 친구의 부모 따라가면 완성)
        if(AddOpenTileList_AroundTile(_bCloseArray, _stOpenTileList, iNearIndex))
        {
            return true;
        }
        else
        {
            //아직 안끝났으면 iNearIndex는 주변 친구들 다 넣어줬으니 OpenTileList에서 뺀다.
            Tile NearTile = _stOpenTileList[iNearIndex].OpenTile;
            _bCloseArray[NearTile.iPosX, NearTile.iPosY] = true;
            _stOpenTileList.RemoveAt(iNearIndex);
        }

        //답이 나올 때 까지 재귀 돌려라
        return khAstar(_bCloseArray, _stOpenTileList);
    }

    private int Find_NearOpenTileIndex(bool[,]  _bCloseArray ,List<STOpenTile> _stOpenTileList)
    {
        //close 때문에 재귀시, 열린 리스트가 없으면 그냥 빠져 나온다
        if (_stOpenTileList.Count == 0)
        {
            return int.MaxValue;
        }

        int iDisMini    = int.MaxValue;
        int iDisTemp    = int.MaxValue;
        int iNearIndex  = int.MaxValue;

        //가장 예상 거리가 짧은 친구를 찾겠다
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
        //근데 찾은 애 주변이 전부 close 된 친구라면?
        if (Inspect_AdjNodeClose(nearTile))
        {
            _bCloseArray[nearTile.iPosX, nearTile.iPosY] = true;
            _stOpenTileList.RemoveAt(iNearIndex);

            //재귀로 다시 찾아준다
            return Find_NearOpenTileIndex(_bCloseArray, _stOpenTileList);
        }
        else
        {
            return iNearIndex;
        }

    }

    private void AddOpenTileList(bool[,] _bCloseArray, List<STOpenTile> _stOpenTileList, Tile _Tile, Tile _parentTile,int _iDisG)
    {
        //만약 닫힌 타일이라면 나온다
        if(_bCloseArray[_Tile.iPosX, _Tile.iPosY])
        {
            return;
        }

        STOpenTile stOpenTile       = new STOpenTile();
        stOpenTile.OpenTile         = _Tile;
        _Tile.m_ParentTile          = _parentTile;
        int iDisX                   = m_LastTile.iPosX - _Tile.iPosX;
        int iDisY                   = _Tile.iPosY - m_LastTile.iPosY; //Y는 라스트 타일이 0값.
        stOpenTile.iDistanceH       = iDisX + iDisY;
        stOpenTile.iDistanceG       = _iDisG;
        stOpenTile.iDistanceF       = stOpenTile.iDistanceH + stOpenTile.iDistanceG;

        _stOpenTileList.Add(stOpenTile);
    }

    //인접 노드를 검사, 인접한 노드가 전부 막혀있다면 true 반환
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

        //만약 네 방향 다 막혀있다면
        if(iAroundCloseCount == 4)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //끝났으면 True를 리턴해줌 (라스트 타일 찾으면)
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

        //순서대로 먼저 검사하기 때문에 아래, 우, 위, 좌 순서대로 넣는다
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

    //밀쳐지는 등, 특이 사항으로 인해 위치가 바뀌었을 경우 한번 불러준다
    public int Get_NextLoadIndex(Vector3 _v3NowPos)
    {
        //현재 타일 인덱스를 찾아서 그 다음 인덱스를 찾는다.
        Vector3 v3TempPos;      //1f는 기본 크기
        float fTileHalfSizeX = (/*1f * */m_LoadTileList[0].transform.localScale.x * 0.5f);
        float fTileHalfSizeZ = (/*1f * */m_LoadTileList[0].transform.localScale.z * 0.5f);

        int iListCount = m_LoadTileList.Count;
        for (int i = 0; i < iListCount; i++)
        {
            v3TempPos = m_LoadTileList[i].transform.position;
            //타일 안에 있다면
            if(    (v3TempPos.x - fTileHalfSizeX) <= _v3NowPos.x 
                && (v3TempPos.x + fTileHalfSizeX) >= _v3NowPos.x
                && (v3TempPos.z - fTileHalfSizeZ) <= _v3NowPos.z
                && (v3TempPos.z + fTileHalfSizeZ) >= _v3NowPos.z)
            {
                return i;
            }
        }

        //타일 안에 없다면 가장 가까운 타일을 찾으러 간다
        //첫번째 타일을 그냥 기본값으로 넣어준다.
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

    //true를 반환하면 목적지에 도착 했다는 뜻.
    public bool Get_NextPos(ref int _iGoalLoadIndex, ref Vector3 _v3Pos, float _fDis)
    {
        Vector3 v3GoalPos   = m_LoadTileList[_iGoalLoadIndex].transform.position;
        float fGoalDis      = Vector3.Distance(v3GoalPos, _v3Pos);

        Vector3 v3GoalDir;
        //만약 가야하는 거리가 더 크다면
        if (fGoalDis < _fDis)
        {
            //인덱스 하나 늘려준다
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

    //플레이어 아래 타일 xy를 찾는다.
    //시작은 0,0 Tile이 0,0 Pos에 있다. 
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
