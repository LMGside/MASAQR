using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Swashbuckle.Swagger.Annotations;

namespace MASAQR.Controllers
{
    public class UserController : ApiController
    {
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var k = entities.User.ToList();
                var quan = k.Count();

                if (quan > 0)
                {
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, k);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error, no Users Found");
                }
            }
        }

        [Route("api/User/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [HttpGet]
        public HttpResponseMessage Get(String id)
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.User.FirstOrDefault(e => e.Employee_ID.Equals(id));
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

        [Route("api/User/{username}/{password}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(User))]
        [HttpGet]
        public HttpResponseMessage Get(String username, String password)
        {
            ArrayList strArr = new System.Collections.ArrayList();
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = from userD in entities.User.AsEnumerable()
                            where userD.Username == username
                            select userD;
                int cnt = query.Count();

                if (cnt > 0)
                {
                    foreach (var element in query)
                    {
                        strArr.Add(element);
                    }
                    return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, strArr);
                }
                else
                {
                    return ControllerContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "User Not Found");
                }
            }
        }
    }
}
