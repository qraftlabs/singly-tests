using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Services.Client;

namespace Singly.Tests
{
    [TestClass]
    public class Tests
    {
        private Singly.SinglyContext context { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            context = new Singly.SinglyContext(new Uri("https://odata.singly.com/types"));
            context.UseJsonFormatWithDefaultServiceModel();
            context.IgnoreMissingProperties = true;
            context.SendingRequest += new EventHandler<System.Data.Services.Client.SendingRequestEventArgs>(Context_SendingRequest);
        }

        void Context_SendingRequest(object sender, System.Data.Services.Client.SendingRequestEventArgs e)
        {
            e.RequestHeaders.Add("Authorization", "Bearer UOmcFTvBlvFhiFVWFTFqF1tcwSA=U0N7obhYa15bf15888f5ed4a883b10b7932989bd8e1df691c5c64df24f7740b145e95e6006aca820e766c3058005bf24948be04898463f0a956099c7874f75ca9932d53bee07d73baab71c50e7d57226b8fa83dfb996d25d84c1a33e5e23b21355a4a361");
        }

        [TestMethod]
        public void Should_Query_Statuses()
        {
            var list = context.Statuses.ToArray();

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Length > 0);
            //Assert.AreEqual("{\"created_at\":\"Mon Jul 23 18:17:32 +0000 2012\",\"id\":227467496784011260,\"id_str\":\"227467496784011266\",\"text\":\"Does anybody know a good Fiddler-like tool for linux?\",\"source\":\"web\",\"truncated\":false,\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":67699724,\"id_str\":\"67699724\",\"name\":\"Gustavo Machado\",\"screen_name\":\"machadogj\",\"location\":\"Buenos Aires\",\"url\":null,\"description\":\"\",\"protected\":false,\"followers_count\":80,\"friends_count\":199,\"listed_count\":7,\"created_at\":\"Fri Aug 21 19:47:50 +0000 2009\",\"favourites_count\":2,\"utc_offset\":-10800,\"time_zone\":\"Buenos Aires\",\"geo_enabled\":false,\"verified\":false,\"statuses_count\":472,\"lang\":\"en\",\"contributors_enabled\":false,\"is_translator\":false,\"profile_background_color\":\"709397\",\"profile_background_image_url\":\"http://a0.twimg.com/images/themes/theme6/bg.gif\",\"profile_background_image_url_https\":\"https://si0.twimg.com/images/themes/theme6/bg.gif\",\"profile_background_tile\":false,\"profile_image_url\":\"http://a0.twimg.com/profile_images/2562524889/nsmp7hxoxu6onlauicb9_normal.jpeg\",\"profile_image_url_https\":\"https://si0.twimg.com/profile_images/2562524889/nsmp7hxoxu6onlauicb9_normal.jpeg\",\"profile_link_color\":\"FF3300\",\"profile_sidebar_border_color\":\"86A4A6\",\"profile_sidebar_fill_color\":\"A0C5C7\",\"profile_text_color\":\"333333\",\"profile_use_background_image\":true,\"default_profile\":false,\"default_profile_image\":false,\"following\":false,\"follow_request_sent\":false,\"notifications\":false},\"geo\":null,\"coordinates\":null,\"place\":null,\"contributors\":null,\"retweet_count\":0,\"entities\":{\"hashtags\":[],\"urls\":[],\"user_mentions\":[]},\"favorited\":false,\"retweeted\":false}",
            //                  list.Last().Data);
            Assert.AreEqual(new DateTime(2012, 07, 23, 18, 17, 32), list.Last().Date);
            Assert.AreEqual("twitter", list.Last().Oembed.SourceName);
            Assert.AreEqual("Does anybody know a good Fiddler-like tool for linux?", list.Last().Oembed.Text);
            Assert.AreEqual("text", list.Last().Oembed.Type);
            Assert.AreEqual("1.0", list.Last().Oembed.Version);
        }

