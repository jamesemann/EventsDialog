using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace EventsDialog.Meetup.Controllers
{
    public class AuthController: ApiController
    {
        public async Task<HttpResponseMessage> Get(string userId, string code) 
        {
            var accessToken = await OAuthUtility.GetAccessToken(userId, code);
            TableStorageService.InsertOrUpdate(accessToken);

            var response = new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent("Thank you, you may now close this window.")
            };
            return response;
        }
    }
}
