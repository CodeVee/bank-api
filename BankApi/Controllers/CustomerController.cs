using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CustomerPoint;

namespace BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        // POST api/customer/login
        [HttpPost("login")]
        public string Post([FromBody] CLoginInfo info)
        {
            return CustomerFunctions.CustomerLogin(info);
        }

        // GET api/customer/balance
        [HttpGet("balance")]
        public double GetBalance([FromBody] CAccountNumber number)
        {
            return CustomerFunctions.Balance(number);
        }

        // GET api/customer/transactions
        [HttpGet("transactions")]
        public List<Transactions> GetTransactions([FromBody] CAccountNumber number)
        {
             CustomerFunctions.History(number);
            return Connection.transactions;
        }

        // PUT api/customer/withdrawal
        [HttpPut("withdrawal")]
        public string MakeWithdrawl([FromBody] CAccountNumber cAccount)
        {
            return CustomerFunctions.Withdrawal(cAccount);
        }

        // PUT api/customer/transfer
        [HttpPut("transfer")]
        public string Transfer([FromBody] CAccountNumber cAccount)
        {
            return CustomerFunctions.Transfer(cAccount);
        }
    }
}