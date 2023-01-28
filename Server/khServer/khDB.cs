using System;
using MySql.Data.MySqlClient;
using khServer;

namespace khDB
{
    class CDBConnector
    {
        string m_strConnection;
        //string m_strConnectInfo = "server=localhost;port=3306;database=kh;Uid=kh;password=1q2w3e4r";

        readonly string m_strPort = "port=3306";
        readonly string m_strDB = "database=kh";
        readonly string m_strUid = "Uid=kh";
        readonly string m_strPwd = "password=1q2w3e4r";

        MySqlConnection m_MySqlConnection;

        readonly string m_strUserTable = "table_kh_idpw";
        readonly string m_strUserTablePrimaryKey = "id";
        readonly string m_strUserTableColumn1 = "pw";

        string m_strCreateIdQuery;

        public void Init()
        {
            //쿼리 string 셋팅
            Setting_strQuery();

            //MySQL에 연결
            ConnectToMySQL();
        }

        public void End()
        {

        }

        private void Setting_strQuery()
        {
            //"INSERT INTO UserTable (id,password) VALUES ('{0}', '{1}');"
            m_strCreateIdQuery = "INSERT INTO " + m_strUserTable +
                " (" + m_strUserTablePrimaryKey + "," + m_strUserTableColumn1 + ") VALUES ('{0}', '{1}');";
        }

        private void ConnectToMySQL()
        {
            m_strConnection = "server=localhost"; //127.0.0.1 (루프백 IP)
            m_strConnection += ";";
            m_strConnection += m_strPort;
            m_strConnection += ";";
            m_strConnection += m_strDB;
            m_strConnection += ";";
            m_strConnection += m_strUid;
            m_strConnection += ";";
            m_strConnection += m_strPwd;
            m_strConnection += ";";

            m_MySqlConnection = new MySqlConnection(m_strConnection);
        }

