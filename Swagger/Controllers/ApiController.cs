using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using Calabonga.DemoClasses;
using Microsoft.AspNetCore.Mvc;

namespace Swagger.Controllers
{
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        private readonly List<Person> _people = People.GetPeople();

        [HttpGet("[action]")]
        public IActionResult GetAll()
        {
            return Ok(_people);
        }

        [HttpGet("[action]/{id:int}")]
        public IActionResult GetById(int id)
        {
            var item = _people.FirstOrDefault(x => x.Id == id);
            return Ok(item);
        }
    }
}
