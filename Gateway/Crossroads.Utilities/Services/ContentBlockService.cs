using System;
using System.Collections.Generic;
using Crossroads.Utilities.Interfaces;
using Crossroads.Utilities.Models;
using log4net;
using RestSharp;
using Newtonsoft.Json;

namespace Crossroads.Utilities.Services
{
    public class ContentBlockService : Dictionary<string, ContentBlock>, IContentBlockService
    {
        private readonly ILog _logger = LogManager.GetLogger(typeof(ContentBlockService));
        public ContentBlockService(IRestClient cmsRestClient)
        {
            try
            {
                // NOTE: In order to connect to the CMS server over SSL, you must enable
                // SecurityProtocolType.Tls12.  This is handled already in TlsHelper.cs
                // called from Application_Start(), but if this code is reused elsewhere
                // beware of this dependency.
                RestRequest request = new RestRequest("/api/contentblock", Method.GET);
                IRestResponse response = cmsRestClient.Execute(request);

                ContentBlocks blocks = JsonConvert.DeserializeObject<ContentBlocks>(response.Content);
                foreach (ContentBlock b in blocks.contentBlocks)
                {
                    Add(b.Title, b);
                }
            }
            catch (Exception e)
            {
                _logger.Error("ContentBlockService: Unable to get the content blocks from the CMS!", e);
            }
        }

        public string GetContent(string title)
        {
            string content = "";

            ContentBlock contentBlock;
            if (this.TryGetValue(title, out contentBlock))
            {
                content = contentBlock.Content;
            }
            else
            {
                _logger.Error($"ContentBlockService: Missing CMS content for '{title}'");
            }

            return content;
        }
    }
}