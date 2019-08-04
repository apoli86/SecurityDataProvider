using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SecurityDataProvider.Entities;
using SecurityDataProvider.Api.Services;

namespace SecurityDataProvider.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityDataProviderController : ControllerBase
    {
        private readonly SecurityPriceRepository securityPriceRepository;
        private readonly JsonSerializerSettings serializerSettings;

        public SecurityDataProviderController(SecurityPriceRepository securityPriceRepository)
        {
            this.securityPriceRepository = securityPriceRepository;
            this.serializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new KeepPropertyNameContractResolver()
            };
        }

        // GET api/values
        [HttpGet]
        public ActionResult<string> Get()
        {
            return new ActionResult<string>("Please request a SecurityPrice");
        }

        // GET api/values/5
        [HttpGet("{symbol}")]
        public ActionResult<Entities.Requests.SecurityPrice> Get(string symbol)
        {
            return Json(securityPriceRepository.GetSecurityPrice(symbol));
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private ActionResult<T> Json<T>(T obj)
        {
            return new JsonResult(obj, serializerSettings);
        }
    }
}
