﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HotelSuggestionBot.Models
{
    [Serializable]
    public class Hotel
    {
        public string Name { get; set; }

        public int Rating { get; set; }

        public int NumberOfReviews { get; set; }

        public int PriceStarting { get; set; }

        public string Image { get; set; }

        public string Location { get; set; }
    }
}