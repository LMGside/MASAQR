using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MASAQR.Controllers
{
    public class LoginController : ApiController
    {
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Login))]
        [HttpGet]
        public HttpResponseMessage Get()
        {
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var res = entities.Login;

                return ControllerContext.Request.CreateResponse(HttpStatusCode.OK, res.ToList());
            }
        }

        /// <summary>
        /// Get Users Email and Password
        /// </summary>
        /// <param name="username">User's Email</param>
        /// <param name="password">User's Password</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Login))]
        [Route("api/Login/{username}/{password}")]
        public HttpResponseMessage Get(String username, String password)
        {
            ArrayList strArr = new System.Collections.ArrayList();
            using (qrMasterEntities entities = new qrMasterEntities())
            {
                var query = from userD in entities.Login.AsEnumerable()
                            where userD.Username == username && userD.Password == password
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
