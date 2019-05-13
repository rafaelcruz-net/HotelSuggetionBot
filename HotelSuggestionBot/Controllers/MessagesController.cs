using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using HotelSuggestionBot.Dialogs;
using HotelSuggestionBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace HotelSuggestionBot
{
    [BotAuthentication()]
    public class MessagesController : ApiController
    {
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                try
                {
                    await Conversation.SendAsync(activity, () => new RootLuisDialog());
                }
                catch (System.Exception ex)
                {
                    Trace.WriteLine(ex);
                }
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);

            return response;
        }
    }
}