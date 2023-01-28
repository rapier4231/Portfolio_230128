using System;
using khServer;
using khDB;

static class CMain
{
    //서버
    static CServer m_cServer;
    static CDBConnector m_cDBConnector;

    //프로그램 진입점
    static public void Main(string[] args)
    {
        Class_Init();

        Console.WriteLine("kh 콘솔 확인 창\n");

        bool m_bMainOn = true;
        while (m_bMainOn)
        {
        }

        Class_End();
    }

    static private void Class_Init()
    {
        m_cDBConnector = new CDBConnector();
        m_cDBConnector.Init();

        m_cServer = new CServer();
        m_cServer.Init();
    }

    static private void Class_End()
    {
        m_cServer.End();
        m_cDBConnector.End();
    }

    static public CDBConnector Get_DBConnector()
    {
        return m_cDBConnector;
    }
}

