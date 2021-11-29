using System.Collections.Generic;

namespace Companion.Data
{
	public static class CostExtensions
	{
		public static List<Cost> Multiply(this List<Cost> costs, int number)
		{
			// always create a new list to avoid modifying references later
			List<Cost> multiplied = new List<Cost>();
			foreach (Cost cost in costs)
			{
				// clone
				Cost newCost = new Cost(cost.GetNode());
				newCost.value *= number;

				multiplied.Add(newCost);
			}

			return multiplied;
		}
	}
}
