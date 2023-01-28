using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LuncherFunc : CMyUnityBase
{
    //복사 해서 사용 할 첫번째 텍스트
    public  TextMeshProUGUI         m_ContentText;

    //텍스트들 담아 둘 곳
    private List<TextMeshProUGUI>   m_TextList;
    private Transform               m_ContentTransform;

    //다운로드시 버튼 없애기
    public GameObject m_AddressableSizeCheckButton;

    //Loadingbar
    public Image m_LoadingbarImage;

    public void Button_AddressableDown()
    {
        CreateText("다운로드를 시작합니다.");
        //사이즈 버튼 없애기
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
            Debug.Log("인스펙터에 LuncherText 안 껴져 있으요~ - LuncherFunc");
        }

        if (m_AddressableSizeCheckButton == null)
        {
            GameObject.Find("AddressableDownloadSizeCheckButton");
            Debug.Log("인스펙터에 AddressableDownloadSizeCheckButton 안 껴져 있으요~ - LuncherFunc");
        }

        if(m_LoadingbarImage == null)
        {
            GameObject.Find("Loadingbar_Red");
            Debug.Log("인스펙터에 Loadingbar_Red 안 껴져 있으요~ - LuncherFunc");

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

    //런처 텍스트 창에 글 띄워주는 함수
    private void CreateText(string _strText)
    {
        TextMeshProUGUI textMeshProUGUI = Instantiate(m_TextList[m_TextList.Count - 1], m_ContentTransform);
        textMeshProUGUI.text = _strText;
        m_TextList.Add(textMeshProUGUI);
    }
}
