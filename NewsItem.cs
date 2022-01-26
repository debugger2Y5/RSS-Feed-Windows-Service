using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K181185_QS2
{
    class NewsItem
    {
        //<NewsItem>
        //  <Title></Title>
        //  <Description></Description>
        //  <PublishedDate></PublishedDate>
        //  <NewsChannel></NewsChannel>
        //</NewsItem>

        public string Title { get; set; }
        public string Description { get; set; }
        public string PublishedDate { get; set; }
        public string NewsChannel { get; set; }

        public NewsItem()
        {

        }
    }
}
