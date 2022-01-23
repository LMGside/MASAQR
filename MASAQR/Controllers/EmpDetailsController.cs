using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MASAQR.Controllers
{
    public class EmpDetailsController : ApiController
    {
        /// <summary>
        /// Get all Employee Details data
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Employee_Detail))]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Employee_Detail.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Get Info with Registration Number
        /// </summary>
        /// <param name="id">Registration Number</param>
        /// <returns></returns>
        [Route("api/EmpDetails/GetID/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Employee_Detail))]
        [HttpGet]
        public HttpResponseMessage GetID(int id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Employee_Detail.FirstOrDefault(e => e.Registration_No == id);
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
        /// Post new Employee Details
        /// </summary>
        /// <param name="user">User ID</param>
        /// <param name="reg"></param>
        /// <param name="name"></param>
        /// <param name="nationality"></param>
        /// <param name="language"></param>
        /// <param name="email"></param>
        /// <param name="disabled"></param>
        /// <param name="NoD"></param>
        /// <param name="martial"></param>
        /// <param name="health"></param>
        /// <param name="vaccinated"></param>
        /// <param name="kin_name"></param>
        /// <param name="kin_contact"></param>
        /// <param name="kin_relation"></param>
        /// <param name="acc_history"></param>
        /// <param name="own_trans"></param>
        /// <param name="acc_detail"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Employee_Detail))]
        [Route("api/EmpDetails/POST")]
        [AcceptVerbs("POST")]
        public HttpResponseMessage POST(int user, int reg, String name, String nationality = null, String language = null, String email = null, bool? disabled = null, String NoD = null, String martial = null, String health = null,
            String vaccinated = null, String kin_name = null, String kin_contact = null, String kin_relation = null, bool? acc_history = null, String own_trans = null, String acc_detail = null)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Employee_Detail.FirstOrDefault(e => e.Registration_No == reg);
                Employee_Detail newDetails;

                if(res == null)
                {
                    newDetails = new Employee_Detail
                    {
                        Registration_No = reg,
                        Name = name,
                        Nationality = nationality,
                        Home_Language = language,
                        Email = email,
                        Disabled = disabled,
                        Nature_of_Disability = NoD,
                        Martial_Status = martial,
                        Health = health,
                        Vaccinated = vaccinated,
                        NoK_Name = kin_name,
                        NoK_Contact = kin_contact,
                        NoK_Relationship = kin_relation,
                        Own_Transport = own_trans,
                        Accident_History = acc_history,
                        Accident_Detail = acc_detail
                    };

                    entities.Employee_Detail.Add(newDetails);
                }
                else
                {
                    res.Name = name;
                    res.Nationality = nationality;
                    res.Home_Language = language;
                    res.Email = email;
                    res.Disabled = disabled;
                    res.Nature_of_Disability = NoD;
                    res.Martial_Status = martial;
                    res.Health = health;
                    res.Vaccinated = vaccinated;
                    res.NoK_Name = kin_name;
                    res.NoK_Contact = kin_contact;
                    res.NoK_Relationship = kin_relation;
                    res.Own_Transport = own_trans;
                    res.Accident_History = acc_history;
                    res.Accident_Detail = acc_detail;

                    Track_Modifications tm = new Track_Modifications()
                    {
                        ID = user,
                        Modified_Action = "Updated Employee Details",
                        Modified_Date = DateTime.Now,
                        Modified_Registration_No = reg,
                        Modified_Table = "Employee Details"
                    };

                    entities.Track_Modifications.Add(tm);
                }

                entities.SaveChanges();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
            }
        }

        /// <summary>
        /// Update Extra Employee Details
        /// </summary>
        /// <param name="empD">Employee Details</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Employee_Detail))]
        [Route("api/EmpDetails/Update")]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Update([FromUri] Employee_Detail empD)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Employee_Detail.Where(e => e.Registration_No == empD.Registration_No).FirstOrDefault();

                if (res != null)
                {
                    res.Name = empD.Name;
                    res.Nationality = empD.Nationality;
                    res.Home_Language = empD.Home_Language;
                    res.Email = empD.Email;
                    res.Disabled = empD.Disabled;
                    res.Nature_of_Disability = empD.Nature_of_Disability;
                    res.Martial_Status = empD.Martial_Status;
                    res.Health = empD.Health;
                    res.Vaccinated = empD.Vaccinated;
                    res.NoK_Name = empD.NoK_Name;
                    res.NoK_Contact = empD.NoK_Contact;
                    res.NoK_Relationship = empD.NoK_Relationship;
                    res.Own_Transport = empD.Own_Transport;
                    res.Accident_History = empD.Accident_History;
                    res.Accident_Detail = empD.Accident_Detail;

                    entities.SaveChanges();
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new String[] { "Updated Successfully" });
                    //return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not Found");
                }
            }
        }

    }
}
