using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/routeone")]
    public class RouteOneController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<decimal>> Get([FromServices] DataContext context, string isocode)
        {
            HttpClient client = new HttpClient();
            decimal factor = 0;

            using (var response = await client.GetAsync("https://www.bancoprovincia.com.ar/Principal/Dolar"))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                factor = Convert.ToDecimal(JsonConvert.DeserializeObject<List<string>>(apiResponse)[0].Replace('.', ','));
            }

            if (isocode != null)
                isocode = isocode.ToUpper();

            if (isocode == Enums.IsoCodes.BRL.ToString())
                return factor / 4;
            else if (isocode == Enums.IsoCodes.USD.ToString())
                return factor;
            else
                return new JsonResult("ISO CODE is not supported!");
        }
    }
}
