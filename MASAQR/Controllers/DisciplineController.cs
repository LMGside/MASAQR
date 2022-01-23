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
    /// Discipline Controller
    /// </summary>
    public class DisciplineController : ApiController
    {
        /// <summary>
        /// Get all Disciplinary Data
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Disciplinary))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Disciplinary.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Data using Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Disciplinary/GetID/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Disciplinary))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Disciplinary.Where(e => e.Registration_No == id).ToList();
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
        /// Get last disciplinary data using registration id
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Disciplinary/GetLastDisc/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Disciplinary))]
        [HttpGet]
        public HttpResponseMessage GetLastDisc(int id)
        {
            using(qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Disciplinary.Where(e => e.Registration_No == id).OrderByDescending(s => s.Registration_No).FirstOrDefault();
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
        /// Save new Discipline
        /// </summary>
        /// <param name="user">User ID</param>
        /// <param name="reg_no">Registration Number</param>
        /// <param name="chair">Chairperson</param>
        /// <param name="charge">Charge</param>
        /// <param name="discDate">Disciplinary Date</param>
        /// <param name="plea">Plea</param>
        /// <param name="sanction">Sanction</param>
        /// <param name="sancReason">Reason for Sanction</param>
        /// <param name="sancExpiry">Sanction Date</param>
        /// <param name="comment">Additional Comment</param>
        /// <returns></returns>
        [Route("api/Disciplinary/SaveDiscipline")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Disciplinary))]
        [HttpGet]
        public HttpResponseMessage SaveDiscipline(int user, int reg_no, string chair, string charge, DateTime discDate, string plea, string sanction, string sancReason, DateTime? sancExpiry, string comment = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Disciplinary.Where(g => g.Registration_No == reg_no);
                if (res != null)
                {
                    Disciplinary newDisc = new Disciplinary()
                    {
                        Registration_No = reg_no,
                        Chairperson = chair,
                        Charge = charge,
                        Date_of_Disciplinary = discDate,
                        Plea = plea,
                        Sanction = sanction,
                        Reason_for_Sanction = sancReason,
                        Expiry_Date_for_Sanction = sancExpiry,
                        Additional_Comment = comment

                    };

                    Track_Modifications tm = new Track_Modifications()
                    {
                        ID = user,
                        Modified_Action = "New Disciplinary",
                        Modified_Date = DateTime.Now,
                        Modified_Registration_No = reg_no,
                        Modified_Table = "Recruitment"
                    };

                    entities.Track_Modifications.Add(tm);
                    entities.Disciplinary.Add(newDisc);
                    entities.SaveChanges();

                    var newRes = entities.Disciplinary.OrderByDescending(s => s.Disciplinary_No).FirstOrDefault();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, newRes);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }
    }
}
