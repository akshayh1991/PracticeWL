using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SecMan.Model
{
    public class ValidPasswordAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var password = value as string;

            if (string.IsNullOrWhiteSpace(password))
            {
                return new ValidationResult("Password cannot be empty.");
            }

            if (password.Length < 6 || password.Length > 15)
            {
                return new ValidationResult("Password must be between 6 and 15 characters.");
            }

            if (!Regex.IsMatch(password, @"[A-Z]")) // At least 1 uppercase character
            {
                return new ValidationResult("Password must contain at least 1 uppercase character.");
            }

            if (!Regex.IsMatch(password, @"[a-z]")) // At least 1 lowercase character
            {
                return new ValidationResult("Password must contain at least 1 lowercase character.");
            }

            if (!Regex.IsMatch(password, @"[0-9]")) // At least 1 numeric character
            {
                return new ValidationResult("Password must contain at least 1 numeric character.");
            }

            if (!Regex.IsMatch(password, @"[^\w]")) // At least 1 non-alphanumeric character
            {
                return new ValidationResult("Password must contain at least 1 non-alphanumeric character.");
            }

            return ValidationResult.Success;
        }
    }
}
