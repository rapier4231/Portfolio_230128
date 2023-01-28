using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;

//Mng�� ����� �ν��Ͻ� ���ܿ;� �ϴϱ� ����ƽ���� ����
public static class KLog 
{
    //private static List<string> m_strLogWriteList       = new List<string>(); //�α� �ۼ��� str ��Ƶδ� ����Ʈ
    //private static int          m_iRepeatNum_ThreadKey  = 0;
    private static ConcurrentQueue<string>  m_strLogCQ              = new ConcurrentQueue<string>();
    private static int                      m_iFileFullSize         = 10000; //���� : ����Ʈ
    private static int                      m_iCreateLogFileCount   = 10;    //�ִ� �α� ���� ���� (0~9���� 10��)
    private static int                      m_iLogFileNum;
    private static string                   m_strWritePath;
    private static StreamWriter             m_cLogWriter;
    private static Task                     m_cTask_WriteLog        = null;

    //�ʱ�ȭ ���� (�α� ���� ���� �ּ�)
    public static void Init(string _strDirectoryPath = "KLog")
    {
        if (ReferenceEquals(m_cTask_WriteLog, null))
        {
            LogFileCheck(_strDirectoryPath);
            //�ƹ��͵� ������ m_cTask_WriteLog �׳� ä���������� (�Ʒ��� m_cTask_WriteLog�� ������ üũ ����ϴ� �� �Ⱦ)
            m_cTask_WriteLog = Task.Run(Task_WriteLog);
        }
    }

    //�α� ���� ������ �˾ƿ� ��, Wirte �� ������ �ּҸ� �˾ƿ´�
    private static void LogFileCheck(string _strDirectoryPath)
    {
        //�α� ���ϵ��� ���� ������ ���ٸ� ������ش�
        if (!Directory.Exists(_strDirectoryPath))
        {
            Directory.CreateDirectory(_strDirectoryPath);
        }

        //�ۼ��� ������ ������ ��ȣ�� �����´�.
        Setting_LogFileNum(_strDirectoryPath);

        //Write �� �ּ� ����
        m_strWritePath = _strDirectoryPath + "/Log" + m_iLogFileNum.ToString() + ".txt";
        //�α� �ۼ� ������ �˷��ִ� ������
        string strStartLogWrite = "";
        //���� ������ ����Ѵٸ� �� �ٲ��� ���δ�.
        if (File.Exists(m_strWritePath))
        {
            strStartLogWrite += "\n";
            m_cLogWriter = File.AppendText(m_strWritePath);
        }
        else
        {
            m_cLogWriter = File.CreateText(m_strWritePath);
        }
        //yyyy - �⵵, MM - ��, dd - ��, HH - ��, mm - ��, ss - ��, tt - ��������
        strStartLogWrite += DateTime.Now.ToString(("yyyy-MM-dd HH:mm:ss tt"));
        strStartLogWrite += " �α� �ۼ� ����";
        m_cLogWriter.WriteLine(strStartLogWrite);
        m_cLogWriter.Flush();
        m_cLogWriter.Close();
    }

    private static void Setting_LogFileNum(string _strDirectoryPath)
    {
        string strPathTemp;
        FileInfo CFileInfo;

        //�α� ���� Ȯ��
        for (int i = 0; i < m_iCreateLogFileCount; ++i)
        {
            strPathTemp = _strDirectoryPath + "/Log" + i.ToString() + ".txt";
            CFileInfo = new FileInfo(strPathTemp);

            //�ش� �ѹ��� ������ ���ٸ� �ٷ� �����
            if (!CFileInfo.Exists)
            {
                m_iLogFileNum = i;
                return;
            }

            //�ش� �ѹ��� ������ �ִٸ�
            //����Ʈ ���� , 10000����Ʈ = 10ų�ι���Ʈ
            //���� ũ�Ⱑ ���� 10 ų�ι���Ʈ�� ���� �ʾҴٸ� �̾ ���ش�
            if (CFileInfo.Length <= m_iFileFullSize)
            {
                m_iLogFileNum = i;
                return;
            }
        }

        //��� ���� (0~9��)�� �� á�ٸ�
        OrganizeLogFile(_strDirectoryPath);
        m_iLogFileNum = m_iCreateLogFileCount - 1;
    }

    //�α� ���� ����
    //0~9������ �α� ������ �ۼ��ϰ�, �Ѿ�� 0���� ���� �� �ϳ��� ������ ����
    private static void OrganizeLogFile(string _strDirectoryPath)
    {
        string strPathTemp1;
        string strPathTemp2;

        //ù��° ������ �����
        strPathTemp1 = _strDirectoryPath + "/Log0.txt";
        File.Delete(strPathTemp1);

        //���ϵ��� �̸��� �����Ѵ�
        for (int i = 1; i < 10; ++i)
        {
            strPathTemp2 = strPathTemp1;
            strPathTemp1 = _strDirectoryPath + "/Log" + i.ToString() + ".txt";
            File.Move(strPathTemp1, strPathTemp2);
        }
    }

    //���� ������� �ð��� �� �ɸ��� ���̴ϱ� ������� �������´�.
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
        //Ȥ�ó� �� ������ �αװ� ���� ���� ������
        if (m_cTask_WriteLog.Status == TaskStatus.RanToCompletion)
        {
            m_cTask_WriteLog        = Task.Run(Task_WriteLog);
            m_cTask_WriteLog.Wait();
        }
    }

    /////////////////////////////////////////////////////////////////////////////
    //�α� �ۼ� �Լ�
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
