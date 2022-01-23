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
    /// Assignment Contract Controller
    /// </summary>
    public class AssignmentController : ApiController
    {
        /// <summary>
        /// Get all Assignment Confirmation Data
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Assignments))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Assignments.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Data using Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Assignment/GetID/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Assignments))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Assignments.Where(e => e.Registration_No == id).ToList();
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
        /// Get Last Assignment Confirmation
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Assignment/GetLastAC/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Assignments))]
        [HttpGet]
        public HttpResponseMessage GetLastAC(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Assignments.Where(e => e.Registration_No == id).OrderByDescending(s => s.Registration_No).FirstOrDefault();
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
        /// Add Assignment Confirmation
        /// </summary>
        /// <param name="id">Reg Number</param>
        /// <param name="name">Client Name</param>
        /// <param name="start">Start Date</param>
        /// <param name="pos">Position of Employee</param>
        /// <param name="endDate">End Date</param>
        /// <param name="endEvent">End Condition</param>
        /// <returns></returns>
        [Route("api/Assignment/AddAssignment/")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Assignments))]
        [HttpGet]
        public HttpResponseMessage AddAssignment(int id, string name, DateTime start, string pos, DateTime? endDate=null, string endEvent=null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                Assignments assign = new Assignments()
                {
                    Registration_No = id,
                    Client_Name = name,
                    Start_Date = start,
                    Position = pos,
                    End_Date = endDate,
                    End_Condition = endEvent
                };
                entities.Assignments.Add(assign);
                entities.SaveChanges();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new string[] { "Assignment Confirmation Added" });
            }
        }
    }
}
