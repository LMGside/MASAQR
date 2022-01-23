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
    /// Crime Check Controller
    /// </summary>
    public class CrimeController : ApiController
    {

        private bool IfFailed(int reg)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.RecruitmentTable.FirstOrDefault(e => e.Reg_No == reg);

                if (res.Interview != null && res.Reference_1 != null && res.Reference_2 != null && res.ABET_Lit != null && res.ABET_Num != null && res.Crime_Check != null && res.Master_Contract != null)
                {
                    res.Vetting_Finished = true;
                }

                if (res.Crime_Check == "Positive")
                {
                    res.Decline_Reason = "Positive Crim Check";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                if (res.ABET_Lit < 40 || res.ABET_Num < 40)
                {
                    res.Decline_Reason = "ABET Scores too Low";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                if (res.Reference_1 <= 5 || res.Reference_2 <= 5)
                {
                    res.Decline_Reason = "Reference Scores too Low";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                if (res.Interview <= 5)
                {
                    res.Decline_Reason = "Interview Score too Low";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                if (res.Manager_Approval == "Decline")
                {
                    res.Decline_Reason = "Manager Declined";
                    res.Employment_Status = "Poor MASA Feedback";
                    res.Vetting_Finished = true;
                    entities.SaveChanges();
                    return false;
                }

                res.Decline_Reason = null;
                entities.SaveChanges();
                return true;
            }

        }

        /// <summary>
        /// Get all Crime Check Data
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Crim_Check))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Crim_Check.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Data using Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Crime/GetID/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Crim_Check))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Crim_Check.Where(e => e.Registration_No == id).ToList();
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
        /// Get Last Crime check of Employee
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/Crime/GetLastCrime/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Crim_Check))]
        [HttpGet]
        public HttpResponseMessage GetLastCrime(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Crim_Check.Where(e => e.Registration_No == id).OrderByDescending(s => s.Registration_No).FirstOrDefault();
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
        /// Add new Crime Check to User
        /// </summary>
        /// <param name="user">User ID</param>
        /// <param name="id">Registartion Number</param>
        /// <param name="result">Clear or Positive</param>
        /// <returns></returns>
        [Route("api/Crime/AddCrimeCheck/")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Crim_Check))]
        [HttpGet]
        public HttpResponseMessage AddCrimeCheck(int user, int id, string result)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {

                var res = entities.Crim_Check.FirstOrDefault(e => e.Registration_No == id && e.Criminal_Date == DateTime.Today);

                Track_Modifications tm = new Track_Modifications()
                {
                    ID = user,
                    Modified_Action = "",
                    Modified_Date = DateTime.Now,
                    Modified_Registration_No = id,
                    Modified_Table = "Crim Check"
                };

                if (res == null)
                {
                    Crim_Check cc = new Crim_Check
                    {
                        Registration_No = id,
                        Criminal_Result = result,
                        Criminal_Date = DateTime.Today
                    };
                    tm.Modified_Action = "Adding Crim Check";
                    entities.Crim_Check.Add(cc);
                }
                else
                {
                    tm.Modified_Action = "Updating Crim Check";
                    res.Criminal_Result = result;
                }

                entities.Track_Modifications.Add(tm);

                //Update Crime Check on Recruitment Table
                var res2 = entities.RecruitmentTable.FirstOrDefault(e => e.Reg_No == id);
                res2.Crime_Check = result;
             
                entities.SaveChanges();
                bool n = IfFailed(id);

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new string[] { "Crim Check Added"});
            }
        }
    }
}
