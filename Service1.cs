using System;
using System.Configuration;
using System.ServiceProcess;
using System.ServiceModel.Syndication;
using System.Xml;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace K181185_QS2
{
    //By: K18-1185
    //Sec - F
    public partial class Service1 : ServiceBase 
    {
        public string path = ConfigurationManager.AppSettings["path"];
        readonly int ScheduleInterval = Convert.ToInt32(ConfigurationManager.AppSettings["ThreadTime"]);
        public Thread worker = null; //to manage intervals of 5 minutes

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                ThreadStart start = new ThreadStart(Working); //run working function on start
                worker = new Thread(start);
                worker.Start();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void Working()
        {
            while (true)
            {
                ReadRSSFeed(); //function that performs the main task
                Thread.Sleep(ScheduleInterval * 60 * 1000); //sleep for 5 minutes
            }
        }

        protected override void OnStop()
        {
            try
            {
                if ((worker != null) & worker.IsAlive)
                {
                    worker.Abort();
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ReadRSSFeed()
        {
            //chosen news rss feeds to read
            string url1 = "https://tribune.com.pk/feed/pakistan"; //Tribune - Pakistan News
            string url2 = "https://www.thenews.com.pk/rss/1/1"; //The News - Pakistan News
            //inorder to overwrite the file everytime
            File.Delete(path);


            //read and write rss into file of each url one by one
            ReadRSSFeedURLs(url1, 1, path);
            ReadRSSFeedURLs(url2, 2, path);
            //1 and 2 is used to identify which news channel it is
        }


        //worker function to read RSS feeds
        private void ReadRSSFeedURLs(string url, int num, string path)
        {

            List<NewsItem> newsItemObj = new List<NewsItem>();
            XDocument xDoc = XDocument.Load(url);

            string channel;
            if (num == 1)
            {
                channel = "Tribune";
            }
            else
            {
                channel = "The News";
            }

            //query performed to sort newsItems in descending order by pubDate
            var items = (from item in xDoc.Descendants("item")
                         orderby (DateTime)item.Element("pubDate") descending
                         select new
                         {
                             Title = item.Element("title").Value,
                             Description = item.Element("description").Value,
                             PublishedDate = item.Element("pubDate").Value,
                             NewsChannel = channel
                         });
            try
            {
                foreach (var x in items)
                {
                    //initializing class NewsItem object and saving data there for each newsItem tag
                    NewsItem newsItem = new NewsItem
                    {
                        Title = x.Title,
                        Description = x.Description,
                        PublishedDate = x.PublishedDate,//, CultureInfo.GetCultureInfo("hi-IN").DateTimeFormat),
                        NewsChannel = x.NewsChannel
                    };
                    //function call to write object data to xml file
                    WriteToFile(newsItem, path);
                }
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        private void WriteToFile(NewsItem newsItemObj, string path)
        {
            if (File.Exists(path) == false)
            {
                //file does not exist so we will create one

                XmlDocument x = new XmlDocument();
                XmlDeclaration xmlDeclaration = x.CreateXmlDeclaration("1.0", "UTF-8", null);
                XmlElement root = x.DocumentElement;
                x.InsertBefore(xmlDeclaration, root);
                XmlElement element1 = x.CreateElement(string.Empty, "NewsItems", string.Empty);
                x.AppendChild(element1);
                x.Save(path);

                XmlNode root1 = x.SelectSingleNode("NewsItems");
                XmlElement newsItem = x.CreateElement("NewsItem");
                root1.AppendChild(newsItem);

                XmlElement Title = x.CreateElement("Title");
                newsItem.AppendChild(Title);
                Title.InnerText = newsItemObj.Title;

                XmlElement Description = x.CreateElement("Description");
                newsItem.AppendChild(Description);
                Description.InnerText = newsItemObj.Description;

                XmlElement PublishedDate = x.CreateElement("PublishedDate");
                newsItem.AppendChild(PublishedDate);
                PublishedDate.InnerText = newsItemObj.PublishedDate;

                XmlElement NewsChannel = x.CreateElement("NewsChannel");
                newsItem.AppendChild(NewsChannel);
                NewsChannel.InnerText = newsItemObj.NewsChannel;

                x.Save(path);
            }
            else
            {
                //file already exists so we will append

                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode root = doc.SelectSingleNode("NewsItems");
                XmlElement NewsItem = doc.CreateElement("NewsItem");
                root.AppendChild(NewsItem);

                XmlElement Title = doc.CreateElement("Title");
                NewsItem.AppendChild(Title);
                Title.InnerText = newsItemObj.Title;

                XmlElement Description = doc.CreateElement("Description");
                NewsItem.AppendChild(Description);
                Description.InnerText = newsItemObj.Description;

                XmlElement PublishedDate = doc.CreateElement("PublishedDate");
                NewsItem.AppendChild(PublishedDate);
                PublishedDate.InnerText = newsItemObj.PublishedDate;

                XmlElement NewsChannel = doc.CreateElement("NewsChannel");
                NewsItem.AppendChild(NewsChannel);
                NewsChannel.InnerText = newsItemObj.NewsChannel;

                doc.Save(path);

            }
        }
    }
}
