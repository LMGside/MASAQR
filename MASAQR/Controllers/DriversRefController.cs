using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MASAQR.Controllers
{
    public class DriversRefController : ApiController
    {
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Drivers_Reference))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Drivers_Reference.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Data using Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/DriversRef/GetID")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Drivers_Reference))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Drivers_Reference.Where(e => e.Registration_No == id).ToList();
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
        /// Set Drivers Reference
        /// </summary>
        /// <param name="dr">Driver Reference</param>
        /// <returns></returns>
        [Route("api/DriversRef/SetReference")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Drivers_Reference))]
        [AcceptVerbs("POST")]
        public HttpResponseMessage SetReference([FromUri] Drivers_Reference dr)
        {
            using(qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Drivers_Reference.FirstOrDefault(e => e.Registration_No == dr.Registration_No && e.Reference_No == dr.Reference_No);
                var recruit = entities.RecruitmentTable.FirstOrDefault(g => g.Reg_No == dr.Registration_No);

                entities.Drivers_Reference.Add(dr);

                if (recruit.Reference_Type_1 == null && recruit.Reference_1 == null)
                {
                    recruit.Reference_Type_1 = "Drivers";
                    recruit.Reference_1 = dr.Rating;
                }
                else if (recruit.Reference_Type_2 == null && recruit.Reference_2 == null)
                {
                    recruit.Reference_Type_2 = "Drivers";
                    recruit.Reference_2 = dr.Rating;
                }
                else if (recruit.Reference_Type_1 != null && recruit.Reference_1 != null && recruit.Reference_Type_2 != null && recruit.Reference_2 != null)
                {
                    recruit.Reference_Type_1 = recruit.Reference_Type_2;
                    recruit.Reference_1 = recruit.Reference_2;

                    recruit.Reference_Type_2 = "Drivers";
                    recruit.Reference_2 = dr.Rating;
                }
                recruit.Last_Modified = DateTime.Today;

                entities.SaveChanges();
                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, dr);
               
            }
        }
    }
}
