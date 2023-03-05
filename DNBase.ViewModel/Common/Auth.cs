using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DNBase.ViewModel
{
    public class AuthConfigModel
    {
        public String LdapAddress1 { get; set; }
        public String LdapAddress2 { get; set; }
        public String LdapAddress3 { get; set; }
        public String LdapAddress4 { get; set; }
        public AuthSettings authSettings { get; set; }
    }
    public class AuthSettings
    {
        public string Secret { get; set; }
    }
    public class AuthServerSettings
    {
        public string Url { get; set; }
        public string Domain { get; set; }
    }
    public class AuthRequestModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        //public object ExtraProps { get; set; }
        //public string RemoteIpAddress { get; set; }
    }
    public class AuthResponseModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        //public object ExtraProps { get; set; }
    }
    public class RefreshTokenModel
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string RemoteIpAdderss { get; set; }
    }
    public class ValidateTokenModel
    {
        [Required]
        public string Token { get; set; }
    }
    public class ChangePassReuqestModel
    {
        public string UserName { get; set; }
        public string PasswordOld { get; set; }
        public string Password { get; set; }
        public string RepeatPassword { get; set; }
    }
    public class ForgotPasswordRequestModel
    {
        [EmailAddress]
        public string Email { get; set; }
    }
    public class ResetPasswordRequestModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}