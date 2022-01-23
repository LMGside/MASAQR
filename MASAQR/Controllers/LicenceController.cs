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
    /// Licence Controller
    /// </summary>
    public class LicenceController : ApiController
    {
        /// <summary>
        /// Get all Licence Data
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Licence))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Licence.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Data using Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Licence/GetID/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Licence))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Licence.Where(e => e.Registration_No == id).ToList();
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
        /// Save Licence
        /// </summary>
        /// <param name="reg_no">Registration Number</param>
        /// <param name="type">Licence Type</param>
        /// <param name="code">Licence Code</param>
        /// <param name="exp">Expiry Date</param>
        /// <param name="mhv">MHV Attachment</param>
        /// <returns></returns>
        [Route("api/Licence/SaveLicence")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Licence))]
        [HttpGet]
        public HttpResponseMessage SaveLicence(int reg_no, string type, string code, DateTime exp, string mhv = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Licence.Where(g => g.Registration_No == reg_no);
                if (res != null)
                {
                    Licence newLic = new Licence()
                    {
                        Registration_No = reg_no,
                        Licence_Type = type,
                        Licence_Code = code,
                        Licence_Expiry_Date = exp,
                        MHV_Attachment = mhv
                    };
                    entities.Licence.Add(newLic);
                    entities.SaveChanges();

                    var newRes = entities.Licence.OrderByDescending(s => s.Licence_No).FirstOrDefault();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, newRes);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, User not Found");
                }
            }
        }

        /// <summary>
        /// Update Licence Data
        /// </summary>
        /// <param name="reg_no">Registration Number</param>
        /// <param name="lic_no">Licence Number</param>
        /// <param name="type">Licence Type</param>
        /// <param name="code">Licence Code</param>
        /// <param name="exp">Expiry Date</param>
        /// <param name="mhv">MHV Attachment</param>
        /// <returns></returns>
        [Route("api/Licence/UpdateLicence")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Licence))]
        [HttpGet]
        public HttpResponseMessage UpdateLicence(int reg_no, int lic_no, string type, string code, DateTime exp, string mhv = null)
        {
            using(qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Licence.Where(e => e.Licence_No == lic_no && e.Registration_No == reg_no).FirstOrDefault();

                if (res != null)
                {
                    res.Licence_Type = type;
                    res.Licence_Code = code;
                    res.Licence_Expiry_Date = exp;
                    res.MHV_Attachment = mhv;

                    entities.SaveChanges();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new String[] { "Updated Successfully" });
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Licence was not Found");
                }
            }
        }
    }
}
