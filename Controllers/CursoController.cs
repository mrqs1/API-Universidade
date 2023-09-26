using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using apiUniversidade.Model;
using Microsoft.AspNetCore.Mvc;

namespace apiUniversidade.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CursoController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<Curso>> Get();
        {  
            var cursos = context.Cursos.ToList();
            if(Cursos is null){
                return NotFound();
            }
            
            return cursos;
        }
    }
}