using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WAConectorAPI.Models.Apis
{
    public class infoArt
    {
        public string ProductDescription { get; set; }
        public string BrandName { get; set; }
        public string ImageUrl { get; set; }
        public List<Images> Images { get; set; }

    }
    public class Images
    {
        public string ImageUrl { get; set; }
    }
}