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
    /// Education Controller
    /// </summary>
    public class EducationController : ApiController
    {
        /// <summary>
        /// Get all Qualification Data
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Education))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Education.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Data using Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Education/GetID/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Education))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Education.Where(e => e.Registration_No == id).ToList();
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
        /// Add new Education Qualification to user
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <param name="category">Type of Qualification</param>
        /// <param name="qualification"></param>
        /// <returns></returns>
        [Route("api/Education/AddEducation/")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Education))]
        [HttpGet]
        public HttpResponseMessage AddEducation(int id, string category, string qualification)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Education.Where(e => e.Registration_No == id && e.Category == category).FirstOrDefault();
                var modify = entities.RecruitmentTable.FirstOrDefault(p => p.Reg_No == id);

                if (res == null)
                {
                    Education edu = new Education()
                    {
                        Registration_No = id,
                        Qualification = qualification,
                        Category = category
                    };

                    entities.Education.Add(edu);
                }
                else
                {
                    res.Qualification = qualification;
                }

                modify.Last_Modified = DateTime.Today;
                entities.SaveChanges();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new string[] { "Education Added" });
            }
        }

        /// <summary>
        /// Update Education Data
        /// </summary>
        /// <param name="reg_no">Registration Number</param>
        /// <param name="edu_no">Education Number</param>
        /// <param name="qual">Qualification</param>
        /// <param name="cat">Education Type</param>
        /// <returns></returns>
        [Route("api/Education/UpdateEducation")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Education))]
        [HttpGet]
        public HttpResponseMessage UpdateEducation(int reg_no, int edu_no, string qual, string cat)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Education.Where(e => e.Education_No == edu_no && e.Registration_No == reg_no).FirstOrDefault();

                if (res != null)
                {
                    res.Qualification = qual;
                    res.Category = cat;

                    entities.SaveChanges();

                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new String[] { "Updated Successfully" });
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Education was not Found");
                }
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Education))]
        [HttpGet]
        public HttpResponseMessage GetEducation()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Education
                                  .Select(p => p.Qualification)
                                  .Distinct().ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
    }
}
