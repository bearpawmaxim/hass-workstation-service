﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ReactiveUI;

namespace UserInterface.ViewModels
{
	public class ViewModelBase : ReactiveObject
	{
		public bool IsValid<T>(T obj, out ICollection<ValidationResult> results) {
			results = new List<ValidationResult>();

			return Validator.TryValidateObject(obj, new ValidationContext(obj), results, true);
		}
	}
}