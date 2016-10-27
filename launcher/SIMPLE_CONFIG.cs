using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace launcher
{
    class SIMPLE_CONFIG
    {
        public static string 
                                IP = "127.0.0.1",
                                AUTH_DB = "auth335",
                                CHARACTERS_DB = "characters335",
                                LAUNCHER_DB = "launcher",
                                USER_DB = "root", // DO NOT USE AN USER WITH
                                PASSWORD_DB = ""; // ALL THE DB PERMISSIONS ( PRIVILEGES )
        public static int       PORT = 3306;
    }
}
