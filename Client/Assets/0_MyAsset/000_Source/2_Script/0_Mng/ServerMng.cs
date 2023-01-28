using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using System.Net.Sockets;
using System.Text;

public class ServerMng : MonoBehaviour
{
    static private ServerMng m_Instance = null;

    static public ServerMng Instance
    {
        get
        {
            if (ReferenceEquals(m_Instance, null))
            {
                GameObject instance = new GameObject();
                m_Instance          = instance.AddComponent<ServerMng>();
                m_Instance.Init();
                DontDestroyOnLoad(instance);
            }
            return m_Instance;
        }
    }

    //�����ڸ� private�� ����� �ܺο��� ���� ����
    private ServerMng() { }
    ////////////////////////////////////////////////////////////////////
    //������ Ÿ�� �״�� ���������� ������ �� (Success)
    //public enum EDataType : byte
    //{
    //    CreateAccount,
    //    NoOverlapID,
    //    Login,
    //    RandomInt,
    //    Chat,
    //    Error = 16,
    //    OverlapID,
    //    Login_IDWrong,
    //    Login_PWWrong,
    //    EDataType_End
    //}

    public enum EDataType : byte
    {
        None = 0, //0x0000 0000
        CreateAccount,
        NoOverlapID,
        Login,
        RandomInt,
        Chat,
        Error = 16, //0x0001 0000
        OverlapID,
        Login_IDWrong,
        Login_PWWrong,
        EDataType_End = 255 //0x1111 1111
    }

    public enum EClearbyteDataArrayType
    {
        Server,
        Temp,
        Both,
        EClearbyteDataArrayType_End
    }

    private TcpClient   m_cTcpClient;
    private int         m_iServerPort       = 8880; //������ ������ ��Ʈ
    private int         m_iDataArrayLength  = 1024; //���� �� ����
    private byte[]      m_byteServerDataArray;      //������ ������ �޴� ������
    private byte[]      m_byteTempDataArray;        //������ ������ �� �� �տ� Ÿ���� ���� ����, ������ �ִ� ������

    public byte[] ByteServerDataArray
    {
        get
        {
            return m_byteServerDataArray;
        }
    }

    public  delegate void ServerDelegate();
    private ServerDelegate  m_delegateServer;
    private bool            m_bNowServerConnecting = false;

    public void Init()
    {
        ClientConnect();
    }

    private void ClientConnect()
    {
        m_cTcpClient = new TcpClient();
        // ù��° �Ű������� ������ IP
        // �ι�° �Ű������� �������� ������ ��Ʈ��ȣ �� �Է����ݴϴ�.
        //m_cTcpClient.Connect("127.0.0.1", m_iServerPort); //���� ������ �� ������ IP ���
        m_cTcpClient.Connect("127.0.0.1", m_iServerPort);

        m_byteServerDataArray = new byte[m_iDataArrayLength];
        m_byteTempDataArray = new byte[(m_iDataArrayLength - 1)];
    }

    private void ClientClose()
    {
        m_cTcpClient.Close();
    }

    private void DataCopy_TempToSendArray(EDataType _eSendDataType)
    {
        //���� �� �������� Ÿ��
        m_byteServerDataArray[0] = (byte)_eSendDataType;
        //Temp�����͸� Send�� ī��
        Buffer.BlockCopy(m_byteTempDataArray, 0, m_byteServerDataArray, 1, m_byteTempDataArray.Length);
    }

    private void SendToServer()
    {
        //������ ����
        m_cTcpClient.GetStream().Write(m_byteServerDataArray, 0, m_iDataArrayLength);

        Clear_byteDataArray(EClearbyteDataArrayType.Both);
    }

    private void Clear_byteDataArray(EClearbyteDataArrayType _eClearbyteDataArrayType)
    {
        switch (_eClearbyteDataArrayType)
        {
            case EClearbyteDataArrayType.Server:
                Array.Clear(m_byteServerDataArray, 0, m_iDataArrayLength);
                m_byteServerDataArray[0] = (byte)EDataType.EDataType_End;
                break;
            case EClearbyteDataArrayType.Temp:
                Array.Clear(m_byteTempDataArray, 0, m_byteTempDataArray.Length);
                m_byteTempDataArray[0] = (byte)EDataType.EDataType_End;
                break;
            case EClearbyteDataArrayType.Both:
                Array.Clear(m_byteServerDataArray, 0, m_iDataArrayLength);
                m_byteServerDataArray[0] = (byte)EDataType.EDataType_End;
                Array.Clear(m_byteTempDataArray, 0, m_byteTempDataArray.Length);
                m_byteTempDataArray[0] = (byte)EDataType.EDataType_End;
                break;
            case EClearbyteDataArrayType.EClearbyteDataArrayType_End:
                break;
        }
    }