        [TestMethod]
        public void Should_Query_With_Top()
        {
            var list = context.Statuses.Take(2).ToArray();

            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Length);
        }

        [TestMethod]
        public void Should_Query_With_Date()
        {
            var full = context.Statuses.Take(3).ToArray();
            var second = full[0].Date;

            var list = context
                        .Statuses
                        .Where(s => s.Date > second)
                        .ToArray();

            Assert.AreEqual(1, list.Length);
        }

        [TestMethod]
        public void Should_Query_With_Contains()
        {
            // search mentions
            var list = context
                        .Statuses
                        .Where(s => s.Data.Contains("@woloski"))
                        .ToArray();

            Assert.AreEqual(6, list.Length);
        }

        [TestMethod]
        public void Should_Query_With_LessThan()
        {
            var list = context
                .Statuses
                .Where(s => s.Date < DateTime.Now.AddDays(10))
                .ToArray();

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Length > 0);
        }

        [TestMethod]
        public void Should_Query_With_LessThanAndGreaterThan()
        {
            // all the tweets done from 2012-07-23 and 24 (3 in total)
            var list = context
                .Statuses
                .Where(s => s.Date > new DateTime(2012, 07, 23) &&
                            s.Date < new DateTime(2012, 07, 24))
                .ToArray();

            Assert.IsNotNull(list);
            Assert.AreEqual(3, list.Length);
        }
 
        [TestMethod]
        public void Should_Not_Get_By_Id()
        {
            try
            {
                context.Statuses.Where(s => s.Id == "foo").ToArray();
            }
            catch (System.Data.Services.Client.DataServiceQueryException e)
            {
                /* TODO: The OData client is not deserializing the error as per the OData JSON
                 * light specification: http://www.odata.org/media/30002/OData%20JSON%20Verbose%20Format.html
                 *
                 * Assert.AreEqual("resource path is not valid. get by id is not supported yet.", e.Message);
                 */
                Assert.IsTrue(e.InnerException.Message.Contains("resource path is not valid. get by id is not supported yet."), e.InnerException.Message);
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        public void Should_Receive_MeaningfulError_When_Unsupported_Operator()
        {
            try
            {
                context.Statuses.Where(s => s.Data == "foo").ToArray();
            }
            catch (DataServiceQueryException e)
            {
                /* TODO: The OData client is not deserializing the error as per the OData JSON
                 * light specification: http://www.odata.org/media/30002/OData%20JSON%20Verbose%20Format.html
                 *
                 * {"error":{"code":"query-parse-error","message":{"lang":"en-us","value":"Unable to parse OData query. operator eq not supported yet."}}}
                 *  
                 * Assert.AreEqual("Unable to parse OData query. operator eq not supported yet.", e.Message); 
                 */
                Assert.IsTrue(e.InnerException.Message.Contains("Unable to parse OData query. operator eq not supported yet."));
                return;
            }

            Assert.Fail();
        }
        /*
         * Serializationi Tests
         */

        [TestMethod]
        public void Should_Query_Checkins()
        {
            var list = context.Checkins.ToArray();

            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void Should_Query_Contacts()
        {
            var list = context.Contacts.ToArray();

            Assert.IsNotNull(list);
            //Assert.AreEqual("{\"id_str\":\"240751001\",\"profile_background_color\":\"FFFFFF\",\"id\":240751001,\"notifications\":false,\"url\":\"http://nodester.com\",\"follow_request_sent\":false,\"created_at\":\"Thu Jan 20 17:16:40 +0000 2011\",\"screen_name\":\"Nodester\",\"profile_background_image_url\":\"http://a0.twimg.com/profile_background_images/338956509/nodesterrocket.png\",\"statuses_count\":2986,\"default_profile_image\":false,\"verified\":false,\"profile_link_color\":\"0084B4\",\"utc_offset\":-25200,\"favourites_count\":23,\"friends_count\":2541,\"name\":\"Nodester\",\"lang\":\"en\",\"profile_use_background_image\":false,\"location\":\"NodeJS PaaS\",\"protected\":false,\"profile_text_color\":\"333333\",\"profile_image_url\":\"http://a0.twimg.com/profile_images/1543095199/ndoesterrocket_normal.png\",\"profile_image_url_https\":\"https://si0.twimg.com/profile_images/1543095199/ndoesterrocket_normal.png\",\"description\":\"NodeJS Open Source Hosting Platform\",\"profile_sidebar_border_color\":\"C0DEED\",\"default_profile\":false,\"followers_count\":2627,\"is_translator\":false,\"following\":true,\"contributors_enabled\":false,\"profile_background_image_url_https\":\"https://si0.twimg.com/profile_background_images/338956509/nodesterrocket.png\",\"time_zone\":\"Arizona\",\"profile_background_tile\":false,\"profile_sidebar_fill_color\":\"DDEEF6\",\"geo_enabled\":false,\"listed_count\":167,\"status\":{\"favorited\":false,\"in_reply_to_status_id_str\":null,\"coordinates\":null,\"in_reply_to_screen_name\":null,\"in_reply_to_user_id_str\":null,\"geo\":null,\"entities\":{\"user_mentions\":[],\"urls\":[],\"hashtags\":[]},\"place\":null,\"retweeted\":false,\"in_reply_to_status_id\":null,\"source\":\"<a href=\\\"http://www.tweetdeck.com\\\" rel=\\\"nofollow\\\">TweetDeck</a>\",\"id_str\":\"259273047779966977\",\"contributors\":null,\"retweet_count\":3,\"id\":259273047779966980,\"in_reply_to_user_id\":null,\"truncated\":false,\"text\":\"Hack the planet!\",\"created_at\":\"Fri Oct 19 12:41:26 +0000 2012\"}}"
            //                , list.Last().Data);
            Assert.AreEqual(new DateTime(2011, 01, 20, 17, 16, 40), list.Last().Date);
            Assert.AreEqual("NodeJS Open Source Hosting Platform", list.Last().Oembed.Description);
            Assert.AreEqual("twitter", list.Last().Oembed.ProviderName);
            Assert.AreEqual("twitter", list.Last().Oembed.SourceName);
            Assert.AreEqual("https://si0.twimg.com/profile_images/1543095199/ndoesterrocket.png", list.Last().Oembed.ThumbnailUrl);
            Assert.AreEqual("Nodester", list.Last().Oembed.Title);
            Assert.AreEqual("contact", list.Last().Oembed.Type);
            Assert.AreEqual("http://twitter.com/Nodester", list.Last().Oembed.Url);
            Assert.AreEqual("1.0", list.Last().Oembed.Version);
        }

        [TestMethod]
        public void Should_Query_News()
        {
            var list = context.News.ToArray();

            Assert.IsNotNull(list);
            //Assert.AreEqual("{\"created_at\":\"Fri Jul 13 17:56:08 +0000 2012\",\"id\":223838232570372100,\"id_str\":\"223838232570372096\",\"text\":\"RT @ChrisLove: I pushed my 1st GitHub repository http://t.co/VFre4264 Used GitHub for Windows, The Code my tiles blog post http://t.co/v ...\",\"source\":\"web\",\"truncated\":false,\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":67699724,\"id_str\":\"67699724\",\"name\":\"Gustavo Machado\",\"screen_name\":\"machadogj\",\"location\":\"Buenos Aires\",\"url\":null,\"description\":\"\",\"protected\":false,\"followers_count\":80,\"friends_count\":199,\"listed_count\":7,\"created_at\":\"Fri Aug 21 19:47:50 +0000 2009\",\"favourites_count\":2,\"utc_offset\":-10800,\"time_zone\":\"Buenos Aires\",\"geo_enabled\":false,\"verified\":false,\"statuses_count\":472,\"lang\":\"en\",\"contributors_enabled\":false,\"is_translator\":false,\"profile_background_color\":\"709397\",\"profile_background_image_url\":\"http://a0.twimg.com/images/themes/theme6/bg.gif\",\"profile_background_image_url_https\":\"https://si0.twimg.com/images/themes/theme6/bg.gif\",\"profile_background_tile\":false,\"profile_image_url\":\"http://a0.twimg.com/profile_images/2562524889/nsmp7hxoxu6onlauicb9_normal.jpeg\",\"profile_image_url_https\":\"https://si0.twimg.com/profile_images/2562524889/nsmp7hxoxu6onlauicb9_normal.jpeg\",\"profile_link_color\":\"FF3300\",\"profile_sidebar_border_color\":\"86A4A6\",\"profile_sidebar_fill_color\":\"A0C5C7\",\"profile_text_color\":\"333333\",\"profile_use_background_image\":true,\"default_profile\":false,\"default_profile_image\":false,\"following\":false,\"follow_request_sent\":false,\"notifications\":false},\"geo\":null,\"coordinates\":null,\"place\":null,\"contributors\":null,\"retweeted_status\":{\"created_at\":\"Fri Jul 13 17:31:09 +0000 2012\",\"id\":223831948739624960,\"id_str\":\"223831948739624960\",\"text\":\"I pushed my 1st GitHub repository http://t.co/VFre4264 Used GitHub for Windows, The Code my tiles blog post http://t.co/vQaH6996\",\"source\":\"<a href=\\\"http://www.metrotwit.com/\\\" rel=\\\"nofollow\\\">MetroTwit</a>\",\"truncated\":false,\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":5658652,\"id_str\":\"5658652\",\"name\":\"Chris Love\",\"screen_name\":\"ChrisLove\",\"location\":\"Here\",\"url\":\"http://www.ProfessionalASPNET.com\",\"description\":\"Passionate about Mobile Web, ASP.NET, jQuery, HTML5, CSS3\",\"protected\":false,\"followers_count\":2497,\"friends_count\":1401,\"listed_count\":157,\"created_at\":\"Mon Apr 30 16:20:57 +0000 2007\",\"favourites_count\":32,\"utc_offset\":-18000,\"time_zone\":\"Eastern Time (US & Canada)\",\"geo_enabled\":true,\"verified\":false,\"statuses_count\":25446,\"lang\":\"en\",\"contributors_enabled\":false,\"is_translator\":false,\"profile_background_color\":\"CEECEC\",\"profile_background_image_url\":\"http://a0.twimg.com/profile_background_images/2430769/starblazers-warping.jpg\",\"profile_background_image_url_https\":\"https://si0.twimg.com/profile_background_images/2430769/starblazers-warping.jpg\",\"profile_background_tile\":true,\"profile_image_url\":\"http://a0.twimg.com/profile_images/492735348/Chris_Love_Close_Up_normal_normal.jpg\",\"profile_image_url_https\":\"https://si0.twimg.com/profile_images/492735348/Chris_Love_Close_Up_normal_normal.jpg\",\"profile_link_color\":\"0000FF\",\"profile_sidebar_border_color\":\"87BC44\",\"profile_sidebar_fill_color\":\"E0FF92\",\"profile_text_color\":\"000000\",\"profile_use_background_image\":true,\"default_profile\":false,\"default_profile_image\":false,\"following\":null,\"follow_request_sent\":false,\"notifications\":null},\"geo\":null,\"coordinates\":null,\"place\":null,\"contributors\":null,\"retweet_count\":2,\"entities\":{\"hashtags\":[],\"urls\":[{\"url\":\"http://t.co/VFre4264\",\"expanded_url\":\"http://bit.ly/MqqRME\",\"display_url\":\"bit.ly/MqqRME\",\"indices\":[34,54]},{\"url\":\"http://t.co/vQaH6996\",\"expanded_url\":\"http://bit.ly/MqqUYK\",\"display_url\":\"bit.ly/MqqUYK\",\"indices\":[108,128]}],\"user_mentions\":[]},\"favorited\":false,\"retweeted\":false,\"possibly_sensitive\":false},\"retweet_count\":2,\"entities\":{\"hashtags\":[],\"urls\":[{\"url\":\"http://t.co/VFre4264\",\"expanded_url\":\"http://bit.ly/MqqRME\",\"display_url\":\"bit.ly/MqqRME\",\"indices\":[49,69]}],\"user_mentions\":[{\"screen_name\":\"ChrisLove\",\"name\":\"Chris Love\",\"id\":5658652,\"id_str\":\"5658652\",\"indices\":[3,13]}]},\"favorited\":false,\"retweeted\":false,\"possibly_sensitive\":false}", 
            //                list.Last().Data);
            Assert.AreEqual(new DateTime(2012, 07, 24, 19, 14, 53), list.Last().Date);
            Assert.AreEqual(null, list.Last().Oembed.AuthorName);
            Assert.AreEqual(null, list.Last().Oembed.AuthorUrl);
            Assert.AreEqual(null, list.Last().Oembed.CacheAge);
            Assert.AreEqual("Simonwillison", list.Last().Oembed.ProviderName);
            Assert.AreEqual("http://simonwillison.net", list.Last().Oembed.ProviderUrl);
            Assert.AreEqual("twitter", list.Last().Oembed.SourceName);
            Assert.AreEqual(337, list.Last().Oembed.ThumbnailHeight);
            Assert.AreEqual("http://simonwillison.net/static/2010/redis-tutorial/redis.004.jpg", list.Last().Oembed.ThumbnailUrl);
            Assert.AreEqual(450, list.Last().Oembed.ThumbnailWidth);
            Assert.AreEqual("Redis tutorial, April 2010 - by Simon Willison", list.Last().Oembed.Title);
            Assert.AreEqual("link", list.Last().Oembed.Type);
            Assert.AreEqual("1.0", list.Last().Oembed.Version);
        }

        [TestMethod]
        public void Should_Query_Photos()
        {
            var list = context.Photos.ToArray();

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Length > 0);

            //Assert.AreEqual("{\"created_at\":\"Wed Sep 07 14:43:05 +0000 2011\",\"id\":111449411963457540,\"id_str\":\"111449411963457536\",\"text\":\"Check out my result from Speedtest.net! http://t.co/NvOwpbi\",\"source\":\"<a href=\\\"http://twitter.com/tweetbutton\\\" rel=\\\"nofollow\\\">Tweet Button</a>\",\"truncated\":false,\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":67699724,\"id_str\":\"67699724\",\"name\":\"Gustavo Machado\",\"screen_name\":\"machadogj\",\"location\":\"Buenos Aires\",\"url\":null,\"description\":\"\",\"protected\":false,\"followers_count\":80,\"friends_count\":199,\"listed_count\":7,\"created_at\":\"Fri Aug 21 19:47:50 +0000 2009\",\"favourites_count\":2,\"utc_offset\":-10800,\"time_zone\":\"Buenos Aires\",\"geo_enabled\":false,\"verified\":false,\"statuses_count\":472,\"lang\":\"en\",\"contributors_enabled\":false,\"is_translator\":false,\"profile_background_color\":\"709397\",\"profile_background_image_url\":\"http://a0.twimg.com/images/themes/theme6/bg.gif\",\"profile_background_image_url_https\":\"https://si0.twimg.com/images/themes/theme6/bg.gif\",\"profile_background_tile\":false,\"profile_image_url\":\"http://a0.twimg.com/profile_images/2562524889/nsmp7hxoxu6onlauicb9_normal.jpeg\",\"profile_image_url_https\":\"https://si0.twimg.com/profile_images/2562524889/nsmp7hxoxu6onlauicb9_normal.jpeg\",\"profile_link_color\":\"FF3300\",\"profile_sidebar_border_color\":\"86A4A6\",\"profile_sidebar_fill_color\":\"A0C5C7\",\"profile_text_color\":\"333333\",\"profile_use_background_image\":true,\"default_profile\":false,\"default_profile_image\":false,\"following\":false,\"follow_request_sent\":false,\"notifications\":false},\"geo\":null,\"coordinates\":null,\"place\":null,\"contributors\":null,\"retweet_count\":0,\"entities\":{\"hashtags\":[],\"urls\":[{\"url\":\"http://t.co/NvOwpbi\",\"expanded_url\":\"http://speedtest.net/result/1470802413.png\",\"display_url\":\"speedtest.net/result/1470802…\",\"indices\":[40,59]}],\"user_mentions\":[]},\"favorited\":false,\"retweeted\":false,\"possibly_sensitive\":false}",
            //                 list.Last().Data);
            Assert.AreEqual(new DateTime(2011, 04, 27, 18, 57, 07), list.Last().Date);
            Assert.AreEqual(null, list.Last().Oembed.CacheAge);
            Assert.AreEqual(1920, list.Last().Oembed.Height);
            Assert.AreEqual("Twitpic", list.Last().Oembed.ProviderName);
            Assert.AreEqual("http://twitpic.com", list.Last().Oembed.ProviderUrl);
            Assert.AreEqual(150, list.Last().Oembed.ThumbnailHeight);
            Assert.AreEqual("http://twitpic.com/show/thumb/4q823h", list.Last().Oembed.ThumbnailUrl);
            Assert.AreEqual(150, list.Last().Oembed.ThumbnailWidth);
            Assert.AreEqual("A photo from @MCarvalhoF", list.Last().Oembed.Title);
            Assert.AreEqual("http://twitpic.com/show/full/4q823h", list.Last().Oembed.Url);
            Assert.AreEqual("1.0", list.Last().Oembed.Version);
            Assert.AreEqual(2560, list.Last().Oembed.Width);
        }


        [TestMethod]
        public void Should_Query_Videos()
        {
            var list = context.Videos.ToArray();

            //Assert.AreEqual("{\"created_at\":\"Wed Oct 14 17:42:00 +0000 2009\",\"id\":4867311022,\"id_str\":\"4867311022\",\"text\":\"Echa un vistazo a este vídeo. -- Pearl Jam-Black http://bit.ly/68RlZ\",\"source\":\"web\",\"truncated\":false,\"in_reply_to_status_id\":null,\"in_reply_to_status_id_str\":null,\"in_reply_to_user_id\":null,\"in_reply_to_user_id_str\":null,\"in_reply_to_screen_name\":null,\"user\":{\"id\":67699724,\"id_str\":\"67699724\",\"name\":\"Gustavo Machado\",\"screen_name\":\"machadogj\",\"location\":\"Buenos Aires\",\"url\":null,\"description\":\"\",\"protected\":false,\"followers_count\":80,\"friends_count\":199,\"listed_count\":7,\"created_at\":\"Fri Aug 21 19:47:50 +0000 2009\",\"favourites_count\":2,\"utc_offset\":-10800,\"time_zone\":\"Buenos Aires\",\"geo_enabled\":false,\"verified\":false,\"statuses_count\":472,\"lang\":\"en\",\"contributors_enabled\":false,\"is_translator\":false,\"profile_background_color\":\"709397\",\"profile_background_image_url\":\"http://a0.twimg.com/images/themes/theme6/bg.gif\",\"profile_background_image_url_https\":\"https://si0.twimg.com/images/themes/theme6/bg.gif\",\"profile_background_tile\":false,\"profile_image_url\":\"http://a0.twimg.com/profile_images/2562524889/nsmp7hxoxu6onlauicb9_normal.jpeg\",\"profile_image_url_https\":\"https://si0.twimg.com/profile_images/2562524889/nsmp7hxoxu6onlauicb9_normal.jpeg\",\"profile_link_color\":\"FF3300\",\"profile_sidebar_border_color\":\"86A4A6\",\"profile_sidebar_fill_color\":\"A0C5C7\",\"profile_text_color\":\"333333\",\"profile_use_background_image\":true,\"default_profile\":false,\"default_profile_image\":false,\"following\":false,\"follow_request_sent\":false,\"notifications\":false},\"geo\":null,\"coordinates\":null,\"place\":null,\"contributors\":null,\"retweet_count\":0,\"entities\":{\"hashtags\":[],\"urls\":[],\"user_mentions\":[]},\"favorited\":false,\"retweeted\":false}",
            //                    list.Last().Data);
            Assert.AreEqual(new DateTime(2009, 10, 14, 17, 42, 00), list.Last().Date);
            Assert.AreEqual("Nothingman54", list.Last().Oembed.AuthorName);
            Assert.AreEqual("http://www.youtube.com/user/Nothingman54", list.Last().Oembed.AuthorUrl);
            Assert.AreEqual(null, list.Last().Oembed.CacheAge);
            Assert.AreEqual(344, list.Last().Oembed.Height);
            Assert.AreEqual(null, list.Last().Oembed.Html);
            Assert.AreEqual("YouTube", list.Last().Oembed.ProviderName);
            Assert.AreEqual("http://www.youtube.com/", list.Last().Oembed.ProviderUrl);
            Assert.AreEqual("twitter", list.Last().Oembed.SourceName);
            Assert.AreEqual(360, list.Last().Oembed.ThumbnailHeight);
            Assert.AreEqual("http://i2.ytimg.com/vi/AFVlJAi3Cso/hqdefault.jpg", list.Last().Oembed.ThumbnailUrl);
            Assert.AreEqual(480, list.Last().Oembed.ThumbnailWidth);
            Assert.AreEqual("Pearl Jam-Black", list.Last().Oembed.Title);
            Assert.AreEqual("video", list.Last().Oembed.Type);
            Assert.AreEqual("1.0", list.Last().Oembed.Version);
            Assert.AreEqual(459, list.Last().Oembed.Width);
        }


    }
}
