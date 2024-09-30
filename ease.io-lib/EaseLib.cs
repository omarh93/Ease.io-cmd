using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Configuration;
using System.Runtime.InteropServices.WindowsRuntime;

namespace ease.io_lib
{
    public class EaseLib
    {

        private  ConnectionMultiplexer _redis;
        private  IDatabase _db;


        public static NLog.Logger myLogger = null;

        public User GetUser(string guid)
        {
            return Get(new Guid(guid.ToUpper()));

        }

        public void writeToLog(string s)
        {
            myLogger.Info(s);
            Console.WriteLine(s);
        }

        public List<User> getAllUsersInCache()
        {
            IServer server = _redis.GetServer(redisServer);
            List<User> users = new List<User>();

            var keys = server.Keys(pattern: "User:*");

            foreach (var key in keys)
            {
                var serializedUser = _db.StringGet(key);
                if (!serializedUser.IsNullOrEmpty)
                {
                    var user = JsonConvert.DeserializeObject<User>(serializedUser);
                    users.Add(user);
                    writeToLog($"User: {user.user}, GUID: {user.guid.ToString().ToUpper()}, Expiration: {user.expires}");
                }
            }
            return users;
        }
        public bool Delete(Guid guid)
        {
            bool result = false;
            try
            {
                if (GetUser(guid.ToString()) != null)
                {
                    _db.KeyDelete(getCacheKey(guid));
                    writeToLog($"Removing key with guid: {guid}");
                    result = true;
                }
                else
                {
                    writeToLog($"Tried to delete non existent key with guid:{guid}");
                }

            }
            catch (Exception ex)
            {
                writeToLog(ex.ToString());
            }

            return result;
        }

        public void Update(Guid guid,string name, DateTime expiry)
        {
            writeToLog($"Updating item: Name={name}, GUID={guid.ToString().ToUpper()}, ExpirationDate={expiry}");
            Set(new User() { expires = expiry, guid = guid, user = name });
        }
        public void Add(string name, Guid guid, DateTime expiry)
        {
            writeToLog($"Adding item: Name={name}, GUID={guid.ToString().ToUpper()}, ExpirationDate={expiry}");
            Set(new User() { expires = expiry, guid = guid, user = name });
        }

        public string getCacheKey(Guid g)
        {
            return $"User:{g.ToString().ToUpper()}";
        }
        public void Set(User user)
        {
            string cacheKey = getCacheKey(user.guid);
            var serializedUser = JsonConvert.SerializeObject(user);
            _db.StringSet(cacheKey, serializedUser,user.expires.Subtract(DateTime.Now));
        }


        public User Get(Guid guid)
        {
            try
            {
                var userJson = _db.StringGet(getCacheKey(guid));
                if (userJson.IsNullOrEmpty)
                {
                    return null;
                }
                // note: I'm not checking expiry because when I inserted the object
                // into redis I already set an expiry on it
                return JsonConvert.DeserializeObject<User>(userJson);
            }
            catch(Exception ex)
            {
                writeToLog($"Error getting user: {ex}");
                return new User() {expires=DateTime.Now, guid=guid, user = ex.Message};
            }
        }

        public string redisServer
        {
            get;set;
        }

        public EaseLib(int redisPort=6073)
        {

            if (myLogger == null)
            {
                LogManager.pathFileName = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\Ease.io\easelib.txt";
                LogManager.maxSize = 10000000;
                myLogger = LogManager.Instance.GetCurrentClassLogger();
            }

            Assembly assm = typeof(EaseLib).Assembly;
            AssemblyName name = assm.GetName();
            writeToLog("EaseLib Version: " + name.Version.ToString());

            redisServer = $"localhost:{redisPort}";

            try
            {
                _redis = ConnectionMultiplexer.Connect(redisServer);
                _db = _redis.GetDatabase();
                writeToLog($"Successfully connected to redis at port:{redisPort}");
            }
            catch(Exception ex)
            {
                writeToLog(ex.ToString());
                throw new Exception("See logs for exception details when connecting to redis");
            }

        }

    }

    public class User
    {
        public string user { get; set; }
        public Guid guid { get; set; }
        public DateTime expires { get; set; }
    }
}
