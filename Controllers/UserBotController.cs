using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using TL;

namespace WTelegramClientTestASP.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UserBotController : ControllerBase
	{
		private readonly WTelegramService WT;
		public UserBotController(WTelegramService wt) => WT = wt;

		[HttpGet("status")]
		public ContentResult Status()
		{
			switch (WT.ConfigNeeded)
			{
				case "connecting": return Content("<meta http-equiv=\"refresh\" content=\"1\">WTelegram is connecting...", "text/html");
                case null: return Content($@"Connected as {WT.User}<br/><a href=""Filters"">Get filters</a>", "text/html");
				default: return Content($@"Enter {WT.ConfigNeeded}: <form action=""config""><input name=""value"" autofocus/></form>", "text/html");
			}
		}

		[HttpGet("config")]
		public async Task<ActionResult> Config(string value)
		{
			await WT.DoLogin(value);
			return Redirect("status");
		}

		[HttpGet("Filters")]
		public async Task<object> Filters()
		{
			if (WT.User == null) throw new Exception("Complete the login first");

			DialogFilterBase[] baseFilters = await WT.Client.Messages_GetDialogFilters();


			List<DialogFilter> filters = new();

			foreach (DialogFilterBase basefilter in baseFilters)
			{
                if (basefilter.GetType().Name == "DialogFilter")
				{
                    filters.Add((DialogFilter)basefilter);
				}
			}

			var html = $@"<h1>Prueba Filtros</h1>
					<form action=""Message"">
						<select name=""id"">";



            foreach (DialogFilter filter in filters)
            {
                html += $@"<option value=""{filter.id}"">{filter.title}</option>";
            }


            html += $@"</select>
						<input name=""msg"" autofocus/>
					</form>";




            return Content(html, "text/html");
		}

		[HttpGet("Message")]
		public async Task<object> Message(int id, string msg)
		{
            if (WT.User == null) throw new Exception("Complete the login first");
            DialogFilterBase[] baseFilters = await WT.Client.Messages_GetDialogFilters();

            List<DialogFilter> filters = new();

            foreach (DialogFilterBase basefilter in baseFilters)
            {
                if (basefilter.GetType().Name == "DialogFilter")
                {
                    filters.Add((DialogFilter)basefilter);
                }
            }

			var filter = filters.Find(e => e.id == id);

			foreach (var Peer in filter.include_peers)
			{
				await WT.Client.SendMessageAsync(Peer, msg);
			}

			return "Mensaje Enviado";
		}

    }
}
