using System;
using System.Collections.Generic;

namespace YCIOT.ModbusPoll.RtuOverTcp.Utils
{
    public static class ConvertHelper
    {
    }

	public static class DisplacementUtils
	{
		public static List<double> FitDisplacement(ushort count, double maxDisplacement)
		{
			var output = new List<double>();

			for (var i = 0; i < count; i++)
			{
				var item = maxDisplacement * 0.5 * (Math.Sin(i * 3.1415 * 2 / count + 3.14 * 1.5) + 1);
				output.Add(Math.Round(item, 2));
			}

			return output;
		}
	}

}
