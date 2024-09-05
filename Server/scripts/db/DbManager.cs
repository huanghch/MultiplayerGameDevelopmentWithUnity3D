using System;
using System.Data;
using System.Security.Permissions;
using MySql.Data.MySqlClient;

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
    }
}