        public bool Check_ExistID(string _strId)
        {
            //아이디가 있는지 확인 -> 있다면 true, 없다면 false
            //select EXISTS (SELECT id FROM 테이블 이름 WHERE id=입력id LIMIT 1) as success
            string strInsertQuery = string.Format(
                "select EXISTS(SELECT * FROM {0} WHERE {1}='{2}' LIMIT 1) as success",
                m_strUserTable, m_strUserTablePrimaryKey, _strId);
            try
            {
                m_MySqlConnection.Open();
                MySqlCommand cMySqlCommand = new MySqlCommand(strInsertQuery, m_MySqlConnection);
                MySqlDataReader cMySqlDataReader = cMySqlCommand.ExecuteReader();
                if (cMySqlDataReader == null)
                {
                    Console.WriteLine("쿼리 처리 실패 (Check_ExistID)");
                    m_MySqlConnection.Close();
                }
                else
                {
                    string strTemp = string.Empty;
                    while (cMySqlDataReader.Read())
                    {
                        int iFieldCount = cMySqlDataReader.FieldCount;
                        for (int i = 0; i < iFieldCount; i++)
                        {
                            strTemp += cMySqlDataReader[i];
                        }
                    }
                    int iCheck = int.Parse(strTemp);
                    if (iCheck == 1)
                    {
                        Console.WriteLine("중복 아이디 존재 (Check_ExistID)");
                        m_MySqlConnection.Close();
                        return true;
                    }
                    else if (iCheck == 0)
                    {
                        Console.WriteLine("중복 아이디 없음 (Check_ExistID)");
                        m_MySqlConnection.Close();
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("쿼리 처리 실패 (Check_ExistID)");
                Console.WriteLine(ex);
            }

            return false; //실패한 것도 오지만 되는 것 확인했으므로 이쪽을 탄다면 데이터 베이스 오류
        }

        public CServer.EDataType Set_CreateId(string _strId, string _strPwd)
        {
            ////아이디를 만들기 전, 중복되는 아이디가 있는지 확인
            ////select EXISTS (SELECT id FROM 테이블 이름 WHERE id=입력id LIMIT 1) as success
            //string strInsertQuery = string.Format(
            //    "select EXISTS(SELECT * FROM {0} WHERE {1}='{2}' LIMIT 1) as success",
            //    m_strUserTable, m_strUserTablePrimaryKey, _strId);
            //try
            //{
            //    m_MySqlConnection.Open();
            //    MySqlCommand cMySqlCommand = new MySqlCommand(strInsertQuery, m_MySqlConnection);
            //    MySqlDataReader cMySqlDataReader = cMySqlCommand.ExecuteReader();
            //    if (cMySqlDataReader == null)
            //    {
            //        Console.WriteLine("쿼리 처리 실패 (CreateId_Overlap Id Search)");
            //        m_MySqlConnection.Close();
            //        return CServer.EDataType.Error;
            //    }
            //    else
            //    {
            //        string strTemp = string.Empty;
            //        while (cMySqlDataReader.Read())
            //        {
            //            int iFieldCount = cMySqlDataReader.FieldCount;
            //            for (int i = 0; i < iFieldCount; i++)
            //            {
            //                strTemp += cMySqlDataReader[i];
            //            }
            //        }
            //        int iCheck = int.Parse(strTemp);
            //        if (iCheck == 1)
            //        {
            //            Console.WriteLine("중복 아이디 존재 (Overlap Id)");
            //            m_MySqlConnection.Close();
            //            return CServer.EDataType.OverlapID;
            //        }
            //        else if (iCheck == 0)
            //        {
            //            m_MySqlConnection.Close();
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("쿼리 처리 실패 (Overlap Id Check)");
            //    Console.WriteLine(ex);
            //    return CServer.EDataType.Error;
            //}

            //만약 중복되는 아이디가 있다면
            if(Check_ExistID(_strId))
            {
                return CServer.EDataType.OverlapID;
            }

            CServer.EDataType eDataType = CServer.EDataType.EDataType_End;

            //INSERT INTO 테이블이름 [column1, column2, ...] VALUES (value1, value2, ...);
            string strInsertQuery = string.Format(m_strCreateIdQuery, _strId, _strPwd);

            try
            {
                m_MySqlConnection.Open();

                //명령어 전달
                MySqlCommand cMySqlCommand = new MySqlCommand(strInsertQuery, m_MySqlConnection);

                //성공했으면 1을 반환
                if (cMySqlCommand.ExecuteNonQuery() == 1)
                {
                    Console.WriteLine("쿼리 처리 성공 (CreateId)");
                    eDataType = CServer.EDataType.CreateAccount;
                }
                else
                {
                    Console.WriteLine("쿼리 처리 실패 (CreateId)");
                    eDataType = CServer.EDataType.Error;
                }

                m_MySqlConnection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("쿼리 처리 실패 (CreateId)");
                Console.WriteLine(ex);
                eDataType = CServer.EDataType.Error;
            }

            return eDataType;
        }

        public CServer.EDataType Get_CheckLogin(string _strId, string _strPwd)
        {
            //만약 로그인 하려는데 아이디가 없다면
            if(!Check_ExistID(_strId))
            {
                return CServer.EDataType.Login_IDWrong;
            }

            CServer.EDataType eDataType = CServer.EDataType.EDataType_End;

            //아이디랑 패스워드가 모두 같은지 확인한다
             string strLoginQuery = string.Format(
            "select EXISTS(SELECT * FROM {0} WHERE {1}='{2}' AND {3}='{4}' LIMIT 1) as success",
            m_strUserTable, m_strUserTablePrimaryKey, _strId,m_strUserTableColumn1, _strPwd);

            try
            {
                m_MySqlConnection.Open();
                MySqlCommand cMySqlCommand = new MySqlCommand(strLoginQuery, m_MySqlConnection);
                MySqlDataReader cMySqlDataReader = cMySqlCommand.ExecuteReader();
                if (cMySqlDataReader == null)
                {
                    Console.WriteLine("쿼리 처리 실패 (Get_CheckLogin)");
                    m_MySqlConnection.Close();
                }
                else
                {
                    string strTemp = string.Empty;
                    while (cMySqlDataReader.Read())
                    {
                        int iFieldCount = cMySqlDataReader.FieldCount;
                        for (int i = 0; i < iFieldCount; i++)
                        {
                            strTemp += cMySqlDataReader[i];
                        }
                    }
                    int iCheck = int.Parse(strTemp);
                    if (iCheck == 1)
                    {
                        Console.WriteLine("아이디 비밀번호 옳바름 (Get_CheckLogin)");
                        m_MySqlConnection.Close();
                        eDataType = CServer.EDataType.Login;
                    }
                    else if (iCheck == 0)
                    {
                        Console.WriteLine("비밀번호 다름 (Get_CheckLogin)");
                        m_MySqlConnection.Close();
                        eDataType = CServer.EDataType.Login_PWWrong;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("쿼리 처리 실패 (Get_CheckLogin)");
                Console.WriteLine(ex);
                eDataType = CServer.EDataType.Error;
            }

            return eDataType;
        }
    }
}