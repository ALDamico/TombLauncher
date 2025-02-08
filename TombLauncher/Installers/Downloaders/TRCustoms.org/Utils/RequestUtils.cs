using System.Collections.Generic;
using TombLauncher.Core.Utils;
using TombLauncher.Installers.Downloaders.TRCustoms.org.Requests;

namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Utils;

internal static class RequestUtils
{
    internal static IEnumerable<KeyValuePair<string, string>> DictifyRequest(SearchRequest request)
    {
        var tempRequest = (Dictionary<string, string>)DictifyRequest((TrCustomsBaseRequest)request);

        if (request != null)
        {
            if (request.Sort != null)
            {
                tempRequest["sort"] = request.Sort;
            }

            if (request.Tags.IsNotNullOrEmpty())
            {
                tempRequest["tags"] = string.Join(",", request.Tags);
            }

            if (request.Genres.IsNotNullOrEmpty())
            {
                tempRequest["genres"] = string.Join(",", request.Genres);
            }

            if (request.Engines.IsNotNullOrEmpty())
            {
                tempRequest["engines"] = string.Join(",", request.Engines);
            }

            if (request.Difficulties.IsNotNullOrEmpty())
            {
                tempRequest["difficulties"] = string.Join(",", request.Difficulties);
            }

            if (request.Durations.IsNotNullOrEmpty())
            {
                tempRequest["durations"] = string.Join(",", request.Difficulties);
            }

            if (request.Ratings.IsNotNullOrEmpty())
            {
                tempRequest["ratings"] = string.Join(",", request.Ratings);
            }
        }

        return tempRequest;
    }
    internal static IEnumerable<KeyValuePair<string, string>> DictifyRequest(TrCustomsBaseRequest baseRequest)
    {
        var dictionary = new Dictionary<string, string>();
        dictionary["format"] = "json";
        if (baseRequest != null)
        {
            if (baseRequest.Page.HasValue)
            {
                dictionary["page"] = baseRequest.Page.ToString();
            }

            if (baseRequest.Search != null)
            {
                dictionary["search"] = baseRequest.Search;
            }

            if (baseRequest.Sort != null)
            {
                dictionary["sort"] = baseRequest.Sort;
            }

            if (baseRequest.PageSize != null)
            {
                dictionary["page_size"] = baseRequest.PageSize.ToString();
            }
        }

        return dictionary;
    }
}