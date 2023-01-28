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

    //생성자를 private로 만들어 외부에서 생성 제한
    private ServerMng() { }
    ////////////////////////////////////////////////////////////////////
    //데이터 타입 그대로 돌려받으면 성공한 것 (Success)
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
    private int         m_iServerPort       = 8880; //서버와 연결할 포트
    private int         m_iDataArrayLength  = 1024; //보낼 총 길이
    private byte[]      m_byteServerDataArray;      //서버로 보내고 받는 데이터
    private byte[]      m_byteTempDataArray;        //서버로 보내기 전 맨 앞에 타입을 넣지 않은, 정보만 있는 데이터

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
        // 첫번째 매개변수는 접속할 IP
        // 두번째 매개변수는 서버에서 설정한 포트번호 를 입력해줍니다.
        //m_cTcpClient.Connect("127.0.0.1", m_iServerPort); //내가 서버일 때 루프백 IP 사용
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
        //전송 할 데이터의 타입
        m_byteServerDataArray[0] = (byte)_eSendDataType;
        //Temp데이터를 Send에 카피
        Buffer.BlockCopy(m_byteTempDataArray, 0, m_byteServerDataArray, 1, m_byteTempDataArray.Length);
    }

    private void SendToServer()
    {
        //서버에 전송
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
        //음수가 들어오진 않을테고, 100을 넘지 않을 테니까..
        //정석으론 unsigned에 4칸씩 주는게 맞겟지만... 4바이트가 1인티저니까
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

    //string1 + _strMiddle + _str2 가 된다
    private void DataWrite_TempArray(string _str1, string _str2,string _strMiddle)
    {
        string strTemp = _str1 + _strMiddle + _str2;
        m_byteTempDataArray = Encoding.UTF8.GetBytes(strTemp);
    }

    //서버 통신 시도에 성공하면 true, _delegateServer는 데이터를 가져가는 함수를 넣는다
    public bool Set_RandomInt(int _iMin, int _iMax, ServerDelegate _delegateServerReturn)
    {
        //서버랑 통신중이라면
        if(m_bNowServerConnecting)
        {
            return false; //실패
        }
        else 
        {
            //코루틴이 끝나고 불러줄 함수
            m_delegateServer += _delegateServerReturn;
            //전송 할 데이터 저장
            DataWrite_TempArray(_iMin, _iMax);
            //데이터 타입까지 작성 완료, 보낼 준비 끝
            DataCopy_TempToSendArray(EDataType.RandomInt);
            StartCoroutine(Set_Server());
            return true; //시도
        }
    }

    //아이디, 비밀번호, 서버에서 답이 왔을 때 실행시켜 줄 함수
    public bool Set_CreateID(string _strID, string _strPW, ServerDelegate _delegateServerReturn)
    {
        //이미 서버랑 통신중이라면
        if (m_bNowServerConnecting)
        {
            return false; //실패
        }
        else
        {
            //코루틴이 끝나고 불러줄 함수
            m_delegateServer += _delegateServerReturn;
            //전송 할 데이터 저장
            DataWrite_TempArray(_strID, _strPW, ":");
            //데이터 타입까지 작성 완료, 보낼 준비 끝
            DataCopy_TempToSendArray(EDataType.CreateAccount);
            StartCoroutine(Set_Server());
            return true; //시도
        }
    }

    //중복된 아이디가 있는지 확인
    public bool Set_Check_OverlapID(string _strID, ServerDelegate _delegateServerReturn)
    {
        //이미 서버랑 통신중이라면
        if (m_bNowServerConnecting)
        {
            return false; //실패
        }
        else
        {
            //코루틴이 끝나고 불러줄 함수
            m_delegateServer += _delegateServerReturn;
            //전송 할 데이터 저장
            DataWrite_TempArray(_strID);
            //데이터 타입까지 작성 완료, 보낼 준비 끝
            DataCopy_TempToSendArray(EDataType.OverlapID);
            StartCoroutine(Set_Server());
            return true; //시도
        }
    }

    public bool Set_Login(string _strID, string _strPW, ServerDelegate _delegateServerReturn)
    {
        //이미 서버랑 통신중이라면
        if (m_bNowServerConnecting)
        {
            return false; //실패
        }
        else
        {
            //코루틴이 끝나고 불러줄 함수
            m_delegateServer += _delegateServerReturn;
            //전송 할 데이터 저장
            DataWrite_TempArray(_strID, _strPW, ":");
            //데이터 타입까지 작성 완료, 보낼 준비 끝
            DataCopy_TempToSendArray(EDataType.Login);
            StartCoroutine(Set_Server());
            return true; //시도
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
                //데이터 받아놨으니 가져가는 함수 실행 , 실행시킨 클래스에서 알아서 처리해라
                m_delegateServer();
                //데이터 클리어
                Clear_byteDataArray(EClearbyteDataArrayType.Both);
                //델리게이트 초기화
                m_delegateServer = null;
                yield break;
            }
            yield return null;
        }
    }

    //데이터 타입
    public EDataType Get_byteServerDataType()
    {
        return (EDataType)m_byteServerDataArray[0];
    }

    //원하는 인덱스만 넘겨줌, 데이터 타입 포함한 배열
    public int Get_byteServerData(int _iArrIndex)
    {
        return (int)m_byteServerDataArray[_iArrIndex];
    }

    //타입은 제외하고 데이터만 넘겨주는 함수
    public byte[] Get_OnlyData()
    {
        byte[] byteArray = new byte[m_byteServerDataArray.Length - 1];
        Buffer.BlockCopy(m_byteServerDataArray, 1, byteArray, 0, byteArray.Length);
        return byteArray;
    }
}
