using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;

namespace Tsukihi.Services
{
    public class GelbooruService
    {
        static Random random = new Random();

        public static string GetRandomImage(string[] tags)
        {
            StringBuilder urlBuilder = new StringBuilder();
            tags = tags.Select(tag => $"*{HttpUtility.UrlEncode(tag)}*").ToArray();
            urlBuilder.AppendFormat("https://gelbooru.com/index.php?page=dapi&s=post&q=index&names={0}", string.Join("%20", tags));
            try
            {
                int count = -1;
                using (XmlReader reader = XmlReader.Create(urlBuilder.ToString()))
                {
                    while (reader.Read())
                    {
                        if (reader.Name == "posts")
                        {
                            reader.MoveToAttribute(0);
                            count = int.Parse(reader.Value);
                            break;
                        }
                    }
                }
                if (count == -1) return null;

                int pages = (int)Math.Ceiling(count / 100.0);
                int page = random.Next(0, pages);
                urlBuilder.AppendFormat("&p={0}", page);
                List<string> images = new List<string>();
                using (XmlReader reader = XmlReader.Create(urlBuilder.ToString()))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element && reader.Name == "post")
                        {
                            reader.MoveToAttribute(2);
                            images.Add(reader.Value);
                        }
                    }
                }
             return images[random.Next(0, images.Count)];
            }
            catch (WebException e)
            {
                return null;
            }
        }
    }
}