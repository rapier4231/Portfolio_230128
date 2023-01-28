using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LuncherFunc : CMyUnityBase
{
    //���� �ؼ� ��� �� ù��° �ؽ�Ʈ
    public  TextMeshProUGUI         m_ContentText;

    //�ؽ�Ʈ�� ��� �� ��
    private List<TextMeshProUGUI>   m_TextList;
    private Transform               m_ContentTransform;

    //�ٿ�ε�� ��ư ���ֱ�
    public GameObject m_AddressableSizeCheckButton;

    //Loadingbar
    public Image m_LoadingbarImage;

    public void Button_AddressableDown()
    {
        CreateText("�ٿ�ε带 �����մϴ�.");
        //������ ��ư ���ֱ�
        m_AddressableSizeCheckButton.SetActive(false);
        AddressableMng.Instance.AddressableDownload();
    }

    public void Button_AddressableSizeCheck()
    {
        CreateText("DownloadSize : " + AddressableMng.Instance.fClientDownloadSize.ToString() + " / " + AddressableMng.Instance.fServerDownloadSize.ToString());
    }

    //////////////////////////////////////////////
    private void Update()
    {
        float fServerDownloadSize = AddressableMng.Instance.fServerDownloadSize;
        if (fServerDownloadSize > 0f)
        {
            m_LoadingbarImage.fillAmount = AddressableMng.Instance.fClientDownloadSize / fServerDownloadSize;
        }
    }

    //////////////////////////////////////////////

    protected override void NullCheck()
    {
        base.NullCheck();

        if(m_ContentText == null)
        {
            GameObject.Find("LuncherText");
            Debug.Log("�ν����Ϳ� LuncherText �� ���� ������~ - LuncherFunc");
        }

        if (m_AddressableSizeCheckButton == null)
        {
            GameObject.Find("AddressableDownloadSizeCheckButton");
            Debug.Log("�ν����Ϳ� AddressableDownloadSizeCheckButton �� ���� ������~ - LuncherFunc");
        }

        if(m_LoadingbarImage == null)
        {
            GameObject.Find("Loadingbar_Red");
            Debug.Log("�ν����Ϳ� Loadingbar_Red �� ���� ������~ - LuncherFunc");

        }
    }

    protected override void NewCreate()
    {
        base.NewCreate();

        m_TextList = new List<TextMeshProUGUI>();
    }

    protected override void Setting()
    {
        base.Setting();

        m_TextList.Add(m_ContentText);
        m_ContentTransform = m_ContentText.transform.parent;
    }

    //��ó �ؽ�Ʈ â�� �� ����ִ� �Լ�
    private void CreateText(string _strText)
    {
        TextMeshProUGUI textMeshProUGUI = Instantiate(m_TextList[m_TextList.Count - 1], m_ContentTransform);
        textMeshProUGUI.text = _strText;
        m_TextList.Add(textMeshProUGUI);
    }
}
