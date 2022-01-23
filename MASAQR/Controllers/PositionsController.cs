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
    /// Postions of Employees Controller
    /// </summary>
    public class PositionsController : ApiController
    {
        /// <summary>
        /// Get all Assignment Confirmation Data
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Employee_Positions))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Employee_Positions.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Data using Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/Positions/GetID/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Employee_Positions))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Employee_Positions.Where(e => e.Register_No == id).ToList();
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
        /// Set Positions of Employee
        /// </summary>
        /// <param name="pos">Array of Position names</param>
        /// <returns></returns>
        [Route("api/Positions/SetPositions/")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Employee_Positions))]
        [HttpGet]
        public HttpResponseMessage SetPositions([FromUri] string[] pos)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                int count = 0;
                int reg = 0;
                var collectPos = "";
                // reg_no~pos~posExp
                for (int p = 0; p < pos.Length; p++)
                {
                    string[] str = pos[p].Split('~');
                    int reg_no = int.Parse(str[0]);
                    reg = reg_no;
                    string position = str[1];
                    collectPos = collectPos + position + "~";
                    string posExperience = str[2];

                    var res = entities.Employee_Positions.Where(e => e.Register_No == reg_no && e.Position == position).FirstOrDefault();

                    if (res == null)
                    {
                        Employee_Positions ep = new Employee_Positions()
                        {
                            Register_No = reg_no,
                            Position = position,
                            Experience = posExperience
                        };

                        entities.Employee_Positions.Add(ep);
                        entities.SaveChanges();
                        count++;
                    }
                    else
                    {
                        res.Experience = posExperience;
                        entities.SaveChanges();
                        count++;
                    }
                }

                if(pos.Length == count)
                {
                    var res2 = entities.RecruitmentTable.FirstOrDefault(p => p.Reg_No == reg);
                    res2.Positions = collectPos.Substring(0, (collectPos.Length - 1));
                    res2.Last_Modified = DateTime.Today;

                    entities.SaveChanges();
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new string[] { "Saved Positons" });
                }

                return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Error, Information not Found");
            }
        }

        /// <summary>
        /// Get All Positions
        /// </summary>
        /// <returns></returns>
        [Route("api/Positions/GetPositions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Employee_Positions))]
        [HttpGet]
        public HttpResponseMessage GetPositions()
        {
            using(qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Employee_Positions
                                  .Select(p => p.Position)
                                  .Distinct().ToList();
                res.Add("Other");

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }
    }
}
