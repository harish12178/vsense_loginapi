using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSign.Models;

namespace VSign.Repositories
{
    public interface IMasterRepository
    {

        #region Authentication
        Client FindClient(string clientId);
        AuthenticationResult AuthenticateUser(string UserName, string Password);

        #endregion

        #region User

        List<UserWithRole> GetAllUsers();
        Task<UserWithRole> CreateUser(UserWithRole userWithRole);
        Task<UserWithRole> UpdateUser(UserWithRole userWithRole);
        Task<UserWithRole> DeleteUser(UserWithRole userWithRole);
        #endregion

        #region Role

        List<RoleWithApp> GetAllRoles();
        Task<RoleWithApp> CreateRole(RoleWithApp roleWithApp);
        Task<RoleWithApp> UpdateRole(RoleWithApp roleWithApp);
        Task<RoleWithApp> DeleteRole(RoleWithApp roleWithApp);

        #endregion

        

        

        

        #region App

        List<App> GetAllApps();
        Task<App> CreateApp(App app);
        Task<App> UpdateApp(App app);
        Task<App> DeleteApp(App app);

        #endregion

        #region AppUsage

        List<AppUsageView> GetAllAppUsages();
        List<AppUsageView> GetAppUsagesByUser(Guid UserID);
        Task<AppUsage> CreateAppUsage(AppUsage AppUsage);
        //Task<AppUsage> UpdateAppUsage(AppUsage AppUsage);
        //Task<AppUsage> DeleteAppUsage(AppUsage AppUsage);

        #endregion

        #region SessionMaster

        SessionMaster GetSessionMasterByProject(string ProjectName);
        List<SessionMaster> GetAllSessionMasters();
        List<SessionMaster> GetAllSessionMastersByProject(string ProjectName);
        Task<SessionMaster> CreateSessionMaster(SessionMaster SessionMaster);
        Task<SessionMaster> UpdateSessionMaster(SessionMaster SessionMaster);
        Task<SessionMaster> DeleteSessionMaster(SessionMaster SessionMaster);

        #endregion

        #region LogInAndChangePassword

        Task<UserLoginHistory> LoginHistory(Guid UserID, string Username);
        List<UserLoginHistory> GetAllUsersLoginHistory();
        List<UserLoginHistory> GetCurrentUserLoginHistory(Guid UserID);
        Task<UserLoginHistory> SignOut(Guid UserID);
        List<UserLoginHistory> FilterLoginHistory(string UserName = null, DateTime? FromDate = null, DateTime? ToDate = null);
        #endregion

        #region ChangePassword

        Task<User> ChangePassword(ChangePassword changePassword);
        Task<TokenHistory> SendResetLinkToMail(EmailModel emailModel);
        Task<TokenHistory> ForgotPassword(ForgotPassword forgotPassword);


        #endregion

        #region sendMail

        Task<bool> SendMail(string code, string UserName, string toEmail, string password, string type, string userID, string siteURL);
        Task<bool> SendMailToVendor(string toEmail, string password, string siteURL);

        #endregion

        #region EncryptAndDecrypt
        string Decrypt(string Password, bool UseHashing);
        string Encrypt(string Password, bool useHashing);

        #endregion

        #region UserPreferences

        UserPreference GetUserPrefercences(Guid userID);
        Task<UserPreference> SetUserPreference(UserPreference UserPreference);
        #endregion

        List<UserWithRole> GetSupportDeskUsersByRoleName(string RoleName);


        
    }
}
