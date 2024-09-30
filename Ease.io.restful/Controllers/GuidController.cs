using ease.io_lib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Ease.io.restful.Controllers
{


    public class UserModel
    {
        public string User { get; set; }
        public DateTime? Expires { get; set; } 
    }
    public class UpdateModel
    {
        public string User { get; set; }       
        public DateTime? Expires { get; set; } 
    }


    public class GuidController : ApiController
    {


        [HttpPost]
        [Route("api/guid/{guid?}")] 
        public IHttpActionResult Create([FromBody] UserModel userModel, string guid = null)
        {
            if (userModel == null || string.IsNullOrEmpty(userModel.User))
            {
                return BadRequest("Invalid user data");
            }
            Guid g = new Guid();

            if (string.IsNullOrEmpty(guid))
            {
                g = Guid.NewGuid(); // create one if it wasn't provided.
            }
            else
            {
                g = Guid.Parse(guid);
            }

            if (userModel.Expires == null)
            {
                userModel.Expires = DateTime.UtcNow.AddDays(30);
            }

            EaseLib lib = new EaseLib();
            lib.Add(userModel.User, g, (DateTime)userModel.Expires);

            var response = new
            {
                guid = g,
                user = userModel.User,
                expires = userModel.Expires
            };
            return Ok(response);
        }

        [HttpGet]
        [Route("api/guid/{guid}")]
        public IHttpActionResult Read(string guid)
        {
            string key = $"User:{guid}";

            EaseLib lib = new EaseLib();

            User u = lib.GetUser(guid);
            
            if (u==null)
            {
                return NotFound();
            }
            
            return Ok(u);
        }

        [HttpPut]
        [Route("api/guid/{guid}")]
        public IHttpActionResult Update(string guid, [FromBody] UpdateModel updateModel)
        {
            if (updateModel == null)
            {
                return BadRequest("Invalid data");
            }

            if (string.IsNullOrEmpty (guid)) 
            {
                return BadRequest("Missing guid");        
            }

            EaseLib lib = new EaseLib();
            User existingUser = lib.GetUser(guid);

            if (existingUser==null)
            {
                return BadRequest("No existing user found to update.");
            }

            string updatedUser = updateModel.User ?? existingUser.user;
            DateTime? updatedExpires = updateModel.Expires ?? existingUser.expires;

            lib.Update(Guid.Parse(guid), updatedUser, (DateTime)updatedExpires);

            var response = new
            {
                guid = guid,
                user = updatedUser,
                expires = updatedExpires
            };


            return Ok(response);
        }
        [HttpDelete]
        [Route("api/guid/{guid}")]
        public IHttpActionResult Delete(string guid)
        {

            EaseLib lib = new EaseLib();
            bool wasDeleted = lib.Delete(Guid.Parse(guid));
            if (wasDeleted)
            {
                return Ok($"User with GUID {guid} deleted successfully.");
            }

            return NotFound();
        }


    }
}
