using System;
using System.Globalization;

namespace Ruzzie.Common.Validation
{
    /// <summary>
    /// Extension and helper methods for Email Address validation
    /// </summary>
    public static class EmailValidation
    {
        private static readonly IdnMapping IdnMapping = new IdnMapping();

        /// <summary>
        /// Validates if a given string is an email address.
        /// </summary>
        /// <param name="email">the email to check</param>
        /// <returns>true when valid false otherwise.</returns>
        public static bool IsValidEmailAddress(this string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            try
            {
                var validEmail = new System.Net.Mail.MailAddress(email);

                try
                {
                    var _ = IdnMapping.GetAscii(validEmail.Host);
                }
                catch (ArgumentException)
                {
                    return false;
                }
                finally
                {
                    // ReSharper disable once RedundantAssignment
                    validEmail = null;
                }

                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}