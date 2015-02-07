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
        // 2015.02.08 02:18 참조하지 않는 함수 제거
        public IDbConnection database;

        public isLogged(IDbConnection db)
        {
            database = db;
            var table = new SqlTable("ss_tsusers_islogged", new SqlColumn("Account", MySqlDbType.Int32) { Primary = true }, new SqlColumn("islogged", MySqlDbType.Int32) { DefaultValue = "-1" });

            var creator = new SqlTableCreator(db, db.GetSqlType() == SqlType.Sqlite ? (IQueryBuilder)new SqliteQueryCreator() : new MysqlQueryCreator());
            creator.EnsureExists(table);
        }

        public void LoggedOut(int UserID)
        {
            database.Query("UPDATE ss_tsusers_islogged SET islogged = -1 WHERE Account = @0", UserID);
        }
    }
}
