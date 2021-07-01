using System.Text.RegularExpressions;

namespace Application.Utilities
{
    public static class Standardizers
    {
        public static string EmailNormalize(this string email)
        {
            if (email == null)
                return null;

            if (!new Regex(@"@.+\..+$", RegexOptions.IgnoreCase).IsMatch(email))
                return email;

            var emailFirstPart = email.Split("@")[0].Replace(".", "");
            var emailSecondPart = email.Split("@")[1];
            return emailFirstPart + "@" + emailSecondPart;
        }

        public static string PhoneNumberNormalize(this string phoneNumber)
        {
            if (phoneNumber == null)
                return null;

            phoneNumber = phoneNumber.Trim().Replace(" ", "").TrimStart("0".ToCharArray());

            phoneNumber = phoneNumber.Replace("+98", "");

            phoneNumber = "0" + phoneNumber;

            return phoneNumber;
        }
    }
}