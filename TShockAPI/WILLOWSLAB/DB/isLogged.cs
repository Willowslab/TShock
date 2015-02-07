using System;
using System.Data;
using System.Text;
using System.Net;
using System.IO;
using MySql.Data.MySqlClient;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace TShockAPI.DB
{
    public struct LevelData
    {
        public int id;
        public long exp;
    }

    public class isLogged
    {
        public IDbConnection database;

        public isLogged(IDbConnection db)
        {
            database = db;
            var table = new SqlTable("ss_tsusers_islogged", new SqlColumn("Account", MySqlDbType.Int32) { Primary = true }, new SqlColumn("islogged", MySqlDbType.Int32) { DefaultValue = "-1" });

            var creator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            creator.EnsureExists(table);

            var table2 = new SqlTable("ss_tsCharXPSystem",
                                     new SqlColumn("Account", MySqlDbType.Int32) { Primary = true },
                                     new SqlColumn("exp", MySqlDbType.Int32) { DefaultValue = "0" });
            var creator2 = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            creator2.EnsureExists(table2);

            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.WriteLine(" [ Lv Plugin ] 데이터베이스를 초기화하였습니다.");
        }

        public void setHP(int UserID, int mxHP)
        {
            database.Query("UPDATE ss_tscharacters SET MaxHealth = '" + mxHP + "' WHERE Account = @0", UserID);
        }

        public void setMP(int UserID, int mxMP)
        {
            database.Query("UPDATE ss_tscharacters SET MaxMana = '" + mxMP + "' WHERE Account = @0", UserID);
        }
        public void logged_in(TSPlayer player)
        {
            bool isLogged = getLoggedState(player.UserID);
            if(isLogged)
            {
                player.Disconnect("이미 접속중입니다. 계속될 경우 문의해주시기 바랍니다.                             ");
            }
            else
            {
                setLoggedState(player.UserID, true);
                TShock.Utils.Broadcast("<SYSTEM> " + player.Name + TShock.TSKoreanEndParse(player.Name, 2) + " 접속했습니다.", Color.Yellow);
            }
        }

        public void LoggedOut(int UserID)
        {
            database.Query("UPDATE ss_tsusers_islogged SET islogged = -1 WHERE Account = @0", UserID);
        }

        public void setLoggedState(int UserID,bool logged)
        {
            string logged_info = "-1";
            if (logged)
            {
                logged_info = "0";
            }

            database.Query("UPDATE ss_tsusers_islogged SET islogged = '@1' WHERE Account = @0", UserID, logged_info);
        }

        public bool getLoggedState(int UserID)
        {
            bool isLogged = true;
            try
            {
                using (var reader = database.QueryReader("SELECT * FROM ss_tsusers_islogged WHERE Account=@0", UserID))
                {
                    if (!reader.Read())
                    {
                        database.Query("INSERT INTO ss_tsusers_islogged (Account) VALUES(@0)", UserID);
                        isLogged = false;
                    }
                    else
                    {
                        if(reader.Get<int>("islogged") == -1)
                        {
                            isLogged = false;
                        }
                        else
                        {
                            isLogged = true;
                        }
                    }
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" [ Lv Plugin ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                TShockAPI.Log.Error(" [ Lv Plugin ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
            }
            return isLogged;
        }
        public LevelData sumExpData(int UserID, int sumXP)
        {
            try
            {
                database.Query("UPDATE ss_tsCharXPSystem SET exp = exp + @1 WHERE Account = @0", UserID, sumXP);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" [ Lv Plugin ] 회원의 데이터베이스를 수정하는 데 오류가 발생했습니다.");
                TShockAPI.Log.Error(" [ Lv Plugin ] 회원의 데이터베이스를 수정하는 데 오류가 발생했습니다.");
            }
            return getLevelData(UserID);
        }

        public LevelData getLevelData(int UserID)
        {
            var lvdata = new LevelData();
            try
            {
                using (var reader = database.QueryReader("SELECT * FROM ss_tsCharXPSystem WHERE Account=@0", UserID))
                {
                    if (!reader.Read())
                    {
                        database.Query("INSERT INTO ss_tsCharXPSystem (Account,exp) VALUES(@0,0)", UserID);
                        lvdata.id = UserID;
                        lvdata.exp = 0;
                    }
                    else
                    {
                        lvdata.id = UserID;
                        lvdata.exp = reader.Get<int>("exp");
                    }
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" [ Lv Plugin ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                TShockAPI.Log.Error(" [ Lv Plugin ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
            }
            return lvdata;
        }

        public int getWlabMemberSRL(int UserID)
        {
            int rtn = 0;
            string user_id = "";

            try
            {
                using (var reader = database.QueryReader("SELECT * FROM ss_tsusers WHERE ID=@0", UserID))
                {
                    if (reader.Read())
                    {
                        user_id = reader.Get<string>("Username");
                    }
                }

                if (!user_id.Equals(""))
                {
                    using (var reader = database.QueryReader("SELECT * FROM xe_member WHERE user_id=@0", user_id))
                    {
                        if (reader.Read())
                        {
                            rtn = reader.Get<int>("member_srl");
                        }
                    }
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" [ Lv Plugin ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                TShockAPI.Log.Error(" [ Lv Plugin ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
            }
            return rtn;
        }

        public bool isGroup(int member_srl, int group_srl)
        {
            bool rtn = false;
            try
            {
                using (var reader = database.QueryReader("SELECT * FROM xe_member_group_member WHERE member_srl=@0 AND group_srl=@1", member_srl, group_srl))
                {
                    if (reader.Read())
                    {
                        rtn = true;
                    }
                }
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(" [ Lv Plugin ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
                TShockAPI.Log.Error(" [ Lv Plugin ] 회원의 데이터베이스를 가져오는 데 오류가 발생했습니다.");
            }
            return rtn;
        }

        public int[] calcRGB(string str)
        {
            int[] rgb = { 0, 0, 0 };
            if (str.Length <= 12 && str.Length > 2)
            {
                for (int i = 0; i < str.Length; i++)
                {
                    rgb[i % 3] += (int)str.ToCharArray()[i];
                }

                rgb[0] = ((rgb[0] * 101) % 97) + 159;
                rgb[1] = ((rgb[1] * 101) % 97) + 159;
                rgb[2] = ((rgb[2] * 101) % 97) + 159;
            }
            else
            {
                rgb[0] = 192;
                rgb[1] = 192;
                rgb[2] = 192;
            }
            return rgb;
        }

        public int LevelCalc(double exp)
        {
            var level = 1;
            var maxLv = 200;
            //만랩은 100인거신 허나 200까지 설정해서 체크하도록 한다. 혹시 모르거든...
            for (int i = 1; i <= maxLv; i++)
            {
                if (i >= 200)
                    return 200;
                //레벨 계산 식과 경험치를 대조해 봄
                if (reqLevel(i) <= exp && exp < reqLevel(i + 1))
                {
                    return i;
                }
            }
            return 0;
        }

        public int Level(int lvl)
        {
            int rtn = (int)Math.Floor(Math.Pow(Math.Floor((Math.Floor(lvl * 16 * Math.Pow(lvl, 0.5)) - 16) / 3), 1.7) + (Math.Floor(lvl * 64 * Math.Pow(lvl, 0.5)) - 64));
            return rtn -= rtn % 10;
        }
        public int reqLevel(int lvl)
        {
            var lvxp = 0;
            for (int i = 1; i <= lvl; i++)
            {
                lvxp += (int)Math.Floor(Math.Pow(Math.Floor((Math.Floor(i * 16 * Math.Pow(i, 0.5)) - 16) / 3), 1.7) + (Math.Floor(i * 64 * Math.Pow(i, 0.5)) - 64));
                lvxp -= lvxp % 10;
            }
            return lvxp;
        }
    }
}
