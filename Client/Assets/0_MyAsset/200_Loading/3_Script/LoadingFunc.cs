using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoadingFunc : CMyUnityBase
{
    public float            m_LoadingIconSpeed      = 150f;
    public float            m_LoadingTextChangeTime = 0.5f;
    public Transform        m_LoadingIconTransform;
    public TextMeshProUGUI  m_LoadingText;

    private Vector3 m_v3LoadingIconRotateDir    = new Vector3(0f,0f,-1f);
    private float   m_fTime                     = 0f; //���� �ð� ��� �� ����

    enum ELoadingTextType
    {
        NoDot,
        Dot1,
        Dot2,
        Dot3,
        ELoadingTextType_End
    }
    private ELoadingTextType m_eLoadingText = ELoadingTextType.ELoadingTextType_End;

    private void Update()
    {
        //�ε� ������ ȸ��
        m_LoadingIconTransform.Rotate(m_v3LoadingIconRotateDir, m_LoadingIconSpeed * Time.deltaTime);

        //�ε� �ؽ�Ʈ ��ȯ
        m_fTime += Time.deltaTime;

        if(m_fTime > m_LoadingTextChangeTime)
        {
            switch (m_eLoadingText)
            {
                case ELoadingTextType.NoDot:
                    m_eLoadingText = ELoadingTextType.Dot1;
                    m_LoadingText.text = "Loading.";
                    break;
                case ELoadingTextType.Dot1:
                    m_eLoadingText = ELoadingTextType.Dot2;
                    m_LoadingText.text = "Loading..";
                    break;
                case ELoadingTextType.Dot2:
                    m_eLoadingText = ELoadingTextType.Dot3;
                    m_LoadingText.text = "Loading...";
                    break;
                case ELoadingTextType.Dot3:
                    m_eLoadingText = ELoadingTextType.NoDot;
                    m_LoadingText.text = "Loading";
                    break;
                case ELoadingTextType.ELoadingTextType_End:
                    m_eLoadingText = ELoadingTextType.NoDot;
                    m_LoadingText.text = "Loading";
                    break;
            }
            m_fTime = 0f;
        }
    }
}
