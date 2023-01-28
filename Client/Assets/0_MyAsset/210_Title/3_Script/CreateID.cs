using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CreateID : MonoBehaviour
{
    public TextMeshProUGUI m_InfoText;
    public TMP_InputField   m_InputField_ID;
    public TMP_InputField   m_InputField_PW;
    public TMP_InputField   m_InputField_PW2;

    public GameObject m_LoginUI;

    private readonly string m_strCanCreateID = "Can Create This ID";

    private void OnEnable()
    {
        Init();
    }


    public void Init()
    {
        m_InfoText.text = "";
        m_LoginUI.SetActive(false);
    }

    public void Set_ButtonPush_CreateID()
    {
        if(m_InfoText.text != m_strCanCreateID)
        {
            m_InfoText.text = "Check OverlapID";
            return;
        }

        //비밀번호가 비밀번호 확인과 일치하다면
        if(m_InputField_PW.text == m_InputField_PW2.text)
        {
            m_InfoText.text = "";
            ServerMng.Instance.Set_CreateID(m_InputField_ID.text, m_InputField_PW.text, Set_CreateID_Result);
        }
        else
        {
            m_InfoText.text = "PW2 Miss Maching";
        }
    }

    public void Set_CreateID_Result()
    {
        ServerMng.EDataType eSendDataType = ServerMng.Instance.Get_byteServerDataType();
        switch (eSendDataType)
        {
            case ServerMng.EDataType.CreateAccount:
                m_InfoText.text = "ID Create Success";
                break;
            case ServerMng.EDataType.Error:
                m_InfoText.text = "Error";
                break;
            case ServerMng.EDataType.OverlapID:
                m_InfoText.text = "OverlapID";
                break;
            case ServerMng.EDataType.EDataType_End:
                m_InfoText.text = "???";
                break;
        }
    }

    public void Set_ButtonPush_Exit()
    {
        m_LoginUI.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Set_ButtonPush_Check_OverlapID()
    {
        ServerMng.Instance.Set_Check_OverlapID(m_InputField_ID.text, Set_Check_OverlapID_Result);
    }

    public void Set_Check_OverlapID_Result()
    {
        ServerMng.EDataType eSendDataType = ServerMng.Instance.Get_byteServerDataType();
        switch (eSendDataType)
        {
            case ServerMng.EDataType.NoOverlapID:
                m_InfoText.text = m_strCanCreateID;
                break;
            case ServerMng.EDataType.OverlapID:
                m_InfoText.text = "OverlapID";
                break;
            case ServerMng.EDataType.EDataType_End:
                m_InfoText.text = "???";
                break;
        }
    }
}
