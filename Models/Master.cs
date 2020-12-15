using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VSign.Models
{
    public class LdapUser
    {
        public Guid UserID { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Path { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }

    public class User
    {
        [Key]
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ContactNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string DisplayName { get; set; }
      //  public bool TourStatus { get; set; }
      //  public string Pass1 { get; set; }
      //  public string Pass2 { get; set; }
      //  public string Pass3 { get; set; }
       // public DateTime? ExpiringOn { get; set; }
    }

    public class Role
    {
        [Key]
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class UserRoleMap
    {
        [Column(Order = 0), Key, ForeignKey("User")]
        public Guid UserID { get; set; }
        [Column(Order = 1), Key, ForeignKey("Role")]
        public Guid RoleID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class App
    {
        [Key]
        public int AppID { get; set; }
        public string AppName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class RoleAppMap
    {

        [Key]
        [Column(Order = 1)]
        //[Column(Order = 0), Key, ForeignKey("Role")]
        public Guid RoleID { get; set; }
        //[Column(Order = 1), Key, ForeignKey("App")]
        [Key]
        [Column(Order = 2)]
        public int AppID { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class UserWithRole
    {
        public Guid UserID { get; set; }
        public Guid RoleID { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ContactNumber { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string DisplayName { get; set; }

    }

    public class UserView
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
    }

    public class RoleWithApp
    {
        public Guid RoleID { get; set; }
        public string RoleName { get; set; }
        public int[] AppIDList { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }

    public class UserLoginHistory
    {
        [Key]
        public int ID { get; set; }
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        //public string IP { get; set; }
    }

    public class ChangePassword
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class EmailModel
    {
        public string EmailAddress { get; set; }
        public string siteURL { get; set; }
    }

    public class ForgotPassword
    {
        public Guid UserID { get; set; }
        public string EmailAddress { get; set; }
        public string NewPassword { get; set; }
        public string Token { get; set; }
    }

    public class TokenHistory
    {
        [Key]
        public int TokenHistoryID { get; set; }
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string Token { get; set; }
        public string EmailAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ExpireOn { get; set; }
        public DateTime? UsedOn { get; set; }
        public bool IsUsed { get; set; }
        public string Comment { get; set; }
    }
    public class SessionMaster
    {
        [Key]
        public int ID { get; set; }
        public string ProjectName { get; set; }
        public int SessionTimeOut { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class AppUsage
    {
        [Key]
        public int ID { get; set; }
        public Guid UserID { get; set; }
        //public int AppID { get; set; }
        public string AppName { get; set; }
        public int UsageCount { get; set; }
        public DateTime LastUsedOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class AppUsageView
    {
        [Key]
        public int ID { get; set; }
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string UserRole { get; set; }
        public string AppName { get; set; }
        public int UsageCount { get; set; }
        public DateTime LastUsedOn { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
    public class LoginModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string clientId { get; set; }
    }
    public class AuthenticationResult
    {
        public Guid UserID { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string UserRole { get; set; }
        public string Token { get; set; }
        public string MenuItemNames { get; set; }
        public string IsChangePasswordRequired { get; set; }
        public string Profile { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public bool TourStatus { get; set; }
    }

 

    public class STMPDetails
    {
        public string Host { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
    }
}
