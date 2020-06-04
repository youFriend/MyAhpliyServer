using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AhpilyServer.Util.Concurrent;

namespace GameServer.Cache
{
    class AccountCache
    {
        private Dictionary<string, AccountModel> accountDictionary = new Dictionary<string, AccountModel>();

        private ConcurrentInt id = new ConcurrentInt(-1);

        /// <summary>
        /// 判断是否存在
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public bool isExist(string account) {
            return accountDictionary.ContainsKey(account);
        }

        public void add(string account , string password) {
            AccountModel model = new AccountModel();
            model.Account = account;
            model.password = password;
            model.id = id.Add(); // 添加1 并且返回

            accountDictionary.Add(account, model);
        }
    }
}
