using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;

namespace HotelSuggestionBot.Services
{
    public class SpellCheckService
    {
        private const string SpellCheckApiUrl = "https://api.cognitive.microsoft.com/bing/v5.0/spellcheck/?form=BCSSCK";
        private static readonly string ApiKey = WebConfigurationManager.AppSettings["SpellCheckApiKey"];
        public async Task<string> GetCorrectedTextAsync(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey);

                var values = new Dictionary<string, string>
                {
                    { "text", text }
                };

                var content = new FormUrlEncodedContent(values);

                var response = await client.PostAsync(SpellCheckApiUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                var spellCheckResponse = JsonConvert.DeserializeObject<BingSpellCheckResponse>(responseString);

                StringBuilder sb = new StringBuilder();
                int previousOffset = 0;

                foreach (var flaggedToken in spellCheckResponse.FlaggedTokens)
                {
                    sb.Append(text.Substring(previousOffset, flaggedToken.Offset - previousOffset));
                    sb.Append(flaggedToken.Suggestions.First().Suggestion);
                    previousOffset = flaggedToken.Offset + flaggedToken.Token.Length;
                }

                if (previousOffset < text.Length)
                {
                    sb.Append(text.Substring(previousOffset));
                }

                return sb.ToString();
            }
        }
    }

    public class BingSpellCheckResponse
    {
        [JsonProperty("_type")]
        public string Type { get; set; }

        public BingSpellCheckFlaggedToken[] FlaggedTokens { get; set; }

        public BingSpellCheckError Error { get; set; }
    }

    public class BingSpellCheckFlaggedToken
    {
        public int Offset { get; set; }

        public string Token { get; set; }

        public string Type { get; set; }

        public BingSpellCheckSuggestion[] Suggestions { get; set; }
    }

    public class BingSpellCheckError
    {
        public int StatusCode { get; set; }

        public string Message { get; set; }
    }

    public class BingSpellCheckSuggestion
    {
        public string Suggestion { get; set; }

        public double Score { get; set; }
    }

}