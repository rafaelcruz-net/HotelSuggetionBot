using HotelSuggestionBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace HotelSuggestionBot.Dialogs
{
    [LuisModel("a5359520-e085-4478-9fe5-e45aa8171c9c", "d6ab20cd9daa4b38a1d455254e1fddb3")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        private const string EntityHotelName = "Hotel";

        private const string EntityAirportCode = "AirportCode";

        private IList<string> titleOptions = new List<string> { "“Muito bom hotel”", "“Otimo custo beneficio”", "“bom mais precisa de mais um reforma”", "“Aconchegante hotel.”", "“Otima surpresa”", "“Hotel de boas vibrações”" };

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            string message = $"Desculpa, Não consegui entender '{result.Query}'. Digite 'help' para ajuda.";

            await context.PostAsync(message);

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Hotel")]
        public async Task Search(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync($"Bem vindo ao buscador de hotel! Estamos verificando a sua mensagem: '{message.Text}'...");

            var hotelsQuery = new HotelsQuery();

            var hotelsFormDialog = new FormDialog<HotelsQuery>(hotelsQuery, this.BuildHotelsForm, FormOptions.PromptInStart, result.Entities);

            context.Call(hotelsFormDialog, this.ResumeAfterHotelsFormDialog);
        }

        [LuisIntent("HotelReviews")]
        public async Task Reviews(IDialogContext context, LuisResult result)
        {
            EntityRecommendation hotelEntityRecommendation;

            if (result.TryFindEntity(EntityHotelName, out hotelEntityRecommendation))
            {
                await context.PostAsync($"Buscando reviews of '{hotelEntityRecommendation.Entity}'...");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                for (int i = 0; i < new Random().Next(10); i++)
                {
                    var random = new Random(i);
                    ThumbnailCard thumbnailCard = new ThumbnailCard()
                    {
                        Title = this.titleOptions[random.Next(0, this.titleOptions.Count - 1)],
                        Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Mauris odio magna, sodales vel ligula sit amet, vulputate vehicula velit. Nulla quis consectetur neque, sed commodo metus.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = "https://upload.wikimedia.org/wikipedia/en/e/ee/Unknown-person.gif" }
                        },
                    };

                    resultMessage.Attachments.Add(thumbnailCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }

            context.Wait(this.MessageReceived);
        }

        [LuisIntent("Ajuda")]
        public async Task Help(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("Olá! Tente perguntar algo como 'Buscar hotel próximo ao aeroporto Galeão' ou 'Me mostre as reviews do Hotel Ibis'");

            context.Wait(this.MessageReceived);
        }

        private IForm<HotelsQuery> BuildHotelsForm()
        {
            OnCompletionAsyncDelegate<HotelsQuery> processHotelsSearch = async (context, state) =>
            {
                var message = "Pesquisando Hoteis";

                if (!string.IsNullOrEmpty(state.AirportCode))
                {
                    message += $" próximo ao aeroporo {state.AirportCode.ToUpperInvariant()}";
                }

                await context.PostAsync(message);
            };

            return new FormBuilder<HotelsQuery>()
                .Field(nameof(HotelsQuery.AirportCode), (state) => !string.IsNullOrEmpty(state.AirportCode))
                .OnCompletion(processHotelsSearch)
                .Build();
        }

        private async Task ResumeAfterHotelsFormDialog(IDialogContext context, IAwaitable<HotelsQuery> result)
        {
            try
            {
                var searchQuery = await result;

                var hotels = await this.GetHotelsAsync(searchQuery);

                await context.PostAsync($"Eu encontrei {hotels.Count()} hoteis: =]");

                var resultMessage = context.MakeMessage();
                resultMessage.AttachmentLayout = AttachmentLayoutTypes.Carousel;
                resultMessage.Attachments = new List<Attachment>();

                foreach (var hotel in hotels)
                {
                    HeroCard heroCard = new HeroCard()
                    {
                        Title = hotel.Name,
                        Subtitle = $"{hotel.Rating} stars. {hotel.NumberOfReviews} reviews. Preço ${hotel.PriceStarting} por noite.",
                        Images = new List<CardImage>()
                        {
                            new CardImage() { Url = hotel.Image }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Title = "Mais detalhes",
                                Type = ActionTypes.OpenUrl,
                                Value = $"https://www.bing.com/search?q=hotels+in+" + HttpUtility.UrlEncode(hotel.Location)
                            }
                        }
                    };

                    resultMessage.Attachments.Add(heroCard.ToAttachment());
                }

                await context.PostAsync(resultMessage);
            }
            catch (FormCanceledException ex)
            {
                string reply;

                if (ex.InnerException == null)
                {
                    reply = "Você cancelou a operação";
                }
                else
                {
                    reply = $"Oops! Saiu alto errado:( Technical Details: {ex.InnerException.Message}";
                }

                await context.PostAsync(reply);
            }
            finally
            {
                context.Done<object>(null);
            }
        }

        private async Task<IEnumerable<Hotel>> GetHotelsAsync(HotelsQuery searchQuery)
        {


            var hotels = new List<Hotel>();

            for (int i = 1; i <= new Random().Next(10); i++)
            {
                var random = new Random(i);
                Hotel hotel = new Hotel()
                {
                    Name = $"{searchQuery.AirportCode} Hotel {i}",
                    Location = searchQuery.AirportCode,
                    Rating = random.Next(1, 5),
                    NumberOfReviews = random.Next(0, 5000),
                    PriceStarting = random.Next(80, 450),
                    Image = $"https://placeholdit.imgix.net/~text?txtsize=35&txt=Hotel+{i}&w=500&h=260"
                };

                hotels.Add(hotel);
            }

            hotels.Sort((h1, h2) => h1.PriceStarting.CompareTo(h2.PriceStarting));

            return hotels;

        }
    }
}