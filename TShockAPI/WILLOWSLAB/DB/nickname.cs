using System;
using System.Data;
using System.Text;
using System.Net;
using System.IO;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;
using System.Collections;
using System.Collections.Generic;

namespace TShockAPI.DB
{
    public class NickName
    {
        public IDbConnection database;
        public List<string> prefixID = new List<string>();
        public List<string> titleID = new List<string>();

        public NickName(IDbConnection db)
        {
            try
            {
                database = db;
                var table = new SqlTable(
                    "ss_user_nickname",
                    new SqlColumn("Account", MySqlDbType.Int32) { Primary = true },
                    new SqlColumn("nickname", MySqlDbType.VarChar, 32) { Unique = true },
                    new SqlColumn("title", MySqlDbType.Int32),
                    new SqlColumn("prefix", MySqlDbType.Int32));

                var creator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
                creator.EnsureExists(table);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" [ TShock x Lv Plugin ] 데이터베이스를 초기화하였습니다.");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" [ TShock x Lv Plugin ] 데이터베이스 초기화에 실패했습니다..");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            titleprefixload();
        }

        public void titleprefixload()
        {
            try
            {
                this.prefixID.Clear();
                this.titleID.Clear();

                string title = File.ReadAllText(
                Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName.ToString()).FullName.ToString()).FullName.ToString(), "SERVERSET", "HOSTDIR", "karna", "srinnx", "resources", "title.txt"));
                string prefix = File.ReadAllText(
                Path.Combine(Directory.GetParent(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName.ToString()).FullName.ToString()).FullName.ToString(), "SERVERSET", "HOSTDIR", "karna", "srinnx", "resources", "prefix.txt"));

                foreach(string str in title.Split(','))
                {
                    this.titleID.Add(str.Trim());
                }

                foreach (string str in prefix.Split(','))
                {
                    this.prefixID.Add(str.Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().ToString());
            }
        }

        public string getPrefix(int UserID)
        {
            try
            {
                using (var reader = database.QueryReader("SELECT * FROM ss_user_nickname WHERE Account=@0", UserID))
                {
                    if (reader.Read())
                    {
                        if (reader.Get<int>("prefix") <= 0)
                        {
                            return "";
                        }
                        else
                        {
                            return prefixID[reader.Get<int>("prefix")].Replace("*", "") + " ";
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().ToString());
                return "";
            }
        }

        public string getTitle(int UserID)
        {
            try
            {
                using (var reader = database.QueryReader("SELECT * FROM ss_user_nickname WHERE Account=@0", UserID))
                {
                    if (reader.Read())
                    {
                        if (reader.Get<int>("title") <= 0)
                        {
                            return "";
                        }
                        else
                        {
                            return titleID[reader.Get<int>("title")].Replace("*", "") + " ";
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.GetBaseException().ToString());
                return "";
            }
        }

        public string getUserNickname(int UserID, string Name)
        {
            string nickname = "";
            try
            {
                using (var reader = database.QueryReader("SELECT * FROM ss_user_nickname WHERE Account=@0", UserID))
                {
                    if(reader.Read())
                    {
                        nickname = reader.Get<string>("nickname");
                    }
                    else
                    {
                        initNickName(UserID, Name);
                    }
                }
            }
            catch  (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" [ Nickname ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                Console.WriteLine(" [ Nickname ] " + ex.GetBaseException().ToString());
                TShockAPI.Log.Error(" [ Nickname ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            return nickname;
        }

        public void initNickName(int UserID, string Name)
        {
            try
            {
                using (var reader = database.QueryReader("SELECT * FROM ss_user_nickname WHERE Account=@0", UserID))
                {
                    if (!reader.Read())
                    {
                        using(var read2r = database.QueryReader("SELECT * FROM `xe_member` WHERE `user_id` = @0", Name))
                        {
                            if(read2r.Read())
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" [ Nickname ] " + Name);
                                Console.ForegroundColor = ConsoleColor.Gray;

                                string name = read2r.Get<string>("nick_name");
                                if(name != "")
                                {
                                    database.Query("INSERT INTO ss_user_nickname (Account,nickname) VALUES('" + UserID + "','" + name + "')");
                                    //player.SendInfoMessage("홈페이지에서 닉네임을 설정할 수 있게 되었습니다.");
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(" [ Nickname ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                                TShockAPI.Log.Error(" [ Nickname ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                                Console.ForegroundColor = ConsoleColor.Gray;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(" [ Nickname ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                Console.WriteLine(" [ Nickname ] " + ex.Message);
                TShockAPI.Log.Error(" [ Nickname ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }
}
