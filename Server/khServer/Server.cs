using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using khDB;

namespace khServer
{
    class CServer
    {
        public enum EDataType : byte
        {
            CreateAccount,
            NoOverlapID,
            Login,
            RandomInt,
            Chat,
            Error = 16,
            OverlapID,
            Login_IDWrong,
            Login_PWWrong,
            EDataType_End
        }

        private bool m_bThreadOn = true;

        private string m_strIPBackWebsite = "http://icanhazip.com";
        private string m_strServerIP;

        //클라와 연결할 포트 넘버
        private readonly int m_iPort = 8880;
        private TcpListener m_cTcpListener;

        //클라와 데이터를 주고받을 양 (Byte 갯수)
        private readonly int m_iDataArrayLength = 1024;
        private Thread m_cServerThread;

        public void Init()
        {
            //외부 IP를 사이트를 통해 알아내어 m_strServerIP에 저장
            //외부(공공)IP의 경우, DHCP에 의해 주기적으로 재할당이 일어나기 때문에 (NAT로 받는 경우 더더욱)
            //지속적인 서비스를 위해서는 고정 IP를 따로 할당 받거나 클라우드 서비스를 이용하는 것이 바람직하다.
            Find_ServerIP();

            Create_TCPListener();

            //쓰레드 생성, 서버는 쓰레드에서 돌려준다
            Create_Thread();
        }

        public void End()
        {
            //쓰레드 작업을 메인쓰레드가 기다렸다가 종료
            m_bThreadOn = false;
            m_cServerThread.Join();
            //쓰레드 강제종료
            //m_cServerThread.Abort()

        }

        //외부 IP 찾기
        private void Find_ServerIP()
        {
            string strExternalIp = new WebClient().DownloadString(m_strIPBackWebsite).Replace("\\r\\n", "").Replace("\\n", "").Trim();
            IPAddress cExternalIpAddress = IPAddress.Parse(strExternalIp);
            m_strServerIP = cExternalIpAddress.ToString();
        }

        //TCP통신용 리스너 생성
        private void Create_TCPListener()
        {
            //첫번째 인자는 IP, 두번째는 port 번호
            m_cTcpListener = new TcpListener(IPAddress.Any, m_iPort);
            //서버 개시
            m_cTcpListener.Start();
        }

        //쓰레드 생성 후 시작
        private void Create_Thread()
        {
            m_cServerThread = new Thread(() => Server_ThreadingFunc());
            m_bThreadOn = true;
            m_cServerThread.Start();
        }

        //쓰레드에서 돌고있는 함수
        private void Server_ThreadingFunc()
        {
            //클라 객체를 만들어서 m_iPort에 연결한 클라를 받아옴, 클라가 접속해야 다음 코드라인으로 넘어감
            TcpClient cTcpClient = m_cTcpListener.AcceptTcpClient();

            //클라에서 보낸 데이터를 받는 객체
            NetworkStream cNetworkStream = cTcpClient.GetStream();

            byte[] byteDataArray = new byte[m_iDataArrayLength];

            //End()가 불리기 전까지 계속 도는 구문
            while (m_bThreadOn)
            {
                //클라가 보낸 데이터가 arrByteData에 들어온다
                //읽을 데이터가 없다면 해당 코드라인에서 대기한다
                cNetworkStream.Read(byteDataArray, 0, m_iDataArrayLength);

                //첫번째 배열 데이터로 데이터 종류 판별
                EDataType eDataType = (EDataType)byteDataArray[0];

                //서버에서 어떤 데이터를 보냈는지 분기
                switch (eDataType)
                {
                    case EDataType.CreateAccount:
                        //CDBConnector 클래스를 통해 db에 추가하는 함수
                        DataProcessing_CreateId(cNetworkStream,byteDataArray);
                        break;
                    case EDataType.Login:
                        //CDBConnector 클래스를 통해 db에서 확인하는 함수
                        DataProcessing_Login(cNetworkStream,byteDataArray);
                        break;
                    case EDataType.Chat:
                        //후가공한 데이터를 sting으로 가공해주는 함수
                        DataProcessing_String(byteDataArray);
                        break;
                    case EDataType.RandomInt:
                        //받아온 배열의 1과 2 사이의 숫자를 3에 넣어주는 함수
                        DataProcessing_RandomInt(cNetworkStream, byteDataArray);
                        break;
                    case EDataType.OverlapID:
                        //후가공한 데이터를 sting으로 가공해주는 함수
                        DataProcessing_OverlapID(cNetworkStream, byteDataArray);
                        break;
                    case EDataType.EDataType_End:
                        break;
                }
                Array.Clear(byteDataArray, 0, byteDataArray.Length);
            }
        }

