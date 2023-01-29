using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Login : MonoBehaviour
{
    public TMP_InputField   m_InputField_ID;
    public TMP_InputField   m_InputField_PW;
    public TextMeshProUGUI  m_Text_LoginInfo;

    public GameObject m_CreateAccountUI;

    private void OnEnable()
    {
        Init();
    }

    public void Init()
    {
        m_InputField_ID.text    = "";
        m_InputField_PW.text    = "";
        m_Text_LoginInfo.text   = "";

        m_CreateAccountUI.SetActive(false);
    }

    public void Set_ButtonPush_Login()
    {
        ServerMng.Instance.Set_Login(m_InputField_ID.text, m_InputField_PW.text, Set_Login_Result);
    }

    public void Set_Login_Result()
    {
        ServerMng.EDataType eSendDataType = ServerMng.Instance.Get_byteServerDataType();
        switch (eSendDataType)
        {
            case ServerMng.EDataType.Login:
                m_Text_LoginInfo.text = "Login Success";
                break;
            case ServerMng.EDataType.Error:
                m_Text_LoginInfo.text = "Error";
                break;
            case ServerMng.EDataType.Login_IDWrong:
                m_Text_LoginInfo.text = "ID No Exist";
                break;
            case ServerMng.EDataType.Login_PWWrong:
                m_Text_LoginInfo.text = "PW Wrong";
                break;
            case ServerMng.EDataType.EDataType_End:
                break;
        }
    }

    public void Set_ButtonPush_CreateAccount()
    {
        m_CreateAccountUI.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Button_NextScene()
    {
        SceneMng.Instance.Set_LoadSceneAsync(ESceneType.InGame_Base);
    }

}
