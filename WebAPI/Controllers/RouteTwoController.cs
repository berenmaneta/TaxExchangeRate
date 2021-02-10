using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.Entities;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/routetwo")]
    public class RouteTwoController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        public async Task<ActionResult<List<Purchase>>> Get([FromServices] DataContext context)
        {
            var purchases = await context.Purchases.ToListAsync();
            return purchases;
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionResult<Purchase>> Post(
            [FromServices] DataContext context, 
            [FromBody] PurchaseModel model)
        {
            if (ModelState.IsValid)
            {
                HttpClient client = new HttpClient();
                decimal factor = 0;

                using (var response = await client.GetAsync("https://www.bancoprovincia.com.ar/Principal/Dolar"))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    factor = Convert.ToDecimal(JsonConvert.DeserializeObject<List<string>>(apiResponse)[0].Replace('.', ','));
                }

                if (model.IsoCode != null)
                    model.IsoCode = model.IsoCode.ToUpper();

                if (model.IsoCode == Enums.IsoCodes.BRL.ToString())
                    factor /= 4;
                else if(model.IsoCode.ToUpper() != Enums.IsoCodes.USD.ToString())
                    return new JsonResult(new { Success = false, Message = "ISO CODE is not supported!" });

                Purchase entity = new Purchase();
                entity.IdUser = model.IdUser;
                entity.IsoCode = model.IsoCode;
                entity.Value = model.ValueARG / factor;
                entity.TransactionDate = DateTime.Now;

                if (!ValidateAmount(entity))
                {
                    return new JsonResult(new { Success = false, Message = "Value has exceeded the limit for the selected ISO CODE!" });
                }
                    
                if (!ValidateAmountByUser(context, entity))
                {
                    return new JsonResult(new { Success = false, Message = "Value has exceeded the limit for the user within the month!" });
                }
       
                context.Purchases.Add(entity);
                await context.SaveChangesAsync();
                return entity;
                
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        private bool ValidateAmount(Purchase entity)
        {
            if (entity.IsoCode.Equals(Enums.IsoCodes.USD.ToString()) && entity.Value > 200)
                return false;
            else if (entity.IsoCode.Equals(Enums.IsoCodes.BRL.ToString()) && entity.Value > 300)
                return false;

            return true;
        }

        private bool ValidateAmountByUser(DataContext context, Purchase entity)
        {
            var purchases = GetPurchasesByIdUser(context, entity.IdUser);
            var sum = purchases.Sum(x => x.Value);

            if (entity.IsoCode.Equals(Enums.IsoCodes.USD.ToString()) && (sum + entity.Value) > 200)
                return false;
            if (entity.IsoCode.Equals(Enums.IsoCodes.BRL.ToString()) && (sum + entity.Value) > 300)
                return false;

            return true;
        }

        public List<Purchase> GetPurchasesByIdUser(DataContext context, int IdUser)
        {
            var purchases = context.Purchases.Where(x => x.IdUser == IdUser && x.TransactionDate.Month == DateTime.Now.Month).ToList();
            return purchases;
        }
    }

}
