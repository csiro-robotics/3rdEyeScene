// Copyright (c) CSIRO 2016
// Commonwealth Scientific and Industrial Research Organisation (CSIRO) 
// ABN 41 687 119 230
//
// author Kazys Stepanas
//
using System;
using System.Collections.Generic;

namespace Tes.Logging
{
	/// <summary>
	/// Associated a category number with a string name for the category.
	/// </summary>
	/// <remarks>
	/// Categories are identified by number with names associated using <see cref="SetCategoryName(int, string)" />.
	/// The default category is zero and is unnamed.
	///
	/// For performance reasons, categories should only be registered with contiguous integers.
	/// </remarks>
	public static class LogCategories
  {
		/// <summary>
		/// Query the name of a category.
		/// </summary>
		/// <param name="category">The category to query.</param>
		/// <returns>The category name.</returns>
		/// <remarks>
		/// If the category is unknown then the given number is converted to a string.
		/// </remarks>
		public static string GetCategoryName(int category)
    {
      if (0 <= category && category < _categories.Count)
      {
        return _categories[category];
      }

      return (category > 0) ? category.ToString() : "";
    }

		/// <summary>
		/// Sets the name for a category number.
		/// </summary>
		/// <param name="category">The category number.</param>
		/// <param name="name">The name to set.</param>
		/// <returns>True on success, false if the category is already registered.</returns>
		/// <remarks>
		/// An existing category cannot be overwritten.
		/// </remarks>
		public static bool SetCategoryName(int category, string name)
    {
			while (_categories.Count <= category)
			{
				_categories.Add("");
			}

			if (_categories[category].Length == 0)
			{
				_categories[category] = name;
				return true;
			}

			return false;
		}

		private static List<string> _categories = new List<string>();
  }
}
