using System;
using Android.Util;

namespace MonoDroid.TimesSquare
{
	public class Logr
	{
		public static void D(string message)
		{
#if DEBUG
			Log.Debug("MonoDroid.TimesSquare", message);
#endif
		}

		public static void D(string message, params object[] args)
		{
#if DEBUG
			D(string.Format(message, args));
#endif
		}
	}
}