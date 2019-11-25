using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AdminPoint;

namespace BankApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        // POST api/admin/login
        [HttpPost("login")]
        public string Login([FromBody] AdminInfo info)
        {
            return AdminFunctions.AdminLogin(info);
        }

        // POST api/admin/create
        [HttpPost("create")]
        public string Create([FromBody] CreateUser user)
        {
            return AdminFunctions.CreateAccount(user);
        }

    }
}