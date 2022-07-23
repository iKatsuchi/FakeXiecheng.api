using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace FakeXiecheng.api.Controllers
{
    [Route("api/shoudongapi")]
    [Controller]
    //三种方式 继承则This继承了很多方法
    //public class ShoudongAPIController
    //public class ShoudongAPI:Controller
    public class ShoudongAPI
    {
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

    }
}
