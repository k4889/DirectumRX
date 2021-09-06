using System;
using Sungero.Core;

namespace finex.UserActivityControl.Constants
{
	public static class Module
	{
		public static class RoleGuid
		{
			// GUID роли "Зарезервированная лицензия".
			[Sungero.Core.Public]
			public static readonly Guid ReserveLicenseGuid = Guid.Parse("387494a1-0b0d-44e0-8130-b728f7e03442");
		}
		
		public static class RoleNames
		{	// Наименование роли "Зарезервированная лицензия".
			public const string ReserveLicenseName = "Зарезервированная лицензия";
		}
	}
}