using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VSign.Models;
using VSign.Repositories;
using VSignLogin.Models;

namespace VSign.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly IMasterRepository _masterRepository;
        public MasterController(IMasterRepository masterRepository)
        {
            _masterRepository = masterRepository;
        }

        #region Authentication

        [HttpGet]
        public Client FindClient(string clientId)
        {
            try
            {
                var client = _masterRepository.FindClient(clientId);
                return client;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/FindClient : - ", ex);
                return null;
            }
        }
        [HttpGet]
        public AuthenticationResult AuthenticateUser(string UserName, string Password)
        {
            try
            {
                var result = _masterRepository.AuthenticateUser(UserName, Password);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/FindClient : - ", ex);
                return null;
            }
        }

        #endregion

        #region App

        [HttpGet]
        public List<App> GetAllApps()
        {
            try
            {
                var result = _masterRepository.GetAllApps();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllApps", ex);
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateApp(App app)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.CreateApp(app);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/CreateApp", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateApp(App app)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.UpdateApp(app);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/UpdateApp", ex);
                return BadRequest(ex.Message);
            }
            //return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteApp(App app)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = await _masterRepository.DeleteApp(app);
                return new OkResult();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/DeleteApp", ex);
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region User
       
        [HttpGet]
        public List<UserWithRole> GetAllUsers()
        {
            try
            {
                var userWithRole = _masterRepository.GetAllUsers();
                return userWithRole;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllUsers", ex);
                return null;
            }
        }

        [HttpGet]
        public List<RoleWithUsername> GetAllUsersWithRolenames()
        {
            try
            {
                List<RoleWithUsername> temp = new List<RoleWithUsername>();
                var allusers = _masterRepository.GetAllUsers();
                var allRolenames = _masterRepository.GetAllRoles();
                allusers.ForEach(x =>
                {
                    RoleWithUsername u = new RoleWithUsername();
                  var rl=  allRolenames.FirstOrDefault(y=> x.RoleID==y.RoleID);
                    if (rl != null)
                    {
                        u.userName = x.UserName;
                        u.userRole = rl.RoleName;
                        u.userId = x.UserID.ToString();
                        u.roleId = x.RoleID.ToString();
                        temp.Add(u);
                    }

                });

                return (temp);
            }
            catch(Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllUsers", ex);
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserWithRole userWithRole)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.CreateUser(userWithRole);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/CreateUser", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(UserWithRole userWithRole)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.UpdateUser(userWithRole);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/UpdateUser", ex);
                return BadRequest(ex.Message);
            }
            //return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(UserWithRole userWithRole)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = await _masterRepository.DeleteUser(userWithRole);
                return new OkResult();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/DeleteUser", ex);
                return BadRequest(ex.Message);
            }
        }
        #endregion

        #region Role

        [HttpGet]
        public List<RoleWithApp> GetAllRoles()
        {
            try
            {
                var result = _masterRepository.GetAllRoles();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllRoles", ex);
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleWithApp roleWithApp)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.CreateRole(roleWithApp);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/CreateRole", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(RoleWithApp roleWithApp)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.UpdateRole(roleWithApp);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/UpdateRole", ex);
                return BadRequest(ex.Message);
            }
            //return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(RoleWithApp roleWithApp)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = await _masterRepository.DeleteRole(roleWithApp);
                return new OkResult();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/DeleteRole", ex);
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region AppUsage

        [HttpGet]
        public List<AppUsageView> GetAllAppUsages()
        {
            try
            {
                var result = _masterRepository.GetAllAppUsages();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllAppUsages", ex);
                return null;
            }
        }

        [HttpGet]
        public List<AppUsageView> GetAppUsagesByUser(Guid UserID)
        {
            try
            {
                var result = _masterRepository.GetAppUsagesByUser(UserID);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAppUsagesByUser", ex);
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppUsage(AppUsage AppUsage)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.CreateAppUsage(AppUsage);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/CreateAppUsage", ex);
                return BadRequest(ex.Message);
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> UpdateAppUsage(AppUsage AppUsage)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }
        //        else
        //        {
        //            var result = await _masterRepository.UpdateAppUsage(AppUsage);
        //            return Ok(result);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        ErrorLog.WriteToFile("Master/UpdateAppUsage", ex);
        //        return BadRequest(ex.Message);
        //    }
        //    //return Ok();
        //}

        //[HttpPost]
        //public async Task<IActionResult> DeleteAppUsage(AppUsage AppUsage)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return BadRequest(ModelState);
        //        }
        //        var result = await _masterRepository.DeleteAppUsage(AppUsage);
        //        return new OkResult();
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.WriteToFile("Master/DeleteAppUsage", ex);
        //        return BadRequest(ex.Message);
        //    }
        //}

        #endregion

        #region SessionMaster

        [HttpGet]
        public SessionMaster GetSessionMasterByProject(string ProjectName)
        {
            try
            {
                var result = _masterRepository.GetSessionMasterByProject(ProjectName);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetSessionMasterByProject", ex);
                return null;
            }
        }

        [HttpGet]
        public List<SessionMaster> GetAllSessionMasters()
        {
            try
            {
                var result = _masterRepository.GetAllSessionMasters();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllSessionMasters", ex);
                return null;
            }
        }

        [HttpGet]
        public List<SessionMaster> GetAllSessionMastersByProject(string ProjectName)
        {
            try
            {
                var result = _masterRepository.GetAllSessionMastersByProject(ProjectName);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllSessionMastersByProject", ex);
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateSessionMaster(SessionMaster SessionMaster)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.CreateSessionMaster(SessionMaster);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/CreateSessionMaster", ex);
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSessionMaster(SessionMaster SessionMaster)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.UpdateSessionMaster(SessionMaster);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/UpdateSessionMaster", ex);
                return BadRequest(ex.Message);
            }
            //return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSessionMaster(SessionMaster SessionMaster)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = await _masterRepository.DeleteSessionMaster(SessionMaster);
                return new OkResult();
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/DeleteSessionMaster", ex);
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region LogInAndChangePassword

        [HttpPost]
        public IActionResult LoginHistory(Guid UserID, string Username)
        {
            try
            {
                var result = _masterRepository.LoginHistory(UserID, Username);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/LoginHistory : - ", ex);
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public List<UserLoginHistory> GetAllUsersLoginHistory()
        {
            try
            {
                var result = _masterRepository.GetAllUsersLoginHistory();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllUsersLoginHistory : - ", ex);
                return null;
            }
        }
        [HttpGet]
        public List<UserLoginHistory> GetCurrentUserLoginHistory(Guid UserID)
        {
            try
            {
                var result = _masterRepository.GetCurrentUserLoginHistory(UserID);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetCurrentUserLoginHistory : - ", ex);
                return null;
            }
        }
        [HttpGet]
        public async Task<IActionResult> SignOut(Guid UserID)
        {
            try
            {
                var result = await _masterRepository.SignOut(UserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/SignOut : - ", ex);
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        public List<UserLoginHistory> FilterLoginHistory(string UserName = null, DateTime? FromDate = null, DateTime? ToDate = null)
        {
            try
            {
                var result = _masterRepository.FilterLoginHistory(UserName, FromDate, ToDate);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/FilterLoginHistory : - ", ex);
                return null;
            }
        }

        #endregion

        #region ChangePassword

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePassword changePassword)
        {
            try
            {
                var result = await _masterRepository.ChangePassword(changePassword);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/ChangePassword : - ", ex);
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> SendResetLinkToMail(EmailModel emailModel)
        {
            try
            {
                var result = await _masterRepository.SendResetLinkToMail(emailModel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/SendResetLinkToMail : - ", ex);
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> ForgotPassword(ForgotPassword forgotPassword)
        {
            try
            {
                var result = await _masterRepository.ForgotPassword(forgotPassword);
                if (result.Comment == "Password is already used")
                {
                    return BadRequest("Password is already used");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/ForgotPassword : - ", ex);
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region UserPreferencesTheme

        [HttpGet]
        public UserPreference GetUserPreferenceByUserID(Guid UserID)
        {
            try
            {
                var UserPreference = _masterRepository.GetUserPrefercences(UserID);
                return UserPreference;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetUserPreferenceByUserID", ex);
                return null;
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetUserPreference(UserPreference UserPreference)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                else
                {
                    var result = await _masterRepository.SetUserPreference(UserPreference);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/CreateUser", ex);
                return BadRequest(ex.Message);
            }
        }
        #endregion

        [HttpGet]
        public List<UserWithRole> GetSupportDeskUsersByRoleName(string RoleName)
        {
            try
            {
                var userWithRole = _masterRepository.GetSupportDeskUsersByRoleName(RoleName);
                return userWithRole;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetSupportDeskUsersByRoleName", ex);
                return null;
            }
        }




    }
}