        //받은 데이터를 안쓰는 부분을 제거하고 사용하는 부분만 새로운 배열에 제공
        private void DataRenewal(byte[] _byteSrcArray, out byte[] _byteRenewalDataArray)
        {
            int iEmptyCheck = 0; //받은 데이터 후가공 ( Null인 부분 제외 )
            int iDataUseingZeroCheck = 0; //받은 데이터 중간에 Null이 있는 경우도 있으므로 체크
            int iDataZeroCheckNumber = 3; //(3배열만 체크하겠다)

            //1부터 하는 이유 - 0번째 배열은 데이터 타입 체크 용이므로
            for (int i = 1; i < m_iDataArrayLength; ++i)
            {
                //데이터가 없다?
                if (_byteSrcArray[i] == 0)
                {
                    //데이터가 중간에 Null이 껴있는 걸 감안해 iDataZeroCheckNumber수만큼 확인해줬지만 계속 null 연속
                    if (iDataUseingZeroCheck == iDataZeroCheckNumber)
                    {
                        break;
                    }
                    ++iDataUseingZeroCheck;
                }

                //데이터가 있는 배열로 판단하겠다
                ++iEmptyCheck;
            }

            int iUseArrayCount = iEmptyCheck - iDataUseingZeroCheck;

            _byteRenewalDataArray = new byte[iUseArrayCount];

            //데이터 타입을 알려주는 1번 배열을 제외하고 Null(데이터가 끝난 다음 빈 배열) 나오기 전까지 복사한다
            Buffer.BlockCopy(_byteSrcArray, 1, _byteRenewalDataArray, 0, iUseArrayCount);
            //받은 데이터는 클리어 해준다
            System.Array.Clear(_byteSrcArray, 0, _byteSrcArray.Length);
        }

        private void DataProcessing_CreateId(NetworkStream cNetworkStream,byte[] _byteDataArray)
        {
            //맨 앞 데이터 종류와 뒤쪽의 빈 공간을 제외한, 진짜 데이터만 남도록 후가공
            byte[] byteRenewalDataArray;
            DataRenewal(_byteDataArray, out byteRenewalDataArray);

            //클라에서 보내주는 형식은 id:password 형식, :를 찾아서 앞뒤로 자른다
            string strIdPwd = Encoding.UTF8.GetString(byteRenewalDataArray);

            int iIndex = strIdPwd.LastIndexOf(':');
            string strId = strIdPwd.Substring(0, iIndex);  //0번 인덱스 부터 iIndex 전 까지
            string strPwd = strIdPwd.Substring(iIndex + 1); //끝까지 자를 땐 2번째 인자 생략 가능

            //아이디를 생성한다
            EDataType eResultDataType = CMain.Get_DBConnector().Set_CreateId(strId, strPwd);

            Array.Clear(_byteDataArray, 0, _byteDataArray.Length);
            _byteDataArray[0] = (byte)eResultDataType;
            cNetworkStream.Write(_byteDataArray, 0, m_iDataArrayLength);
            Array.Clear(_byteDataArray, 0, _byteDataArray.Length);
        }

