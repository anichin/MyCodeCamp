using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCodeCamp.Controllers;
using MyCodeCamp.Data.Entities;

namespace MyCodeCamp.Models
{
    public class SpeakerUrlResolver : IValueResolver<Speaker, SpeakerModel, string>
    {
        private IHttpContextAccessor _httpContextAccessor;

        public SpeakerUrlResolver(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Resolve(Speaker source, SpeakerModel destination, string destMember, ResolutionContext context)
        {
            var urlHelper = (IUrlHelper)_httpContextAccessor.HttpContext.Items[BaseController.URLHELPER];
            var url = urlHelper.Link("SpeakerGet", new { moniker = source.Camp.Moniker, id = source.Id });

            return url;
        }
    }
}
