using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LuncherFunc : CMyUnityBase
{
    public  TextMeshProUGUI         m_ContentText;

    private List<TextMeshProUGUI>   m_TextList;
    private Transform               m_ContentTransform;

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

    public void Button_AddressableDown()
    {
        TextMeshProUGUI textMeshProUGUI = Instantiate(m_TextList[m_TextList.Count - 1]);
        textMeshProUGUI.text = "다운로드를 시작합니다.";
        m_TextList.Add(textMeshProUGUI);

        AddressableMng.Instance.
    }

    public void Button_AddressableSizeCheck()
    {
        TextMeshProUGUI textMeshProUGUI = Instantiate(m_TextList[m_TextList.Count - 1]);
        textMeshProUGUI.text = "Size : " + AddressableMng.Instance.fDownloadSize.ToString();
        m_TextList.Add(textMeshProUGUI);
    }
}
