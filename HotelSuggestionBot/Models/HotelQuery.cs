using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelSuggestionBot.Models
{
    [Serializable]
    public class HotelsQuery
    {
        [Prompt("Próximo a qual aeroporto")]
        [Optional]
        public string AirportCode { get; set; }
    }
}