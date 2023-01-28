using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(BezierCurveMaker))]

public class BezierCurveMaker_Editor : Editor
{
    private void OnSceneGUI()
    {
        BezierCurveMaker cBezierCurveMaker = (BezierCurveMaker)target;
        cBezierCurveMaker.Init();

        Transform TransformTemp;
        for (int i = 0; i < cBezierCurveMaker.iStagePosListCount; i++)
        {
            TransformTemp = cBezierCurveMaker.m_StagePosList[i];
            //� ������ ������ �� �ְ� �Ѵ� (������ �Ϲ� ������Ʈ �����̴� �� ó�� ����������)
            TransformTemp.position = Handles.PositionHandle(TransformTemp.position, Quaternion.identity);
            TransformTemp.localRotation = Handles.RotationHandle(TransformTemp.localRotation, TransformTemp.position);
            TransformTemp.localScale = Handles.ScaleHandle(TransformTemp.localScale, TransformTemp.position, TransformTemp.localRotation);
        }

        int iBezierLineNum = cBezierCurveMaker.iStagePosListCount - 1;
        int iLineCount = 10;
        for (int i = 0; i < iBezierLineNum; i++)
        {
            for (int j = 0; j < iLineCount; j++)
            {
                Vector3 v3PrePos = cBezierCurveMaker.Get_BezierCurvePos(((float)j / (float)iLineCount), i);
                Vector3 v3PostPos = cBezierCurveMaker.Get_BezierCurvePos(((float)(j + 1) / (float)iLineCount), i);
                Handles.DrawLine(v3PrePos, v3PostPos);
            }
        }
    }
}
#endif 

public class BezierCurveMaker : CMyUnityBase
{
    public  List<Transform> m_StagePosList = new List<Transform>();

    private int m_iStagePosListCount;
    public int iStagePosListCount
    {
        get { return m_iStagePosListCount; }
    }

    private Vector3 m_v3PosA = Vector3.zero;
    private Vector3 m_v3PosB = Vector3.zero;
    private Vector3 m_v3PosC = Vector3.zero;
    private Vector3 m_v3PosD = Vector3.zero;

    protected override void Setting()
    {
        base.Setting();

        m_iStagePosListCount = m_StagePosList.Count;
    }

    //��� �������� �ѹ�, ���൵(0~1)
    public Vector3 Get_BezierCurvePos(float _fValue, int _iStartStageNum, bool _bBack = false)
    {
        Vector3 v3ResultPos = Vector3.zero;

        //ABCD Pos������ �����ߴٸ� ����
        if(!Setting_BezierPosList(_iStartStageNum, _bBack))
        {
            return v3ResultPos;
        }

        Vector3 v3PosA = Vector3.Lerp(m_v3PosA, m_v3PosB, _fValue);
        Vector3 v3PosB = Vector3.Lerp(m_v3PosB, m_v3PosC, _fValue);
        Vector3 v3PosC = Vector3.Lerp(m_v3PosC, m_v3PosD, _fValue);

        Vector3 v3PosD = Vector3.Lerp(v3PosA, v3PosB, _fValue);
        Vector3 v3PosE = Vector3.Lerp(v3PosB, v3PosC, _fValue);

        v3ResultPos = Vector3.Lerp(v3PosD, v3PosE, _fValue);

        return v3ResultPos;
    }

    private bool Setting_BezierPosList(int _iStartStageNum, bool _bBack)
    {
        //B�� C�� A�� D�� z�� ����� �����Ϸ� �������ش�
        Transform TransformTemp;
        //�ڷ� ���� �ϴ� �κ��̶��
        if(_bBack)
        {
            //�ڷ� ���� �ϴµ� 0���� �۰ų� ���ٸ�? (���� ���� ����)
            if (_iStartStageNum <= 0)
            {
                return false;
            }

            TransformTemp = m_StagePosList[_iStartStageNum - 1];
            m_v3PosA = TransformTemp.position;
            m_v3PosB = (TransformTemp.forward * TransformTemp.localScale.z) + m_v3PosA;

            TransformTemp = m_StagePosList[_iStartStageNum];
            m_v3PosD = TransformTemp.position;
            m_v3PosC = (TransformTemp.forward * TransformTemp.localScale.z) + m_v3PosD;
        }
        else
        {
            //������ ���� �ϴµ� ����Ʈ �ִ� �������� ũ�ų� ���ٸ�? (���� ���� ����)
            if(_iStartStageNum >= m_iStagePosListCount)
            {
                return false;
            }

            TransformTemp = m_StagePosList[_iStartStageNum];
            m_v3PosA = TransformTemp.position;
            m_v3PosB = (TransformTemp.forward * TransformTemp.localScale.z) + m_v3PosA;

            TransformTemp = m_StagePosList[_iStartStageNum + 1];
            m_v3PosD = TransformTemp.position;
            m_v3PosC = (TransformTemp.forward * TransformTemp.localScale.z) + m_v3PosD;
        }

        return true;
    }
}
