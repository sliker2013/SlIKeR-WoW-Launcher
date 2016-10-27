using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Diagnostics;
namespace launcher
{
    class DB
    {
        public static System.Drawing.Color bColor= System.Drawing.Color.FromArgb(((int)(((byte)(187)))), ((int)(((byte)(187)))), ((int)(((byte)(187)))));
        
        public static string CONN_STRING_AUTH = "SERVER=" + SIMPLE_CONFIG.IP + ";" +
                        "DATABASE=" + SIMPLE_CONFIG.AUTH_DB + ";" +
                        "UID=" + SIMPLE_CONFIG.USER_DB + ";" +
                        "PASSWORD=" + SIMPLE_CONFIG.PASSWORD_DB + ";";
        public static string CONN_STRING_CHAR = "SERVER=" + SIMPLE_CONFIG.IP + ";" +
                        "DATABASE=" + SIMPLE_CONFIG.CHARACTERS_DB + ";" +
                        "UID=" + SIMPLE_CONFIG.USER_DB + ";" +
                        "PASSWORD=" + SIMPLE_CONFIG.PASSWORD_DB + ";";
        public static string CONN_STRING_LAUNCHER = "SERVER=" + SIMPLE_CONFIG.IP + ";" +
                        "DATABASE=" + SIMPLE_CONFIG.LAUNCHER_DB + ";" +
                        "UID=" + SIMPLE_CONFIG.USER_DB + ";" +
                        "PASSWORD=" + SIMPLE_CONFIG.PASSWORD_DB + ";";
        public static string GetRealmlist()
        {
            string realm = Properties.Settings.Default.server;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_AUTH);
                MySqlCommand cmd = new MySqlCommand("select address from realmlist", conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                realm = reader["address"].ToString();
                conn.Close();
            }
            catch { }
            Properties.Settings.Default.server = realm;
            Properties.Settings.Default.Save();
            return realm;
        }
        public static string GetRealmName()
        {
            string realm = Properties.Settings.Default.server;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_AUTH);
                MySqlCommand cmd = new MySqlCommand("select name from realmlist", conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                realm = reader["name"].ToString();
                conn.Close();
            }
            catch { }
            Properties.Settings.Default.server = realm;
            Properties.Settings.Default.Save();
            return realm;
        }
        public static int GetOnlinePlayers()
        {
            int online = 0;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_CHAR);
                MySqlCommand cmd = new MySqlCommand("select guid from characters where online=1", conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    online++;
                conn.Close();
            }
            catch { }
            return online;
        }
        public static bool IsServerOnline()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_AUTH);
                MySqlCommand cmd = new MySqlCommand("use auth", conn);
                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();

                return true;
            }
            catch(MySqlException ex) {
                Debug.WriteLine(ex.Message);
                return false; }
        }
        public static string GetNews()
        {
            string news = Properties.Settings.Default.news;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_LAUNCHER);
                MySqlCommand cmd = new MySqlCommand("select news from data", conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                news = reader["news"].ToString();
                conn.Close();
            }
            catch { }
            Properties.Settings.Default.news = news;
            Properties.Settings.Default.Save();
            return news;
        }
        public static string GetChangelog()
        {
            string changelog = Properties.Settings.Default.changelog;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_LAUNCHER);
                MySqlCommand cmd = new MySqlCommand("select changelog from data", conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                reader.Read();
                changelog = reader["changelog"].ToString();
                conn.Close();
            }
            catch { }
            Properties.Settings.Default.changelog = changelog;
            Properties.Settings.Default.Save();
            return changelog;
        }
        public static void GetLinks(out string _register, out string _forum, out string _account, out string _changelog, out string _news)
        {
            _register = Properties.Settings.Default.register;
            _forum = Properties.Settings.Default.forum;
            _account = Properties.Settings.Default.account;
            _changelog = Properties.Settings.Default.changelog_link;
            _news = Properties.Settings.Default.news_link;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_LAUNCHER);
                MySqlCommand cmd = new MySqlCommand("select * from settings", conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _register = reader["register"].ToString();
                    _forum = reader["forum"].ToString();
                    _account = reader["account"].ToString();
                }
                conn.Close();
                cmd = new MySqlCommand("select * from data", conn);
                conn.Open();
                reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _changelog = reader["changelog_link"].ToString();
                    _news = reader["news_link"].ToString();
                }
                conn.Close();
            }
            catch { }
            Properties.Settings.Default.register = _register;
            Properties.Settings.Default.forum = _forum;
            Properties.Settings.Default.account = _account;
            Properties.Settings.Default.changelog_link = _changelog;
            Properties.Settings.Default.news_link = _news;
            Properties.Settings.Default.Save();
            
        }
        public static void GetPatch(int nr,out int _patch_version, out string _patch_name,out string _patch_link)
        {
            _patch_version = Properties.Settings.Default.patch_version_1;
            _patch_name = Properties.Settings.Default.patch_name_1;
            _patch_link = "Not Found";
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_LAUNCHER);
                MySqlCommand cmd = new MySqlCommand("select * from settings", conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _patch_version = Convert.ToInt32(reader["patch_version_"+nr.ToString()]);
                    _patch_name = reader["patch_name_" + nr.ToString()].ToString();
                    _patch_link = reader["patch_link_" + nr.ToString()].ToString();
                }
                conn.Close();
            }
            catch { }
        }
        public static int GetNewPatchId()
        {
            int patch_v;
            string patch_name, patch_l;
            GetPatch(1, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_1)
                return 1;
            GetPatch(2, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_2)
                return 2;
            GetPatch(3, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_3)
                return 3;
            GetPatch(4, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_4)
                return 4;
            GetPatch(5, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_5)
                return 5;
            return 0;
        }
        public static bool RequiresUpdate()
        {
            int patch_v;
            string patch_name, patch_l;
            GetPatch(1,out patch_v,out patch_name,out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_1)
                return true;
            GetPatch(2, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_2)
                return true;
            GetPatch(3, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_3)
                return true;
            GetPatch(4, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_4)
                return true;
            GetPatch(5, out patch_v, out patch_name, out patch_l);
            if (patch_v != Properties.Settings.Default.patch_version_5)
                return true;
            return false;
        }
        public static bool PlayerExists(string name)
        {
            int exists = 0;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_CHAR);
                MySqlCommand cmd = new MySqlCommand("select guid from characters where name=@name", conn);
                cmd.Parameters.AddWithValue("@name", char.ToUpper(name[0]) + name.Substring(1));
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                    exists++;
                conn.Close();
            }
            catch { }
            return (exists != 0);
        }
        public static Character AddCharacterData(string _NAME)
        {
        int _LEVEL=0, _RACE=0, _GENDER=0, _CLASS=0, _KILLS=0, _ONLINE=0;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_CHAR);
                MySqlCommand cmd = new MySqlCommand("select level,race,gender,class,totalkills,online from characters where name=@name", conn);
                cmd.Parameters.AddWithValue("@name", char.ToUpper(_NAME[0]) + _NAME.Substring(1));
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _LEVEL = Convert.ToInt32(reader["level"]);
                    _RACE = Convert.ToInt32(reader["race"]);
                    _GENDER = Convert.ToInt32(reader["gender"]);
                    _CLASS = Convert.ToInt32(reader["class"]);
                    _KILLS = Convert.ToInt32(reader["totalkills"]);
                    _ONLINE = Convert.ToInt32(reader["online"]);
                }
                conn.Close();
            }
            catch { }
            return new Character(_NAME, _LEVEL, _RACE, _GENDER, _CLASS, _KILLS, _ONLINE);
        }
        public static void GetAllOnlineCharactersData(out Character[] chr)
        {
            int i = 0;
            chr = new Character[GetOnlinePlayers()];
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_CHAR);
                MySqlCommand cmd = new MySqlCommand("select guid from characters where online=1 order by name", conn);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    chr[i++] = AddCharacterData(Convert.ToInt32(reader["guid"]));
                }
                conn.Close();
            }
            catch { }
        }
        public static Character AddCharacterData(int _GUID)
        {
            string _NAME = "empty";
            int _LEVEL = 0, _RACE = 0, _GENDER = 0, _CLASS = 0, _KILLS = 0, _ONLINE = 0;
            try
            {
                MySqlConnection conn = new MySqlConnection(CONN_STRING_CHAR);
                MySqlCommand cmd = new MySqlCommand("select name,level,race,gender,class,totalkills,online from characters where guid=@guid", conn);
                cmd.Parameters.AddWithValue("@guid", _GUID);
                conn.Open();
                MySqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    _NAME = Convert.ToString(reader["name"]);
                    _LEVEL = Convert.ToInt32(reader["level"]);
                    _RACE = Convert.ToInt32(reader["race"]);
                    _GENDER = Convert.ToInt32(reader["gender"]);
                    _CLASS = Convert.ToInt32(reader["class"]);
                    _KILLS = Convert.ToInt32(reader["totalkills"]);
                    _ONLINE = Convert.ToInt32(reader["online"]);
                }
                conn.Close();
            }
            catch { }
            return new Character(_NAME, _LEVEL, _RACE, _GENDER, _CLASS, _KILLS, _ONLINE);
        }
        public static void ShowOnlineCharacters(int page, Character[] chr,Label lblName, Label lblLevel, PictureBox picSide, PictureBox picClass, Control _this)
        {

            // Removes all characters from the last page
            for (int i = _this.Controls.Count - 1; i >= 0; i--)
                if (_this.Controls[i].Name != "lblTitle" && _this.Controls[i].Name != "btnNext" && _this.Controls[i].Name != "btnBack" && _this.Controls[i].Name != "lblExit")
                    _this.Controls.Remove(_this.Controls[i]);

            //Adds characters to the current page
            int y = 0, counter = 0;
            foreach (Character p in chr)
            {
                if (counter >= ((page - 1) * 12) && counter < (page * 12))
                {

                    Label lblName_t = lblName.Clone();

                    lblName_t.Visible = true;
                    lblName_t.Text = p._NAME;
                    lblName_t.Location = new System.Drawing.Point(lblName.Location.X, lblName.Location.Y + 27 * y);
                    _this.Controls.Add(lblName_t);
                    lblName_t.Name = "lblName_t";

                    Label lblLevel_t = lblLevel.Clone();
                    lblLevel_t = lblLevel.Clone();

                    lblLevel_t.Visible = true;
                    lblLevel_t.Text = p._NAME;
                    lblLevel_t.Location = new System.Drawing.Point(lblLevel.Location.X, lblLevel.Location.Y + 27 * y);
                    _this.Controls.Add(lblLevel_t);
                    lblLevel_t.Name = "lblLevel_t";

                    PictureBox picSide_t = picSide.Clone();
                    picSide_t.Visible = true; picSide_t.Location = new System.Drawing.Point(picSide.Location.X, picSide.Location.Y + 27 * y);

                    _this.Controls.Add(picSide_t);
                    picSide_t.Name = "picSide_t";


                    PictureBox picClass_t = picClass.Clone();
                    picClass_t.Visible = true; picClass_t.Location = new System.Drawing.Point(picClass.Location.X, picClass.Location.Y + 27 * y);

                    _this.Controls.Add(picClass_t);
                    picClass_t.Name = "picClass_t";


                    lblName_t.Text = p._NAME;
                    lblLevel_t.Text = p._LEVEL.ToString();
                    if (p._RACE == 1 || p._RACE == 3 || p._RACE == 4 || p._RACE == 7 || p._RACE == 11)
                        picSide_t.Image = Properties.Resources.alliance;
                    else picSide_t.Image = Properties.Resources.horde;
                    switch (p._CLASS)
                    {
                        case 1:
                            picClass_t.Image = Properties.Resources.warrior;
                            break;
                        case 2:
                            picClass_t.Image = Properties.Resources.paladin;
                            break;
                        case 3:
                            picClass_t.Image = Properties.Resources.hunter;
                            break;
                        case 4:
                            picClass_t.Image = Properties.Resources.rogue;
                            break;
                        case 5:
                            picClass_t.Image = Properties.Resources.priest;
                            break;
                        case 6:
                            picClass_t.Image = Properties.Resources.dk;
                            break;
                        case 7:
                            picClass_t.Image = Properties.Resources.shaman;
                            break;
                        case 8:
                            picClass_t.Image = Properties.Resources.mage;
                            break;
                        case 9:
                            picClass_t.Image = Properties.Resources._lock;
                            break;
                        case 11:
                            picClass_t.Image = Properties.Resources.druid;
                            break;
                    }

                    y++;
                }
                counter++;
            }
            
        }
    }
    class Character // WORK IN PROGRESS
    {
        public string _NAME;
        public int _LEVEL, _RACE, _GENDER, _CLASS, _KILLS, _ONLINE;
        public Character(string _name, int _level, int _race, int _gender, int _class, int _kills,int _online)
        {
            _NAME = char.ToUpper(_name[0]) + _name.Substring(1);
            _LEVEL = _level;
            _GENDER = _gender;
            _RACE = _race;
            _CLASS = _class;
            _KILLS = _kills;
            _ONLINE = _online;
        }
        public void Show(int Nr, Label lblName,Label lblLevel, PictureBox picSide, PictureBox picClass )
        {
            
        
        }
        public void Reset(Label lblName, PictureBox picStatus)
        {
            lblName.Visible = false;
            picStatus.Visible = false;
        }
    }
    public static class ControlExtensions
    {
        public static T Clone<T>(this T controlToClone)
            where T : Control
        {
            System.Reflection.PropertyInfo[] controlProperties = typeof(T).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            T instance = Activator.CreateInstance<T>();

            foreach (System.Reflection.PropertyInfo propInfo in controlProperties)
            {
                if (propInfo.CanWrite)
                {
                    if (propInfo.Name != "WindowTarget")
                        propInfo.SetValue(instance, propInfo.GetValue(controlToClone, null), null);
                }
            }

            return instance;
        }
    }
}
