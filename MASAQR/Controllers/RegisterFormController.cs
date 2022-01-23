using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using MASAQR.Models;

namespace MASAQR.Controllers
{
    /// <summary>
    /// Register Form Controller
    /// </summary>
    public class RegisterFormController : ApiController
    {
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Register_Form.ToList();

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
            }
        }

        /// <summary>
        /// Find Employee History
        /// </summary>
        /// <param name="id">Employee ID</param>
        /// <returns></returns>
        [Route("api/RegisterForm/EmpActivity/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Register_Form))]
        [HttpGet]
        public HttpResponseMessage EmpActivity(String id)
        {
            using(qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Register_Form.Where(s => s.Employee_ID.Contains(id)).ToList();

                if(query.Count() > 0)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound,  "User Not Found");
                }
            }
        }

        /// <summary>
        /// Find Site History
        /// </summary>
        /// <param name="id">Site ID</param>
        /// <returns></returns>
        [Route("api/RegisterForm/SiteActivity/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Register_Form))]
        [HttpGet]
        public HttpResponseMessage SiteActivity(String id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = entities.Register_Form.Where(s => s.Site_ID.Contains(id)).ToList();

                if (query.Count() > 0)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, query);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "User Not Found");
                }
            }
        }

        [Route("api/RegisterForm/Scanned")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Register_Form))]
        [AcceptVerbs("GET")]
        public HttpResponseMessage Scanned([FromUri] Register_Form register)
        {
            var site = register.Site_ID;
            var emp = register.Employee_ID;
            var day = register.Date;
            TimeSpan time = DateTime.Now.TimeOfDay;
            double lati2 = 0.0, longi2 = 0.0; 

            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var q2 = from site2 in entities.Site.AsEnumerable()
                         where site2.Site_ID == register.Site_ID
                         select site2;

                foreach (var element in q2)
                {
                    lati2 = Decimal.ToDouble(element.Latitude);
                    longi2 = Decimal.ToDouble(element.Longitude);
                }
                var reg = entities.Register_Form.Where(r => r.Site_ID.Contains(site) && r.Employee_ID.Contains(emp) && r.Date.Equals(day));
                var regC = reg.FirstOrDefault();

                var cnt = reg.Count();

                if(cnt > 0)
                {
                    if(regC.Time_Out != null)
                    {
                        return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Time Out already Recorded");
                    }
                    else
                    {
                        register.Time_Out = time;
                        regC.Time_Out = time;
                        entities.SaveChanges();
                        return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, new String[] { "GREAT: "+time });
                    }
                }
                else
                {
                    double lati = Decimal.ToDouble(register.Latitude); 
                    double longi = Decimal.ToDouble(register.Longitude);  
                    
                    string[] tude = register.Location_In.Split(',');

                    var R = 6371; // km
                    var dLat = (lati2 - lati) * Math.PI / 180;
                    var dLon = (longi2 - longi) * Math.PI / 180;
                    var lat1 = (lati) * Math.PI / 180;
                    var lat2 = (lati2) * Math.PI / 180;

                    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Sin(dLon / 2) * Math.Sin(dLon / 2) * Math.Cos(lat1) * Math.Cos(lat2);
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    var d = R * c;

                    var new_reg = new Register_Form
                    {
                        Site_ID = site,
                        Employee_ID = emp,
                        Date = day,
                        Time_In = time,
                        Location_In = register.Location_In,
                        Distance_to_Site_Location = Convert.ToDecimal(d),
                        Latitude = register.Latitude,
                        Longitude = register.Longitude
                    };
                    entities.Register_Form.Add(new_reg);
                    entities.SaveChanges();

                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "Not Found, ADD: "+lati+", "+ longi + "..... " +d);
                }
            }
        }
    }
}
