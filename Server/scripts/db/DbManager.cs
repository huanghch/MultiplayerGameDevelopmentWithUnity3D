using System;
using System.Data;
using System.Security.Permissions;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace Server.db
{
    class DbManager
    {
        public static MySqlConnection mysql;

        //连接mysql数据库
        public static bool Connect(string db, string ip, int port, string user, string pw)
        {
            //创建MySqlConnection对象
            mysql = new MySqlConnection();
            //连接参数
            string s = string.Format("Database={0};Data Source={1}; port={2};User Id={3}; Password={4}", db, ip, port, user, pw);
            mysql.ConnectionString = s;
            //连接
            try
            {
                mysql.Open();
                Console.WriteLine("[数据库]connect succ ");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("[数据库]connect fail, " + e.Message);
                return false;
            }
        }

        //判定安全字符串
        private static bool IsSafeString(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }

        // 是否存在该用户
        public static bool IsAccountExist(string id)
        {
            // 防止SQL注入
            if(!DbManager.IsSafeString(id))
            {
                return false;
            }

            // SQL语句
            string s = string.Format("select * from account where id = '{0}';", id);
            // 查询
            try
            {
                MySqlCommand cmd = new MySqlCommand(s, mysql);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return !hasRows;
            }
            catch(Exception e)
            {
                Console.WriteLine("[数据库] IsSafeString err, " + e.Message);
                return false;
            }

            return false;
        }

        // 注册
        public static bool Register(string id, string pw)
        {
            if (!DbManager.IsSafeString(id))   
            {
                Console.WriteLine("[数据库] Register fail, id is not safe");
                return false;
            }

            if(!DbManager.IsSafeString(pw))
            {
                Console.WriteLine("[数据库] Register fail, password is not safe");
                return false;
            }


            // 能否注册
            if(!IsAccountExist(id))
            {
                Console.WriteLine("[数据库] Register fail, id exist");
                return false;
            }
            

            // 写入数据库User表
            string s = string.Format("insert into account set id = '{0}', pw = '{1}';", id, pw);

            try
            {
                MySqlCommand cmd = new MySqlCommand(s, mysql);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[数据库] Register fail " + e.Message);
                return false;
            }
        }
    }
}
