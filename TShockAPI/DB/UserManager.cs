/*
TShock, a server mod for Terraria
Copyright (C) 2011-2015 Nyx Studios (fka. The TShock Team)

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.CodeDom.Compiler;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace TShockAPI.DB
{
	public class UserManager
	{
		private IDbConnection database;

		public UserManager(IDbConnection db)
		{
			database = db;

			var table = new SqlTable("ss_tsUsers",
			                         new SqlColumn("ID", MySqlDbType.Int32) {Primary = true, AutoIncrement = true},
			                         new SqlColumn("Username", MySqlDbType.VarChar, 32) {Unique = true},
			                         new SqlColumn("Password", MySqlDbType.VarChar, 128),
                                     new SqlColumn("UUID", MySqlDbType.VarChar, 128),
			                         new SqlColumn("Usergroup", MySqlDbType.Text),
									 new SqlColumn("Registered", MySqlDbType.Text),
                                     new SqlColumn("LastAccessed", MySqlDbType.Text),
                                     new SqlColumn("KnownIPs", MySqlDbType.Text)
				);
			var creator = new SqlTableCreator(db,
			                                  db.GetSqlType() == SqlType.Sqlite
			                                  	? (IQueryBuilder) new SqliteQueryCreator()
			                                  	: new MysqlQueryCreator());
			creator.EnsureExists(table);
		}

		/// <summary>
		/// Adds a given username to the database
		/// </summary>
		/// <param name="user">User user</param>
		public void AddUser(User user)
		{
            //using (var reader = database.QueryReader("SELECT * FROM Warps WHERE WorldID = @0",Main.worldID.ToString()))
            //{
            //    while (reader.Read())
            //    {
            //        Warps.Add(new Warp(
            //            new Point(reader.Get<int>("X"), reader.Get<int>("Y")),
            //            reader.Get<string>("WarpName"),
            //            (reader.Get<string>("Private") ?? "0") != "0"));
            //    }
            //}

			if (!TShock.Groups.GroupExists(user.Group))
				throw new GroupNotExistsException(user.Group);

			int ret;
			try
            {
				ret = database.Query("INSERT INTO ss_tsUsers (Username, Password, UUID, UserGroup, Registered) VALUES (@0, @1, @2, @3, @4);", user.Name,
								   TShock.Utils.HashPassword(user.Password), user.UUID, user.Group, DateTime.UtcNow.ToString("s"));
			}
			catch (Exception ex)
			{
				// Detect duplicate user using a regexp as Sqlite doesn't have well structured exceptions
				if (Regex.IsMatch(ex.Message, "Username.*not unique"))
					throw new UserExistsException(user.Name);
				throw new UserManagerException("AddUser SQL returned an error (" + ex.Message + ")", ex);
			}

			if (1 > ret)
				throw new UserExistsException(user.Name);

			Hooks.AccountHooks.OnAccountCreate(user);
		}

		/// <summary>
		/// Removes a given username from the database
		/// </summary>
		/// <param name="user">User user</param>
		public void RemoveUser(User user)
		{
			try
			{
				var tempuser = GetUser(user);
				int affected = database.Query("DELETE FROM ss_tsUsers WHERE Username=@0", user.Name);

				if (affected < 1)
					throw new UserNotExistException(user.Name);

				Hooks.AccountHooks.OnAccountDelete(tempuser);
			}
			catch (Exception ex)
			{
				throw new UserManagerException("RemoveUser SQL returned an error", ex);
			}
		}

		/// <summary>
		/// Sets the Hashed Password for a given username
		/// </summary>
		/// <param name="user">User user</param>
		/// <param name="group">string password</param>
		public void SetUserPassword(User user, string password)
		{
			try
			{
				if (
					database.Query("UPDATE ss_tsUsers SET Password = @0 WHERE Username = @1;", TShock.Utils.HashPassword(password),
					               user.Name) == 0)
					throw new UserNotExistException(user.Name);
			}
			catch (Exception ex)
			{
				throw new UserManagerException("SetUserPassword SQL returned an error", ex);
			}
		}

		/// <summary>
		/// Sets the UUID for a given username
		/// </summary>
		/// <param name="user">User user</param>
		/// <param name="group">string uuid</param>
		public void SetUserUUID(User user, string uuid)
		{
			try
			{
				if (
					database.Query("UPDATE ss_tsUsers SET UUID = @0 WHERE Username = @1;", uuid,
								   user.Name) == 0)
					throw new UserNotExistException(user.Name);
			}
			catch (Exception ex)
			{
				throw new UserManagerException("SetUserUUID SQL returned an error", ex);
			}
		}

		/// <summary>
		/// Sets the group for a given username
		/// </summary>
		/// <param name="user">User user</param>
		/// <param name="group">string group</param>
		public void SetUserGroup(User user, string group)
		{
			try
			{
				Group grp = TShock.Groups.GetGroupByName(group);
				if (null == grp)
					throw new GroupNotExistsException(group);

				if (database.Query("UPDATE ss_tsUsers SET UserGroup = @0 WHERE Username = @1;", group, user.Name) == 0)
					throw new UserNotExistException(user.Name);
				
				// Update player group reference for any logged in player
				foreach (var player in TShock.Players.Where(p => null != p && p.UserAccountName == user.Name))
				{
					player.Group = grp;
				}
			}
			catch (Exception ex)
			{
				throw new UserManagerException("SetUserGroup SQL returned an error", ex);
			}
		}

        public void UpdateLogin(User user)
        {
            try
            {
                if (database.Query("UPDATE ss_tsUsers SET LastAccessed = @0, KnownIps = @1 WHERE Username = @2;", DateTime.UtcNow.ToString("s"), user.KnownIps, user.Name) == 0)
                    throw new UserNotExistException(user.Name);
            }
            catch (Exception ex)
            {
                throw new UserManagerException("UpdateLogin SQL returned an error", ex);
            }
        }

        public void UpdateLoggedinfo(int UserID, Boolean flag)
        {
            try
            {
                //database.Query("UPDATE ss_tsUser_islogged SET isLogged = @0 WHERE userid = @1;", args.Player.UserID, "true");
                var strtmp = "";
                if (flag)
                    strtmp = "true";
                else
                    strtmp = "false";

                Console.WriteLine(UserID);
                var tmp = database.Query("UPDATE ss_tsuser_islogged SET isLogged = @0 WHERE userid = @1;", UserID, strtmp);
                if (tmp == 0)
                {
                    Console.WriteLine(tmp);
                }
                Console.WriteLine(tmp);
            }
            catch (Exception ex)
            {
                var tmp = GetUserByID(UserID);
                AddLogged(tmp.ID,tmp.Name,flag);
                throw new UserManagerException("UpdateLogin SQL returned an error", ex);
            }
        }


        public void AddLogged(int UserID,string Name, Boolean flag)
        {
            int ret;
            try
            {
                ret = database.Query("INSERT INTO ss_tsUser_islogged (userid, username, isLogged) VALUES (@0, @1, @2);", UserID, Name, flag ? "true" : "fasle");
            }
            catch (Exception ex)
            {
                if (Regex.IsMatch(ex.Message, "Username.*not unique"))
                    throw new UserExistsException(Name);
                throw new UserManagerException("AddUser SQL returned an error (" + ex.Message + ")", ex);
            }

            if (1 > ret)
                throw new UserExistsException(Name);
        }

		public int GetUserID(string username)
		{
			try
			{
				using (var reader = database.QueryReader("SELECT * FROM ss_tsUsers WHERE Username=@0", username))
				{
					if (reader.Read())
					{
						return reader.Get<int>("ID");
					}
				}
			}
			catch (Exception ex)
			{
				Log.ConsoleError("FetchHashedPasswordAndGroup SQL returned an error: " + ex);
			}
			return -1;
		}

		public User GetUserByName(string name)
		{
			try
			{
				return GetUser(new User {Name = name});
			}
			catch (UserManagerException)
			{
				return null;
			}
		}

        public Boolean GetUserByNameforTEST(User user)
        {
            try
            {
                return GetTESTID(user);
            }
            catch (UserManagerException)
            {
                return false;
            }
        }

        public Boolean changeLogged(int id,string name,Boolean flag)
        {
            try
            {
                return setLogged(new User {ID = id, Name =name },flag);
            }
            catch (UserManagerException)
            {
                return false;
            }
        }

		public User GetUserByID(int id)
		{
			try
			{
				return GetUser(new User {ID = id});
			}
			catch (UserManagerException)
			{
				return null;
			}
		}
        public String getMD5(string str)
        {
            System.Security.Cryptography.MD5 MD5hasher = System.Security.Cryptography.MD5.Create();
            byte[] Data = MD5hasher.ComputeHash(System.Text.Encoding.Default.GetBytes(str));
            System.Text.StringBuilder sBuilder = new System.Text.StringBuilder();
            int i = 0;
            for (i = 0; i <= Data.Length - 1; i++)
            {
                sBuilder.Append(Data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// logged then false, else true
        /// </summary>
        /// <param name="user"></param>
        /// <param name="name"></param>
        /// <returns></returns>

        public Boolean setLogged(User user, Boolean flag)
        {
            try
            {
                using (var result = database.QueryReader("SELECT * FROM ss_tsUsers_islogged WHERE Account=@0", user.ID))
                {
                    if(!result.Read())
                    {
                        //ret = database.Query("INSERT INTO ss_tsUsers_islogged (userid, isLogged) VALUES (@0, @1);", user, "-1");
                        return false;
                        throw new UserNotExistException(user.Name);
                    }
                    else
                    {
                        database.Query("UPDATE ss_tsUsers_islogged SET isLogged = @0 WHERE Account = @1;", user.ID, flag ? "0" : "-1");
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public Boolean GetTESTID(User usr)
        {
            try
            {
                using (var result = database.QueryReader("SELECT * FROM xe_member WHERE user_id=@0", usr.Name))
                {
                    if (!result.Read())
                        return false;
                    else
                    {
                        var pw = result.Get<string>("password");
                        if (pw == getMD5(usr.Password))
                        {
                            using (var result2 = database.QueryReader("SELECT * FROM srinn_gamedata WHERE member_srl=@0", result.Get<string>("member_srl")))
                            {
                                if (!result2.Read())
                                {
                                    throw new UserManagerException("SrinnX의 게임 데이터의 갱신이 필요합니다.");
                                }
                                else
                                {
                                    try
                                    {
                                        if (result2.Get<Int32>("terraria") >= 0)
                                            return true;
                                        else
                                            throw new UserManagerException("정품 인증이 되어 있어야 등록이 가능합니다.");
                                    }
                                    catch
                                    {
                                        throw new UserManagerException("SrinnX의 게임 데이터의 갱신이 필요합니다.");
                                    }
                                }
                            }
                        }
                        else
                            return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

		public User GetUser(User user)
        {

			bool multiple = false;
			string query;
			string type;
			object arg;
			if (0 != user.ID)
			{
				query = "SELECT * FROM ss_tsUsers WHERE ID=@0";
				arg = user.ID;
				type = "id";
			}
			else
			{
				query = "SELECT * FROM ss_tsUsers WHERE Username=@0";
				arg = user.Name;
				type = "name";
			}

			try
			{
				using (var result = database.QueryReader(query, arg))
				{
					if (result.Read())
					{
						user = LoadUserFromResult(user, result);
						// Check for multiple matches
						if (!result.Read())
							return user;
						multiple = true;
					}
				}
			}
			catch (Exception ex)
			{
				throw new UserManagerException("GetUser SQL returned an error (" + ex.Message + ")", ex);
			}
			if (multiple)
				throw new UserManagerException(String.Format("Multiple Users found for {0} '{1}'", type, arg));

			throw new UserNotExistException(user.Name);
		}

		public List<User> GetUsers()
		{
			try
			{
				List<User> users = new List<User>();
				using (var reader = database.QueryReader("SELECT * FROM ss_tsUsers"))
				{
					while (reader.Read())
					{
						users.Add(LoadUserFromResult(new User(), reader));
					}
					return users;
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			return null;
		}

		private User LoadUserFromResult(User user, QueryResult result)
		{
			user.ID = result.Get<int>("ID");
			user.Group = result.Get<string>("Usergroup");
			user.Password = result.Get<string>("Password");
            user.UUID = result.Get<string>("UUID");
			user.Name = result.Get<string>("Username");
			user.Registered = result.Get<string>("Registered");
            user.LastAccessed = result.Get<string>("LastAccessed");
            user.KnownIps = result.Get<string>("KnownIps");
			return user;
		}
	}

	public class User
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Password { get; set; }
        public string UUID { get; set; }
		public string Group { get; set; }
		public string Registered { get; set; }
        public string LastAccessed { get; set; }
        public string KnownIps { get; set; }

		public User(string name, string pass, string uuid, string group, string registered, string last, string known)
		{
			Name = name;
			Password = pass;
            UUID = uuid;
			Group = group;
			Registered = registered;
		    LastAccessed = last;
		    KnownIps = known;
		}

		public User()
		{
			Name = "";
			Password = "";
            UUID = "";
			Group = "";
			Registered = "";
            LastAccessed = "";
            KnownIps = "";
		}
	}

	[Serializable]
	public class UserManagerException : Exception
	{
		public UserManagerException(string message)
			: base(message)
		{
		}

		public UserManagerException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}

	[Serializable]
	public class UserExistsException : UserManagerException
	{
		public UserExistsException(string name)
			: base("User '" + name + "' already exists")
		{
		}
	}

	[Serializable]
	public class UserNotExistException : UserManagerException
	{
		public UserNotExistException(string name)
			: base("User '" + name + "' does not exist")
		{
		}
	}

	[Serializable]
	public class GroupNotExistsException : UserManagerException
	{
		public GroupNotExistsException(string group)
			: base("Group '" + group + "' does not exist")
		{
		}
	}
}