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
        public Singly.SinglyContext Context { get; set; }

        [TestInitialize]
        public void SetUp()
        {
            Context = new Singly.SinglyContext(new Uri("http://localhost:8778/types"));
            Context.UseJsonFormatWithDefaultServiceModel();
            Context.IgnoreMissingProperties = true;
            Context.SendingRequest += new EventHandler<System.Data.Services.Client.SendingRequestEventArgs>(Context_SendingRequest);
        }

        void Context_SendingRequest(object sender, System.Data.Services.Client.SendingRequestEventArgs e)
        {
            e.RequestHeaders.Add("Authorization", "Bearer UOmcFTvBlvFhiFVWFTFqF1tcwSA=U0N7obhYa15bf15888f5ed4a883b10b7932989bd8e1df691c5c64df24f7740b145e95e6006aca820e766c3058005bf24948be04898463f0a956099c7874f75ca9932d53bee07d73baab71c50e7d57226b8fa83dfb996d25d84c1a33e5e23b21355a4a361");
        }

        [TestMethod]
        public void Should_Query_Statuses()
        {
            var list = Context.Statuses.ToArray();

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Length > 0);
            Assert.IsTrue(list[0].Oembed.Text.Length > 0);
        }

        [TestMethod]
        public void Should_Query_With_Top()
        {
            var list = Context.Statuses.Take(2).ToArray();

            Assert.IsNotNull(list);
            Assert.AreEqual(2, list.Length);
        }

        [TestMethod]
        public void Should_Query_With_Date()
        {
            var full = Context.Statuses.Take(3).ToArray();
            var second = full[0].Date;

            var list = Context
                        .Statuses
                        .Where(s => s.Date > second)
                        .ToArray();

            Assert.AreEqual(1, list.Length);
        }

        [TestMethod]
        public void Should_Query_With_LessThan()
        {
            var list = Context
                .Statuses
                .Where(s => s.Date < DateTime.Now.AddDays(10))
                .ToArray();

            Assert.IsNotNull(list);
            Assert.IsTrue(list.Length > 0);
        }
 
        [TestMethod]
        [ExpectedException(typeof(DataServiceQueryException))]
        public void Should_Not_Get_By_Id()
        {
            try
            {
                Context.Statuses.Where(s => s.Id == "foo").ToArray();
            }
            catch (System.Data.Services.Client.DataServiceClientException e)
            {
                /* TODO: The OData client is not deserializing the error as per the OData JSON
                 * light specification: http://www.odata.org/media/30002/OData%20JSON%20Verbose%20Format.html
                 *
                 * Assert.AreEqual("resource path is not valid. get by id is not supported yet.", e.Message);
                 */
                return;
            }

            Assert.Fail();
        }

        [TestMethod]
        [ExpectedException(typeof(DataServiceQueryException))]
        public void Should_Receive_MeaningfulError_When_Unsupported_Operator()
        {
            try
            {
                Context.Statuses.Where(s => s.Data == "foo").ToArray();
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
            var list = Context.Checkins.ToArray();

            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void Should_Query_Contacts()
        {
            var list = Context.Contacts.ToArray();

            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void Should_Query_News()
        {
            var list = Context.News.ToArray();

            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void Should_Query_Photos()
        {
            var list = Context.Photos.ToArray();

            Assert.IsNotNull(list);
        }

        [TestMethod]
        public void Should_Query_Videos()
        {
            var list = Context.Videos.ToArray();

            Assert.IsNotNull(list);
        }


    }
}
