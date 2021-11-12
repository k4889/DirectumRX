using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using finex.EditableConstants.ConstantsGroup;

namespace finex.EditableConstants
{
	partial class ConstantsGroupClientHandlers
	{

		public override void Showing(Sungero.Presentation.FormShowingEventArgs e)
		{
			var isSystem = Sungero.CoreEntities.Users.Current.IsSystem.GetValueOrDefault();
      if (!isSystem)
        e.HideAction(_obj.Info.Actions.DeleteEntity);		
		}
	}
}