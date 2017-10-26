//------------------------------------------------------------------------------
using System.Data;
using FirebirdSql.Data.FirebirdClient;
using System;
//------------------------------------------------------------------------------
namespace csforth
{
    public class FirebirdDB
    {
        //------------------------------------------------------------------------------
        //------------------------------------------------------------------------------
        private FbConnection conn = null;
        public FbConnection Conn
        {
            get { return conn; }
        }

        private bool connected = false;
        public bool Connected
        {
            get { return connected; }
        }

        public delegate int cb_select(object[] list, int cnt);
        //------------------------------------------------------------------------------
        //------------------------------------------------------------------------------
        public FirebirdDB(string server, string port, string database)
        {
            string connectionString =
            /*"User=SYSDBA;" +
            "Password=masterkey;";

             connectionString += "Database=" + DB + ";DataSource=" + server + ";";

             connectionString +=

             "Port=3050;" +
             "Dialect=3;" +
             "Charset=NONE;" +
             "Role=;" +
             "Connection lifetime=15;" +
             "Pooling=true;" +
             "MinPoolSize=0;" +
             "MaxPoolSize=50;" +
             "Packet Size=8192;" +
             "ServerType=0";*/

            string.Format
            (
                "data source=ApothecaryDS;initial catalog={0}/{1}:{2};user id=sysdba;Password=masterkey;Charset=WIN1251",
                server,
                port,
                database
            );
            //"data source=ApothecaryDS;initial catalog=192.168.1.112/3052:c:\\iadb\\iapteka0.fdb;user id=sysdba;Password=masterkey;Charset=WIN1251";

            try
            {
                conn = new FbConnection(connectionString);
                conn.Open();

                connected = (conn.State == ConnectionState.Open);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }
        //------------------------------------------------------------------------------
        public object SelectOne(string command)
        {
            object retval = null;

            if (Conn.State == ConnectionState.Open)
            {
                FbCommand cmd = new FbCommand(command, conn);

                FbDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    if (reader.Read())
                        retval = reader.GetValue(0);
                }

                reader.Close();
            }

            return retval;
        }
        //------------------------------------------------------------------------------
        public int Select(string command, cb_select callback)
        {
            int cnt = 0;

            if (Conn.State == ConnectionState.Open)
            {
                FbCommand cmd = new FbCommand(command, conn);
                //cmd.CommandTimeout = 0;


                FbDataReader reader = cmd.ExecuteReader();


                if (reader.HasRows)
                {
                    //string[] list = new string[reader.FieldCount];
                    object[] values = new object[reader.FieldCount];
                    //int fieldCount;

                    while (reader.Read())
                    {

                        reader.GetValues(values);

                        //for (int i = 0; i < reader.FieldCount; i++)
                        //    list[i] = reader.GetString(i);
                        if (callback(values, cnt++) != 0)
                            break;
                    }
                }

                reader.Close();

            }

            return cnt;
        }
        //------------------------------------------------------------------------------
        public object[][] Select(string command)
        {
            object[][] ret = null;
            uint cnt;

            if (Conn.State == ConnectionState.Open)
            {
                cnt = uint.Parse(SelectOne("select count(*) from (" + command + ")").ToString());

                if (cnt > 0)
                {
                    FbCommand cmd = new FbCommand(command, conn);
                    FbDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        ret = new object[cnt][];
                        cnt = 0;

                        while (reader.Read())
                        {
                            ret[cnt] = new object[reader.FieldCount];
                            reader.GetValues(ret[cnt++]);
                        }
                    }

                    reader.Close();
                }
            }

            return ret;
        }
        //------------------------------------------------------------------------------
    }
}