    private void DataWrite_TempArray(int _i)
    {
        m_byteTempDataArray = BitConverter.GetBytes(_i);
    }
    private void DataWrite_TempArray(int _i1,int _i2)
    {
        //������ ������ �����װ�, 100�� ���� ���� �״ϱ�..
        //�������� unsigned�� 4ĭ�� �ִ°� �°�����... 4����Ʈ�� 1��Ƽ���ϱ�
        if (_i1 < 100)
        {
            m_byteTempDataArray[0] = BitConverter.GetBytes(_i1)[0];
        }
        if (_i2 < 100)
        {
            m_byteTempDataArray[1] = BitConverter.GetBytes(_i2)[0];
        }
    }
    private void DataWrite_TempArray(string _str)
    {
        m_byteTempDataArray = Encoding.UTF8.GetBytes(_str);
    }

    //string1 + _strMiddle + _str2 �� �ȴ�
    private void DataWrite_TempArray(string _str1, string _str2,string _strMiddle)
    {
        string strTemp = _str1 + _strMiddle + _str2;
        m_byteTempDataArray = Encoding.UTF8.GetBytes(strTemp);
    }

    //���� ��� �õ��� �����ϸ� true, _delegateServer�� �����͸� �������� �Լ��� �ִ´�
    public bool Set_RandomInt(int _iMin, int _iMax, ServerDelegate _delegateServerReturn)
    {
        //������ ������̶��
        if(m_bNowServerConnecting)
        {
            return false; //����
        }
        else 
        {
            //�ڷ�ƾ�� ������ �ҷ��� �Լ�
            m_delegateServer += _delegateServerReturn;
            //���� �� ������ ����
            DataWrite_TempArray(_iMin, _iMax);
            //������ Ÿ�Ա��� �ۼ� �Ϸ�, ���� �غ� ��
            DataCopy_TempToSendArray(EDataType.RandomInt);
            StartCoroutine(Set_Server());
            return true; //�õ�
        }
    }

    //���̵�, ��й�ȣ, �������� ���� ���� �� ������� �� �Լ�
    public bool Set_CreateID(string _strID, string _strPW, ServerDelegate _delegateServerReturn)
    {
        //�̹� ������ ������̶��
        if (m_bNowServerConnecting)
        {
            return false; //����
        }
        else
        {
            //�ڷ�ƾ�� ������ �ҷ��� �Լ�
            m_delegateServer += _delegateServerReturn;
            //���� �� ������ ����
            DataWrite_TempArray(_strID, _strPW, ":");
            //������ Ÿ�Ա��� �ۼ� �Ϸ�, ���� �غ� ��
            DataCopy_TempToSendArray(EDataType.CreateAccount);
            StartCoroutine(Set_Server());
            return true; //�õ�
        }
    }

    //�ߺ��� ���̵� �ִ��� Ȯ��
    public bool Set_Check_OverlapID(string _strID, ServerDelegate _delegateServerReturn)
    {
        //�̹� ������ ������̶��
        if (m_bNowServerConnecting)
        {
            return false; //����
        }
        else
        {
            //�ڷ�ƾ�� ������ �ҷ��� �Լ�
            m_delegateServer += _delegateServerReturn;
            //���� �� ������ ����
            DataWrite_TempArray(_strID);
            //������ Ÿ�Ա��� �ۼ� �Ϸ�, ���� �غ� ��
            DataCopy_TempToSendArray(EDataType.OverlapID);
            StartCoroutine(Set_Server());
            return true; //�õ�
        }
    }

    public bool Set_Login(string _strID, string _strPW, ServerDelegate _delegateServerReturn)
    {
        //�̹� ������ ������̶��
        if (m_bNowServerConnecting)
        {
            return false; //����
        }
        else
        {
            //�ڷ�ƾ�� ������ �ҷ��� �Լ�
            m_delegateServer += _delegateServerReturn;
            //���� �� ������ ����
            DataWrite_TempArray(_strID, _strPW, ":");
            //������ Ÿ�Ա��� �ۼ� �Ϸ�, ���� �غ� ��
            DataCopy_TempToSendArray(EDataType.Login);
            StartCoroutine(Set_Server());
            return true; //�õ�
        }
    }

    private IEnumerator Set_Server()
    {
        SendToServer();
        while(true)
        {
            if(m_cTcpClient.GetStream().DataAvailable)
            {
                m_cTcpClient.GetStream().Read(m_byteServerDataArray);
                //������ �޾Ƴ����� �������� �Լ� ���� , �����Ų Ŭ�������� �˾Ƽ� ó���ض�
                m_delegateServer();
                //������ Ŭ����
                Clear_byteDataArray(EClearbyteDataArrayType.Both);
                //��������Ʈ �ʱ�ȭ
                m_delegateServer = null;
                yield break;
            }
            yield return null;
        }
    }

    //������ Ÿ��
    public EDataType Get_byteServerDataType()
    {
        return (EDataType)m_byteServerDataArray[0];
    }

    //���ϴ� �ε����� �Ѱ���, ������ Ÿ�� ������ �迭
    public int Get_byteServerData(int _iArrIndex)
    {
        return (int)m_byteServerDataArray[_iArrIndex];
    }

    //Ÿ���� �����ϰ� �����͸� �Ѱ��ִ� �Լ�
    public byte[] Get_OnlyData()
    {
        byte[] byteArray = new byte[m_byteServerDataArray.Length - 1];
        Buffer.BlockCopy(m_byteServerDataArray, 1, byteArray, 0, byteArray.Length);
        return byteArray;
    }
}
