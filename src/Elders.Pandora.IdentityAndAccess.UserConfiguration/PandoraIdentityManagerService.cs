using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security.DataProtection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.IdentityManager;
using Thinktecture.IdentityManager.AspNetIdentity;

namespace Elders.Pandora.IdentityAndAccess.UserConfiguration
{
    public class PandoraIdentityManagerService : AspNetIdentityManagerService<PandoraUser, string, PandoraRole, string>
    {
        UserManager<PandoraUser, string> manager;

        public PandoraIdentityManagerService(UserManager<PandoraUser, string> manager, IDataProtectionProvider dataProtectionProvider = null)
            : base(manager, new PandoraRoleManager())
        {
            this.manager = manager;
            this.manager.UserLockoutEnabledByDefault = false;
            this.manager.UserValidator = new UserValidator<PandoraUser>(manager) { AllowOnlyAlphanumericUserNames = false, RequireUniqueEmail = true };
            this.manager.PasswordValidator = new PasswordValidator() { RequireDigit = true, RequiredLength = 6, RequireLowercase = true, RequireNonLetterOrDigit = false, RequireUppercase = true, };

            this.manager.UserTokenProvider = new DataProtectorTokenProvider<PandoraUser, string>(new CustomTokenProvider()) { TokenLifespan = TimeSpan.FromHours(24) };
        }

        public async Task<IdentityManagerResult<CreateResult>> CreateUserAsync(string username, string password, string email, IdentityMessage WelcomeMessage)
        {
            PandoraUser user = new PandoraUser { UserName = username, Email = email, EmailConfirmed = true };
            var result = await this.manager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return new IdentityManagerResult<CreateResult>(result.Errors.ToArray());
            }
            if (WelcomeMessage != null)
            {
                using (var client = new SmtpClient())
                {
                    var msg = new MailMessage("web@Pandora.org", user.Email);
                    msg.IsBodyHtml = true;
                    msg.Body = WelcomeMessage.Body.Replace("usernameplaceholder", user.UserName);
                    msg.Subject = WelcomeMessage.Subject;

                    await client.SendMailAsync(msg);
                }
            }
            var updateProfile = await UpdateProfile(user.Id.ToString(), new List<System.Security.Claims.Claim>()
                {
                    new System.Security.Claims.Claim("name", user.UserName)
                });
            return new IdentityManagerResult<CreateResult>(new CreateResult { Subject = user.Id.ToString() });
        }

        public async Task<IdentityManagerResult> RequestResetPasswordAsync(string username, string confirmUrl, IdentityMessage ResetPasswordMessage)
        {
            var user = await this.manager.FindByNameAsync(username);
            if (user != null)
            {
                var token = await this.manager.GeneratePasswordResetTokenAsync(user.Id);

                var link = confirmUrl.TrimEnd('/', '\\') + "?id=" + System.Web.HttpUtility.UrlEncode(token) + "&message=" + System.Web.HttpUtility.UrlEncode(System.Convert.ToBase64String(Encoding.UTF8.GetBytes(username)));
                using (var client = new SmtpClient())
                {
                    var msg = new MailMessage("web@Pandora.org", user.Email);
                    msg.IsBodyHtml = true;
                    msg.Body = ResetPasswordMessage.Body.Replace("resetpasswordlinkplaceholder", link);
                    msg.Subject = ResetPasswordMessage.Subject;

                    await client.SendMailAsync(msg);
                }
            }
            return new IdentityManagerResult();
        }

