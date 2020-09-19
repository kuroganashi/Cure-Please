using System;

namespace CurePlease
{
  public static class Helpers
	{
		public static T[] CreateAndFill<T>(int length, Func<T> valueGenerator)
		{
			T[] items = new T[length];
			for (var i = 0; i < length; i++)
			{
				items[i] = valueGenerator();
			}

			return items;
		}
	}
}
