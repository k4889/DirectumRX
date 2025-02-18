﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.EditableConstants.ConstantsGroup;

namespace finex.EditableConstants
{
  partial class ConstantsGroupServerHandlers
  {

    public override void BeforeDelete(Sungero.Domain.BeforeDeleteEventArgs e)
    {
    	var current = Sungero.CoreEntities.Users.Current;
      Logger.ErrorFormat("ВНИМАНИЕ: Зафиксировано удаление группы констант \"{0}\" пользователем {1}", _obj.Name, current.Name);
    }
  }

	partial class ConstantsGroupFilteringServerHandler<T>
	{

		public override IQueryable<T> Filtering(IQueryable<T> query, Sungero.Domain.FilteringEventArgs e)
		{
			if (finex.CollectionFunctions.PublicFunctions.Module.Remote.IsAdministrator())
				return query;
			else
				return query.Where(r => r == null);
		}
	}

}