using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MASAQR.Controllers
{
    /// <summary>
    /// Standard Reference Controller
    /// </summary>
    public class StandardRefController : ApiController
    {
        /// <summary>
        /// Get All Standard References
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Standard_Reference))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Standard_Reference.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Data using Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/StandardRef/GetID")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Standard_Reference))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Standard_Reference.Where(e => e.Registration_No == id).ToList();
                if (res != null)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Set Standard Reference
        /// </summary>
        /// <param name="sr">Standard Reference</param>
        /// <returns></returns>
        [Route("api/StandardRef/SetReference")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Standard_Reference))]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SetReference([FromUri] Standard_Reference sr)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Standard_Reference.FirstOrDefault(e => e.Registration_No == sr.Registration_No && e.Reference_No == sr.Reference_No);
                var recruit = entities.RecruitmentTable.FirstOrDefault(g => g.Reg_No == sr.Registration_No);

                entities.Standard_Reference.Add(sr);

                if(recruit.Reference_Type_1 == null && recruit.Reference_1 == null)
                {
                    recruit.Reference_Type_1 = "Standard";
                    recruit.Reference_1 = sr.Rating;
                }else if(recruit.Reference_Type_2 == null && recruit.Reference_2 == null)
                {
                    recruit.Reference_Type_2 = "Standard";
                    recruit.Reference_2 = sr.Rating;
                }
                else if(recruit.Reference_Type_1 != null && recruit.Reference_1 != null && recruit.Reference_Type_2 != null && recruit.Reference_2 != null)
                {
                    recruit.Reference_Type_1 = recruit.Reference_Type_2;
                    recruit.Reference_1 = recruit.Reference_2;

                    recruit.Reference_Type_2 = "Standard";
                    recruit.Reference_2 = sr.Rating;
                }
                recruit.Last_Modified = DateTime.Today;

                entities.SaveChanges();
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, sr);
                

            }
        }
    }
}
