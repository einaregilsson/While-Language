/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
This code is licensed under the Visual Studio SDK license terms.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Package;

namespace Babel
{
	public abstract class BabelLanguageService : Microsoft.VisualStudio.Package.LanguageService
	{
		#region Custom Colors
		public override int GetColorableItem(int index, out IVsColorableItem item)
		{
			if (index <= Configuration.ColorableItems.Count)
			{
				item = Configuration.ColorableItems[index - 1];
				return Microsoft.VisualStudio.VSConstants.S_OK;
			}
			else
			{
				throw new ArgumentNullException("index");
			}
		}

		public override int GetItemCount(out int count)
		{
			count = Configuration.ColorableItems.Count;
			return Microsoft.VisualStudio.VSConstants.S_OK;
		}
		#endregion

		#region MPF Accessor and Factory specialisation
		private LanguagePreferences preferences;
		public override LanguagePreferences GetLanguagePreferences()
		{
			if (this.preferences == null)
			{
				this.preferences = new LanguagePreferences(this.Site,
														typeof(CCS.LanguageService.WhileLanguageService).GUID,
														this.Name);
				this.preferences.Init();
			}

			return this.preferences;
		}

		public override Microsoft.VisualStudio.Package.Source CreateSource(IVsTextLines buffer)
		{
			return new Source(this, buffer, this.GetColorizer(buffer));
		}

		private IScanner scanner;
		public override IScanner GetScanner(IVsTextLines buffer)
		{
			if (scanner == null)
				this.scanner = new LineScanner();

			return this.scanner;
		}
		#endregion

		public override void OnIdle(bool periodic)
		{
			// from IronPythonLanguage sample
			// this appears to be necessary to get a parse request with ParseReason = Check?
			Source src = (Source) GetSource(this.LastActiveTextView);
			if (src != null && src.LastParseTime >= Int32.MaxValue >> 12)
			{
				src.LastParseTime = 0;
			}
			base.OnIdle(periodic);
		}

		public override string Name
		{
			get { return Configuration.Name; }
		}
	}
}