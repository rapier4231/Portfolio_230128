using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

//Mng로 만들면 인스턴스 땡겨와야 하니까 스태틱으로 제작
public static class KLog 
{
    //private static List<string> m_strLogWriteList       = new List<string>(); //로그 작성할 str 모아두는 리스트
    //private static int          m_iRepeatNum_ThreadKey  = 0;
    private static ConcurrentQueue<string>  m_strLogCQ              = new ConcurrentQueue<string>();
    private static int                      m_iFileFullSize         = 10000; //단위 : 바이트
    private static int                      m_iCreateLogFileCount   = 10;    //최대 로그 파일 갯수 (0~9까지 10개)
    private static int                      m_iLogFileNum;
    private static string                   m_strWritePath;
    private static StreamWriter             m_cLogWriter;
    private static Task                     m_cTask_WriteLog        = null;

    //초기화 셋팅 (로그 파일 생성 주소)
    public static void Init(string _strDirectoryPath = "KLog")
    {
        if (ReferenceEquals(m_cTask_WriteLog, null))
        {
            LogFileCheck(_strDirectoryPath);
            //아무것도 없지만 m_cTask_WriteLog 그냥 채워놓을려고 (아래서 m_cTask_WriteLog가 널인지 체크 계속하는 건 싫어서)
            m_cTask_WriteLog = Task.Run(Task_WriteLog);
        }
    }

    //로그 파일 순번을 알아온 후, Wirte 할 파일의 주소를 알아온다
    private static void LogFileCheck(string _strDirectoryPath)
    {
        //로그 파일들의 상위 폴더가 없다면 만들어준다
        if (!Directory.Exists(_strDirectoryPath))
        {
            Directory.CreateDirectory(_strDirectoryPath);
        }

        //작성을 시작할 파일의 번호를 가져온다.
        Setting_LogFileNum(_strDirectoryPath);

        //Write 할 주소 생성
        m_strWritePath = _strDirectoryPath + "/Log" + m_iLogFileNum.ToString() + ".txt";
        //로그 작성 시작을 알려주는 프린팅
        string strStartLogWrite = "";
        //기존 파일을 사용한다면 줄 바꿈을 붙인다.
        if (File.Exists(m_strWritePath))
        {
            strStartLogWrite += "\n";
            m_cLogWriter = File.AppendText(m_strWritePath);
        }
        else
        {
            m_cLogWriter = File.CreateText(m_strWritePath);
        }
        //yyyy - 년도, MM - 월, dd - 일, HH - 시, mm - 분, ss - 초, tt - 오전오후
        strStartLogWrite += DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss tt"));
        strStartLogWrite += " 로그 작성 시작";
        m_cLogWriter.WriteLine(strStartLogWrite);
        m_cLogWriter.Flush();
        m_cLogWriter.Close();
    }

    private static void Setting_LogFileNum(string _strDirectoryPath)
    {
        string strPathTemp;
        FileInfo CFileInfo;

        //로그 파일 확인
        for (int i = 0; i < m_iCreateLogFileCount; ++i)
        {
            strPathTemp = _strDirectoryPath + "/Log" + i.ToString() + ".txt";
            CFileInfo = new FileInfo(strPathTemp);

            //해당 넘버의 파일이 없다면 바로 만든다
            if (!CFileInfo.Exists)
            {
                m_iLogFileNum = i;
                return;
            }

            //해당 넘버의 파일이 있다면
            //바이트 기준 , 10000바이트 = 10킬로바이트
            //파일 크기가 아직 10 킬로바이트를 넘지 않았다면 이어서 써준다
            if (CFileInfo.Length <= m_iFileFullSize)
            {
                m_iLogFileNum = i;
                return;
            }
        }

        //모든 파일 (0~9번)이 꽉 찼다면
        OrganizeLogFile(_strDirectoryPath);
        m_iLogFileNum = m_iCreateLogFileCount - 1;
    }

    //로그 파일 정리
    //0~9번까지 로그 파일을 작성하고, 넘어가면 0번을 지운 후 하나씩 앞으로 당긴다
    private static void OrganizeLogFile(string _strDirectoryPath)
    {
        string strPathTemp1;
        string strPathTemp2;

        //첫번째 파일을 지운다
        strPathTemp1 = _strDirectoryPath + "/Log0.txt";
        File.Delete(strPathTemp1);

        //파일들의 이름을 수정한다
        for (int i = 1; i < 10; ++i)
        {
            strPathTemp2 = strPathTemp1;
            strPathTemp1 = _strDirectoryPath + "/Log" + i.ToString() + ".txt";
            File.Move(strPathTemp1, strPathTemp2);
        }
    }

    //파일 입출력은 시간이 좀 걸리는 편이니까 쓰레드로 돌려놓는다.
    private static void Task_WriteLog()
    {
        m_cLogWriter    = File.AppendText(m_strWritePath);
        int iCount      = m_strLogCQ.Count;

        for (int i = 0; i < iCount;)
        {
            if (m_strLogCQ.TryDequeue(out string _strResult))
            {
                m_cLogWriter.WriteLine(_strResult);
                i++;
            }
        }

        //m_cLogWriter.Flush();
        m_cLogWriter.Close();
    }

    public static void Exit()
    {
        //혹시나 다 안적힌 로그가 있을 수도 있으니
        if (m_cTask_WriteLog.Status == TaskStatus.RanToCompletion)
        {
            m_cTask_WriteLog        = Task.Run(Task_WriteLog);
            m_cTask_WriteLog.Wait();
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    //로그 작성 함수
    public static void Log(string _strLog)
    {
        string strWriteLog = _strLog + DateTime.Now.ToString((" - yyyy-MM-dd HH:mm:ss tt"));
        m_strLogCQ.Enqueue(strWriteLog);

        if (m_cTask_WriteLog.Status == TaskStatus.RanToCompletion)
        {
            m_cTask_WriteLog        = Task.Run(Task_WriteLog);
        }
    }
}
