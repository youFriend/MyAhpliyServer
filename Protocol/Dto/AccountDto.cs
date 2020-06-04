using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Dto
{
    [Serializable]
    public class AccountDto
    {
        public string Account;
        public string Password;

        public AccountDto() {
            
        }

        public AccountDto(string Account, string Password) {
            this.Account = Account;
            this.Password = Password;
        }
    }
}