        private void DataProcessing_Login(NetworkStream cNetworkStream, byte[] _byteDataArray)
        {
            //맨 앞 데이터 종류와 뒤쪽의 빈 공간을 제외한, 진짜 데이터만 남도록 후가공
            byte[] byteRenewalDataArray;
            DataRenewal(_byteDataArray, out byteRenewalDataArray);

            //클라에서 보내주는 형식은 id:password 형식, :를 찾아서 앞뒤로 자른다
            string strIdPwd = Encoding.UTF8.GetString(byteRenewalDataArray);

            int iIndex = strIdPwd.LastIndexOf(':');
            string strId = strIdPwd.Substring(0, iIndex);  //0번 인덱스 부터 iIndex 전 까지
            string strPwd = strIdPwd.Substring(iIndex + 1); //끝까지 자를 땐 2번째 인자 생략 가능

            //로그인에 대한 결과를 받아온다
            EDataType eResultDataType = CMain.Get_DBConnector().Get_CheckLogin(strId, strPwd);
            Array.Clear(_byteDataArray, 0, _byteDataArray.Length);
            _byteDataArray[0] = (byte)eResultDataType;
            cNetworkStream.Write(_byteDataArray, 0, m_iDataArrayLength);
            Array.Clear(_byteDataArray, 0, _byteDataArray.Length);
        }

        //데이터를 String으로 변환
        private string DataProcessing_String(byte[] _byteDataArray)
        {
            //맨 앞 데이터 종류와 뒤쪽의 빈 공간을 제외한, 진짜 데이터만 남도록 후가공
            byte[] byteRenewalDataArray;
            DataRenewal(_byteDataArray, out byteRenewalDataArray);

            string strChat = Encoding.UTF8.GetString(byteRenewalDataArray);

            return strChat;
        }

        //후가공(리뉴얼) 하지 않고 기본의 데이터를 받아온다
        private void DataProcessing_RandomInt(NetworkStream cNetworkStream, byte[] _byteDataArray)
        {
            Random cRandom = new Random();
            //타입을 알려주는 0번째를 제외한, 첫번째 배열부터 두번째 배열 값 사이 중 하나를 랜덤으로 선택
            int iRandom = cRandom.Next(_byteDataArray[1], _byteDataArray[2]);

            //세번째 배열에 랜덤으로 나온 값을 넣어준다. (사실 4번째 임)
            _byteDataArray[3] = (byte)iRandom;

            cNetworkStream.Write(_byteDataArray, 0, m_iDataArrayLength);
            Array.Clear(_byteDataArray, 0, _byteDataArray.Length);
        }

        private void DataProcessing_OverlapID(NetworkStream cNetworkStream, byte[] _byteDataArray)
        {
            //맨 앞 데이터 종류와 뒤쪽의 빈 공간을 제외한, 진짜 데이터만 남도록 후가공
            byte[] byteRenewalDataArray;
            DataRenewal(_byteDataArray, out byteRenewalDataArray);

            //클라에서 보내주는 형식은 id:password 형식, :를 찾아서 앞뒤로 자른다
            string strId = Encoding.UTF8.GetString(byteRenewalDataArray);

            //아이디가 존재하면 true를 반환한다
            EDataType eResultDataType = EDataType.EDataType_End;
            if (CMain.Get_DBConnector().Check_ExistID(strId))
            {
                eResultDataType = EDataType.OverlapID;
            }
            else
            {
                eResultDataType = EDataType.NoOverlapID;
            }
            Array.Clear(_byteDataArray, 0, _byteDataArray.Length);
            _byteDataArray[0] = (byte)eResultDataType;
            cNetworkStream.Write(_byteDataArray, 0, m_iDataArrayLength);
            Array.Clear(_byteDataArray, 0, _byteDataArray.Length);
        }

        //////Send/////////////////////////////////////////////////////////////////////////
    }
}