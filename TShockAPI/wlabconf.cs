using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Xml;

using Mono.Data.Sqlite;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using MaxMind;

using Terraria;
using TerrariaApi.Server;

using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Net;
using TShockAPI.ServerSideCharacters;

namespace TShockAPI
{
    public class wlabconf
    {
        public double xpMultipler = 1.0;
        public double xpEventMultipler = 1.0;
        public double FPMultipler = 1.0;
        public bool goldevent = false;
        public bool battleEvent = false;
        public bool heartEvent = false;
        public string xpEventMent = "";
        public int LevelLimit = 0;
        public string channelname = "CH1";

        public static string dbt_character = "ss_tsCharacter";
        public static string dbt_itemban = "ss_tsItemBans";
        public static bool forceTime = false;
        public static bool forceWeather = false; // false is clear;

        public string savePath = Path.Combine(TShock.SavePath, "wlabsetting.xml");
        //AppDomain.CurrentDomain.BaseDirectory
        public string eventsetPath = Path.Combine(Directory.GetParent(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).FullName.ToString()).FullName.ToString(), "serversetting.xml");

        public wlabconf()
        {
            this.xpMultipler = 1;
            this.xpEventMultipler = 1;
            this.xpEventMent = "";
            this.FPMultipler = 1;
            this.LevelLimit = 0;
            this.goldevent = false;
            readSetting(true);
        }

        public void reload()
        {
            readSetting();
        }

        public void saveEventSetting()
        {
            #region unused

            StringBuilder files = new StringBuilder();
            files.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            files.AppendLine("<wlabsetting ver=\"1.0\">");
            files.AppendLine("\t<setting name=\"GoldEvent\" var1=\"" + this.goldevent.ToString() + "\" />");
            files.AppendLine("\t<setting name=\"xpEventMultipler\" var1=\"" + this.xpEventMultipler + "\" />");
            files.AppendLine("</wlabsetting>");
            File.WriteAllText(this.eventsetPath, files.ToString(), Encoding.UTF8);

            #endregion
        }

        public void readEventSetting(bool isDevel = false)
        {
            //if (!File.Exists(this.saveNcPath)) saveSetting();
            if (!File.Exists(this.eventsetPath))
            {
                saveEventSetting();
                Console.WriteLine("DEVEL");
            }
            #region unused

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(eventsetPath);
            foreach (XmlElement xnode in xmlDocument.SelectNodes("/wlabsetting/*"))
            {
                //if (isDevel) Console.WriteLine(xnode.Name);
                var Name = xnode.GetAttribute("name");

                if (xnode.Name.ToLower() == "setting")
                {
                    if (Name == "xpEventMultipler")
                    {
                        this.xpEventMultipler = Convert.ToDouble(xnode.GetAttribute("var1"));
                        if (isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }

                    if (Name == "xpEventMent")
                    {
                        this.xpEventMent = xnode.GetAttribute("var1");
                        if (isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }

                    if (Name == "GoldEvent")
                    {
                        this.goldevent = xnode.GetAttribute("var1").ToLower() == "true"?true:false;
                        if (isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }

                    if (Name == this.channelname + "_FPMultipler")
                    {
                        this.FPMultipler = Convert.ToDouble(xnode.GetAttribute("var1"));
                        if (isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }

                    if (Name == this.channelname + "_LevelLimit")
                    {
                        this.LevelLimit = Convert.ToInt32(xnode.GetAttribute("var1"));
                        if (isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }

                    if (Name == this.channelname + "_xpMultipler")
                    {
                        this.xpMultipler = Convert.ToDouble(xnode.GetAttribute("var1"));
                        if (isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }
                }
            }

            #endregion
        }

        public void readSetting(bool isDevel = false)
        {
            //if (!File.Exists(this.saveNcPath)) saveSetting();
            if (!File.Exists(this.savePath))
            {
                saveSetting();
                Console.WriteLine("DEVEL");
            }
            #region unused

            var xmlDocument = new XmlDocument();
            xmlDocument.Load(savePath);
            foreach(XmlElement xnode in xmlDocument.SelectNodes("/wlabsetting/*"))
            {
                //if (isDevel) Console.WriteLine(xnode.Name);
                var Name = xnode.GetAttribute("name");

                if(xnode.Name.ToLower() == "setting")
                {
                    if (Name == "channelname")
                    {
                        this.channelname = xnode.GetAttribute("var1");
                        if(isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }
                    if (Name == "itemban_dbtablename")
                    {
                        wlabconf.dbt_itemban = xnode.GetAttribute("var1");
                        if(isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }
                    if (Name == "character_dbtablename")
                    {
                        wlabconf.dbt_character = xnode.GetAttribute("var1");
                        if(isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }
                    if (Name == "forcedtime")
                    {
                        wlabconf.forceTime = (xnode.GetAttribute("var1").ToLower() == "true"?true: false);
                        if(isDevel) Console.WriteLine(xnode.GetAttribute("var1"));
                    }
                }
            }

            readEventSetting(isDevel);
            
            #endregion
        }

        public void saveSetting()
        {
            #region unused
            
            StringBuilder files = new StringBuilder();
            files.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            files.AppendLine("<wlabsetting ver=\"1.0\">");
            files.AppendLine("\t<setting name=\"channelname\" var1=\"" + this.channelname + "\" />");
            files.AppendLine("</wlabsetting>");
            File.WriteAllText(this.savePath,files.ToString(),Encoding.UTF8);
            
            #endregion
        }
    }
}
