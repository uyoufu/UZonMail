﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UZonMailService.UzonMailDB.SQL;

namespace UZonMailService.Controllers.Tests
{
    /// <summary>
    /// 测试用的控制器
    /// </summary>
    public class TestController(SqlContext db) : ControllerBaseV1
    {
        /// <summary>
        /// 测试 jsonMap
        /// </summary>
        /// <returns></returns>
        [HttpGet("json-map")]
        [AllowAnonymous]
        public async Task TestJsonMap()
        {
           
        }
    }
}
