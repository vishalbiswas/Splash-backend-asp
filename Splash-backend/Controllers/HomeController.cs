using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Splash_backend
{
	[Route("")]
	public class HomeController {
		[HttpGet]
		public ObjectResult Get() {
			return new ObjectResult("Welcome to Splash!");
		}
	}
}