        public async Task<IdentityManagerResult> ConfirmResetPasswordAsync(string username, string newPassword, string token)
        {
            var user = await this.manager.FindByNameAsync(username);
            if (user != null)
            {
                var result = await this.manager.ResetPasswordAsync(user.Id, token, newPassword);

                if (!result.Succeeded)
                {
                    return new IdentityManagerResult(result.Errors.ToArray());
                }
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> SetEmailAsync(string subject, string email)
        {
            var result = await this.manager.SetEmailAsync(subject, email);
            if (!result.Succeeded)
            {
                return new IdentityManagerResult(result.Errors.ToArray());
            }

            var token = await this.userManager.GenerateEmailConfirmationTokenAsync(subject);
            result = this.userManager.ConfirmEmail(subject, token);
            if (!result.Succeeded)
            {
                return new IdentityManagerResult(result.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> SetPasswordAsync(string subject, string currentPassword, string password)
        {
            var user = this.manager.FindById(subject);

            var hasValidOldPassword = this.manager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, currentPassword);
            if (hasValidOldPassword != PasswordVerificationResult.Success)
                return new IdentityManagerResult<CreateResult>("Invalid old password");

            var token = await this.manager.GeneratePasswordResetTokenAsync(subject);
            var result = await this.manager.ResetPasswordAsync(subject, token, password);
            if (!result.Succeeded)
            {
                return new IdentityManagerResult<CreateResult>(result.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> SetPasswordAsync(string subject, string password)
        {
            var user = this.manager.FindById(subject);

            var token = await this.manager.GeneratePasswordResetTokenAsync(subject);
            var result = await this.manager.ResetPasswordAsync(subject, token, password);
            if (!result.Succeeded)
            {
                return new IdentityManagerResult<CreateResult>(result.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> SetSecurityAnswer(string subject, string question, string answer)
        {
            List<string> errors = new List<string>();
            if (String.IsNullOrWhiteSpace(subject)) errors.Add("Invalid argument subject");
            if (String.IsNullOrWhiteSpace(question)) errors.Add("Invalid argument question");
            if (String.IsNullOrWhiteSpace(answer)) errors.Add("Invalid argument answer");

            var user = await userManager.FindByIdAsync(subject);
            if (user == null)
                errors.Add(String.Format("User not found. subject='{0}'", subject));

            user.SecurityQuestion = question;
            user.SecurityAnswer = answer;
            var result = await userManager.UpdateAsync(user);
            if (errors.Count > 0 || !result.Succeeded)
            {
                if (!result.Succeeded)
                    errors.AddRange(result.Errors);
                return new IdentityManagerResult(errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> SetUsernameAsync(string subject, string username)
        {
            var user = this.manager.FindById(subject);
            user.UserName = username;
            var result = await this.manager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return new IdentityManagerResult(result.Errors.ToArray());
            }
            var updateProfile = await UpdateProfile(user.Id.ToString(), new List<System.Security.Claims.Claim>()
                {
                    new System.Security.Claims.Claim("name", user.UserName)
                });

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> UpdateAddress(string subject, List<System.Security.Claims.Claim> addressClaims)
        {
            var existingClaims = await this.manager.GetClaimsAsync(subject);
            foreach (var profileClaim in addressClaims)
            {
                if (String.IsNullOrWhiteSpace(profileClaim.Value))
                    return new IdentityManagerResult("Invalid claim value " + profileClaim.Type + " Type:" + profileClaim.Type);

                var claimToUpdate = existingClaims.FirstOrDefault(x => x.Type == profileClaim.Type);
                if (!ReferenceEquals(null, claimToUpdate))
                {
                    if (profileClaim.Value == claimToUpdate.Value)
                        continue;

                    var delClaimResult = await this.manager.RemoveClaimAsync(subject, claimToUpdate);
                    if (!delClaimResult.Succeeded)
                        return new IdentityManagerResult(delClaimResult.Errors.ToArray());
                }
                var result = await this.manager.AddClaimAsync(subject, profileClaim);
                if (!result.Succeeded)
                    return new IdentityManagerResult(result.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> UpdateProfile(string subject, List<System.Security.Claims.Claim> profileClaims)
        {
            var existingClaims = await this.manager.GetClaimsAsync(subject);
            foreach (var profileClaim in profileClaims)
            {
                var claimToUpdate = existingClaims.FirstOrDefault(x => x.Type == profileClaim.Type);
                if (!ReferenceEquals(null, claimToUpdate))
                {
                    if (profileClaim.Value == claimToUpdate.Value)
                        continue;

                    var delClaimResult = await this.manager.RemoveClaimAsync(subject, claimToUpdate);
                    if (!delClaimResult.Succeeded)
                        return new IdentityManagerResult(delClaimResult.Errors.ToArray());
                }
                if (!String.IsNullOrWhiteSpace(profileClaim.Value))
                {
                    var result = await this.manager.AddClaimAsync(subject, profileClaim);
                    if (!result.Succeeded)
                        return new IdentityManagerResult(result.Errors.ToArray());
                }
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> UpdateRole(string subject, string value)
        {
            var existingClaims = await this.manager.GetClaimsAsync(subject);

            var claimToUpdate = existingClaims.FirstOrDefault(x => x.Type == System.Security.Claims.ClaimTypes.Role);
            if (!ReferenceEquals(null, claimToUpdate))
            {
                if (value == claimToUpdate.Value)
                    return IdentityManagerResult.Success;

                var delClaimResult = await this.manager.RemoveClaimAsync(subject, claimToUpdate);
                if (!delClaimResult.Succeeded)
                    return new IdentityManagerResult(delClaimResult.Errors.ToArray());
            }
            var result = await this.manager.AddClaimAsync(subject, new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, value));
            if (!result.Succeeded)
                return new IdentityManagerResult(result.Errors.ToArray());

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> ChangeEmailAsync(string subject, string oldPasswird, string email)
        {
            var user = this.manager.FindById(subject);

            var hasValidOldPassword = this.manager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, oldPasswird);
            if (hasValidOldPassword != PasswordVerificationResult.Success)
                return new IdentityManagerResult<CreateResult>("Invalid old password");

            var result = await SetEmailAsync(subject, email);
            if (!result.IsSuccess)
            {
                return new IdentityManagerResult<CreateResult>(result.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> ChangeUsernameAsync(string subject, string oldPasswird, string username)
        {
            var user = this.manager.FindById(subject);

            var hasValidOldPassword = this.manager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, oldPasswird);
            if (hasValidOldPassword != PasswordVerificationResult.Success)
                return new IdentityManagerResult<CreateResult>("Invalid old password");

            var result = await SetUsernameAsync(subject, username);
            if (!result.IsSuccess)
            {
                return new IdentityManagerResult<CreateResult>(result.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }

        public async Task<IdentityManagerResult> ChangeQuestionAndAnswer(string subject, string oldPasswird, string question, string answer)
        {
            var user = this.manager.FindById(subject);

            var hasValidOldPassword = this.manager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, oldPasswird);
            if (hasValidOldPassword != PasswordVerificationResult.Success)
                return new IdentityManagerResult<CreateResult>("Invalid old password");

            var result = await SetSecurityAnswer(subject, question, answer);
            if (!result.IsSuccess)
            {
                return new IdentityManagerResult<CreateResult>(result.Errors.ToArray());
            }

            return IdentityManagerResult.Success;
        }
    }
}