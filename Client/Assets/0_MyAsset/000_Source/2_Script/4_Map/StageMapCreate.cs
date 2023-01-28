using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(StageMapCreate))]

public class StageMapCreate_Editor : Editor
{
    private int             m_iX = 0;
    private int             m_iY = 0;

    private void OnSceneGUI()
    {
        StageMapCreate cStageMapCreate = (StageMapCreate)target;
        if(cStageMapCreate.m_CreateFinish)
        {
            cStageMapCreate.Set_CreateFinish();
            return;
        }

        cStageMapCreate.Init();

        //ground 사이즈가 바뀌었거나 처음 시작이거나 해서 사이즈가 바뀌었다면
        if((m_iX != cStageMapCreate.iSizeX) || (m_iY != cStageMapCreate.iSizeY))
        {
            //cStageMapCreate.Set_DestroyTile();

            m_iX = cStageMapCreate.iSizeX;
            m_iY = cStageMapCreate.iSizeY;

            cStageMapCreate.Set_CreateTile();
        }
    }
}
#endif 

public class StageMapCreate : CMyUnityBase
{
    [Range(1,3)]
    public  int         m_StageNum = 1;

    public  bool        m_CreateFinish = false;

    private Tile[,]     m_TileArray;
    private Transform   m_GroundTransform;
    private int         m_iSizeX;
    private int         m_iSizeY; //사실은 Z
    public int iSizeX
    {
        get { return m_iSizeX; }
    }
    public int iSizeY
    {
        get { return m_iSizeY; }
    }

    protected override void Setting()
    {
        base.Setting();

        m_GroundTransform = transform.GetChild(0);

        m_iSizeX = (int)(m_GroundTransform.localScale.x + 0.1f);
        m_iSizeY = (int)(m_GroundTransform.localScale.y + 0.1f);
    }

    public void Set_CreateTile()
    {
        if (!ReferenceEquals(m_TileArray, null))
        {
            if (m_TileArray[0, 0] != null)
            {
                if (m_iSizeX == m_TileArray.GetLength(0))
                {
                    if (m_iSizeY == m_TileArray.GetLength(1))
                    {
                        return;
                    }
                }
            }
        }
        Set_DestroyTile();

        m_TileArray = new Tile[m_iSizeX, m_iSizeY];
        Tile tileTemp;

        for (int x = 0; x < m_iSizeX; x++)
        {
            for (int y = 0; y < m_iSizeY; y++)
            {
                tileTemp = Instantiate(Resources.Load<GameObject>("Tile/Tile"),transform).GetComponent<Tile>();
                tileTemp.iPosX = x;
                tileTemp.iPosY = y;
                tileTemp.m_StageNum = m_StageNum;
                tileTemp.m_TileType = Tile.ETileType.Low1; //기본 셋팅
                tileTemp.Init();

                m_TileArray[x, y] = tileTemp;
            }
        }
    }

    public void Set_DestroyTile()
    {
        if (!ReferenceEquals(m_TileArray, null))
        {
            if (m_TileArray[0, 0] != null)
            {
                int ix = m_TileArray.GetLength(0);
                int iy = m_TileArray.GetLength(1);

                for (int x = 0; x < ix; x++)
                {
                    for (int y = 0; y < iy; y++)
                    {
                        if (Application.isEditor)
                        {
                            if (m_TileArray[x, y] != null)
                            {
                                Object.DestroyImmediate(m_TileArray[x, y].gameObject);
                            }
                        }
                        else
                        {
                            Destroy(m_TileArray[x, y]);
                        }
                    }
                }
            }
        }
    }

    public void Set_CreateFinish()
    {

        if (!ReferenceEquals(m_TileArray, null))
        {
            if (m_TileArray[0, 0] != null)
            {
                GameObject gameObject = new GameObject("MapTile");
                Transform TempTransform = gameObject.transform;
                TempTransform.position = transform.position;
                TempTransform.localPosition = transform.localPosition;
                TempTransform.localScale = transform.localScale;
                TempTransform.rotation = transform.rotation;
                TempTransform.localRotation = transform.localRotation;

                int ix = m_TileArray.GetLength(0);
                int iy = m_TileArray.GetLength(1);
                for (int x = 0; x < ix; x++)
                {
                    for (int y = 0; y < iy; y++)
                    {
                        m_TileArray[x, y].transform.parent = gameObject.transform;
                    }
                }

                MapTile mapTile = gameObject.AddComponent<MapTile>();
                mapTile.Set_NumXY(ix, iy);

                Object.DestroyImmediate(this.gameObject);
            }
        }
    }
}
