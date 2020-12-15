using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using VSign.DbContexts;
using System.Net;
using System.Net.Mail;
using VSign.Models;
using System.Text;
using System.Web;

namespace VSign.Repositories
{
    public class MasterRepository : IMasterRepository
    {
        private readonly AuthContext _dbContext;
        IConfiguration _configuration;
        private int _tokenTimespan = 0;

        public MasterRepository(AuthContext context, IConfiguration configuration)
        {
            _dbContext = context;
            _configuration = configuration;
            try
            {
                var span = "30";
                if (span != "")
                    _tokenTimespan = Convert.ToInt32(span.ToString());
                if (_tokenTimespan <= 0)
                {
                    _tokenTimespan = 30;
                }
            }
            catch
            {
                _tokenTimespan = 30;
            }
        }

        #region Authentication

        public Client FindClient(string clientId)
        {
            try
            {
                var client = _dbContext.Clients.Find(clientId);
                return client;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/FindClient : - ", ex);
                return null;
            }
        }

        public AuthenticationResult AuthenticateUser(string UserName, string Password)
        {
            try
            {
                AuthenticationResult authenticationResult = new AuthenticationResult();
                List<string> MenuItemList = new List<string>();
                string MenuItemNames = "";
                string ProfilesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Profiles");
                string Profile = "Empty";
                User user = null;
                string isChangePasswordRequired = "No";
                string DefaultPassword = "Exalca@123";
                //string DefaultPassword = ConfigurationManager.AppSettings["DefaultPassword"];


                if (UserName.Contains('@') && UserName.Contains('.'))
                {
                    user = (from tb in _dbContext.Users
                            where tb.Email == UserName && tb.IsActive
                            select tb).FirstOrDefault();
                }
                else
                {
                    user = (from tb in _dbContext.Users
                            where tb.UserName == UserName && tb.IsActive
                            select tb).FirstOrDefault();
                }

                if (user != null)
                {
                    bool isValidUser = false;

                    string DecryptedPassword = Decrypt(user.Password, true);
                    isValidUser = DecryptedPassword == Password;
                    if (isValidUser)
                    {
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        if (Password == DefaultPassword)
                        {
                            isChangePasswordRequired = "Yes";
                        }
                        //MasterController MasterController = new MasterController();
                        //await MasterController.LoginHistory(user.UserID, user.UserName);
                        Role userRole = (from tb1 in _dbContext.Roles
                                         join tb2 in _dbContext.UserRoleMaps on tb1.RoleID equals tb2.RoleID
                                         join tb3 in _dbContext.Users on tb2.UserID equals tb3.UserID
                                         where tb3.UserID == user.UserID && tb1.IsActive && tb2.IsActive && tb3.IsActive
                                         select tb1).FirstOrDefault();

                        if (userRole != null)
                        {
                            MenuItemList = (from tb1 in _dbContext.Apps
                                            join tb2 in _dbContext.RoleAppMaps on tb1.AppID equals tb2.AppID
                                            where tb2.RoleID == userRole.RoleID && tb1.IsActive && tb2.IsActive
                                            select tb1.AppName).ToList();
                            foreach (string item in MenuItemList)
                            {
                                if (MenuItemNames == "")
                                {
                                    MenuItemNames = item;
                                }
                                else
                                {
                                    MenuItemNames += "," + item;
                                }
                            }
                        }
                        authenticationResult.IsSuccess = true;
                        authenticationResult.UserID = user.UserID;
                        authenticationResult.UserName = user.UserName;
                        authenticationResult.DisplayName = user.UserName;
                        authenticationResult.EmailAddress = user.Email;
                        authenticationResult.UserRole = userRole != null ? userRole.RoleName : string.Empty;
                        authenticationResult.MenuItemNames = MenuItemNames;
                        authenticationResult.IsChangePasswordRequired = isChangePasswordRequired;
                        authenticationResult.Profile = string.IsNullOrEmpty(Profile) ? "Empty" : Profile;
                    }
                    else
                    {
                        authenticationResult.IsSuccess = false;
                        authenticationResult.Message = "The user name or password is incorrect.";
                    }
                }
                else
                {
                    authenticationResult.IsSuccess = false;
                    authenticationResult.Message = "The user name or password is incorrect.";
                }

                return authenticationResult;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/AuthenticateUser : - ", ex);
                return null;
            }
        }

        #endregion

        #region User

        public List<UserWithRole> GetAllUsers()
        {

            try
            {
                var result = (from tb in _dbContext.Users
                              join tb1 in _dbContext.UserRoleMaps on tb.UserID equals tb1.UserID
                              where tb.IsActive && tb1.IsActive
                              select new
                              {
                                  tb.UserID,
                                  tb.UserName,
                                  tb.DisplayName,
                                  tb.Email,
                                  tb.ContactNumber,
                                  tb.Password,
                                  tb.IsActive,
                                  tb.CreatedOn,
                                  tb.ModifiedOn,
                                  tb1.RoleID,
                              }).ToList();

                List<UserWithRole> UserWithRoleList = new List<UserWithRole>();

                result.ForEach(record =>
                {
                    UserWithRoleList.Add(new UserWithRole()
                    {
                        UserID = record.UserID,
                        UserName = record.UserName,
                        Email = record.Email,
                        ContactNumber = record.ContactNumber,
                        Password = Decrypt(record.Password, true),
                        IsActive = record.IsActive,
                        CreatedOn = record.CreatedOn,
                        ModifiedOn = record.ModifiedOn,
                        DisplayName = record.DisplayName,
                        RoleID = record.RoleID
                    });

                });
                return UserWithRoleList;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllUsers : - ", ex);
                return null;
            }
        }

        public async Task<UserWithRole> CreateUser(UserWithRole userWithRole)
        {
            UserWithRole userResult = new UserWithRole();
            try
            {
                // Creating User
                User user1 = (from tb1 in _dbContext.Users
                              where tb1.UserName == userWithRole.UserName && tb1.IsActive
                              select tb1).FirstOrDefault();

                if (user1 == null)
                {
                    User user2 = (from tb1 in _dbContext.Users
                                  where tb1.Email == userWithRole.Email && tb1.IsActive
                                  select tb1).FirstOrDefault();
                    if (user2 == null)
                    {
                        //string DefaultPassword = ConfigurationManager.AppSettings["DefaultPassword"];
                        string DefaultPassword = "Exalca@123";
                        User user = new User();
                        user.UserID = Guid.NewGuid();
                        user.UserName = userWithRole.UserName;
                        user.Email = userWithRole.Email;
                        user.Password = Encrypt(DefaultPassword, true);
                        user.ContactNumber = userWithRole.ContactNumber;
                        user.DisplayName = userWithRole.DisplayName;
                        user.CreatedBy = userWithRole.CreatedBy;
                        user.IsActive = true;
                        user.CreatedOn = DateTime.Now;
                        var result = _dbContext.Users.Add(user);
                        //_dbContext.Users.Add(user);
                        await _dbContext.SaveChangesAsync();

                        UserRoleMap UserRole = new UserRoleMap()
                        {
                            RoleID = userWithRole.RoleID,
                            UserID = user.UserID,
                            IsActive = true,
                            CreatedOn = DateTime.Now
                        };
                        var result1 = _dbContext.UserRoleMaps.Add(UserRole);
                        await _dbContext.SaveChangesAsync();

                        userResult.UserName = user.UserName;
                        userResult.Email = user.Email;
                        userResult.ContactNumber = user.ContactNumber;
                        userResult.UserID = user.UserID;
                        userResult.Password = user.Password;
                        userResult.RoleID = UserRole.RoleID;
                        userResult.DisplayName = user.DisplayName;
                        // Attachment
                    }
                    else
                    {
                        return userResult;
                        //return BadRequest("User with same email address already exist");
                    }
                }
                else
                {
                    return userResult;
                    //return Content(HttpStatusCode.BadRequest, "User with same name already exist");
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/CreateUser : - ", ex);
                return null;
            }
            return userResult;
            //_dbContext.Users.Add(entity);
            //_dbContext.SaveChanges();
        }

        public async Task<UserWithRole> UpdateUser(UserWithRole userWithRole)
        {
            UserWithRole userResult = new UserWithRole();
            try
            {

                User user1 = (from tb1 in _dbContext.Users
                              where tb1.UserName == userWithRole.UserName && tb1.IsActive && tb1.UserID != userWithRole.UserID
                              select tb1).FirstOrDefault();

                if (user1 == null)
                {
                    User user2 = (from tb1 in _dbContext.Users
                                  where tb1.Email == userWithRole.Email && tb1.IsActive && tb1.UserID != userWithRole.UserID
                                  select tb1).FirstOrDefault();
                    if (user2 == null)
                    {
                        //Updating User details
                        var user = (from tb in _dbContext.Users
                                    where tb.IsActive &&
                                    tb.UserID == userWithRole.UserID
                                    select tb).FirstOrDefault();
                        user.UserName = userWithRole.UserName;
                        user.Email = userWithRole.Email;
                        //user.Password = Encrypt(userWithRole.Password, true);
                        user.ContactNumber = userWithRole.ContactNumber;
                        user.DisplayName = userWithRole.DisplayName;
                        user.IsActive = true;
                        user.ModifiedOn = DateTime.Now;
                        user.ModifiedBy = userWithRole.ModifiedBy;
                        await _dbContext.SaveChangesAsync();

                        UserRoleMap OldUserRole = _dbContext.UserRoleMaps.Where(x => x.UserID == userWithRole.UserID && x.IsActive).FirstOrDefault();
                        if (OldUserRole.RoleID != userWithRole.RoleID)
                        {
                            //Delete old role related to the user
                            _dbContext.UserRoleMaps.Remove(OldUserRole);
                            _dbContext.SaveChanges();

                            //Add new roles for the user
                            UserRoleMap UserRole = new UserRoleMap()
                            {
                                RoleID = userWithRole.RoleID,
                                UserID = user.UserID,
                                IsActive = true,
                                CreatedBy = userWithRole.ModifiedBy,
                                CreatedOn = DateTime.Now,
                            };
                            var r = _dbContext.UserRoleMaps.Add(UserRole);
                            await _dbContext.SaveChangesAsync();

                            userResult.UserName = user.UserName;
                            userResult.Email = user.Email;
                            userResult.ContactNumber = user.ContactNumber;
                            userResult.UserID = user.UserID;
                            userResult.Password = user.Password;
                            userResult.RoleID = UserRole.RoleID;
                            userResult.DisplayName = user.DisplayName;
                        }

                    }
                    else
                    {
                        return userResult;
                        //return Content(HttpStatusCode.BadRequest, "User with same email address already exist");
                    }
                }
                else
                {
                    return userResult;
                    //return Content(HttpStatusCode.BadRequest, "User with same name already exist");
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/UpdateUser : - ", ex);
                return null;
            }
            return userResult;
        }

        public async Task<UserWithRole> DeleteUser(UserWithRole userWithRole)
        {
            UserWithRole userResult = new UserWithRole();
            try
            {
                var result = (from tb in _dbContext.Users
                              where tb.IsActive &&
                              tb.UserID == userWithRole.UserID
                              select tb).FirstOrDefault();
                if (result == null)
                {
                    return userResult;
                }
                else
                {
                    //result.IsActive = false;
                    //result.ModifiedOn = DateTime.Now;
                    //result.ModifiedBy = userWithRole.ModifiedBy;
                    _dbContext.Users.Remove(result);
                    await _dbContext.SaveChangesAsync();

                    //Changing the Status of role related to the user
                    UserRoleMap UserRole = _dbContext.UserRoleMaps.Where(x => x.UserID == userWithRole.UserID && x.IsActive).FirstOrDefault();
                    //UserRole.IsActive = false;
                    //UserRole.ModifiedOn = DateTime.Now;
                    //UserRole.ModifiedBy = userWithRole.ModifiedBy;
                    _dbContext.UserRoleMaps.Remove(UserRole);
                    await _dbContext.SaveChangesAsync();

                    userResult.UserName = result.UserName;
                    userResult.Email = result.Email;
                    userResult.ContactNumber = result.ContactNumber;
                    userResult.UserID = result.UserID;
                    userResult.Password = result.Password;
                    userResult.RoleID = UserRole.RoleID;
                    return userResult;
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/DeleteUser : - ", ex);
                return null;
            }
        }
        #endregion
        #region Role

        public List<RoleWithApp> GetAllRoles()
        {
            try
            {
                List<RoleWithApp> RoleWithAppList = new List<RoleWithApp>();
                List<Role> RoleList = (from tb in _dbContext.Roles
                                       where tb.IsActive
                                       select tb).ToList();
                foreach (Role rol in RoleList)
                {
                    RoleWithAppList.Add(new RoleWithApp()
                    {
                        RoleID = rol.RoleID,
                        RoleName = rol.RoleName,
                        IsActive = rol.IsActive,
                        CreatedOn = rol.CreatedOn,
                        ModifiedOn = rol.ModifiedOn,
                        AppIDList = _dbContext.RoleAppMaps.Where(x => x.RoleID == rol.RoleID && x.IsActive).Select(r => r.AppID).ToArray()
                    });
                }
                return RoleWithAppList;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllRoles : - ", ex);
                return null;

            }
        }

        public async Task<RoleWithApp> CreateRole(RoleWithApp roleWithApp)
        {
            RoleWithApp roleResult = new RoleWithApp();
            try
            {
                Role role1 = (from tb in _dbContext.Roles
                              where tb.IsActive && tb.RoleName == roleWithApp.RoleName
                              select tb).FirstOrDefault();
                if (role1 == null)
                {
                    Role role = new Role();
                    role.RoleID = Guid.NewGuid();
                    role.RoleName = roleWithApp.RoleName;
                    role.CreatedOn = DateTime.Now;
                    role.CreatedBy = roleWithApp.CreatedBy;
                    role.IsActive = true;
                    var result = _dbContext.Roles.Add(role);
                    await _dbContext.SaveChangesAsync();

                    foreach (int AppID in roleWithApp.AppIDList)
                    {
                        RoleAppMap roleApp = new RoleAppMap()
                        {
                            AppID = AppID,
                            RoleID = role.RoleID,
                            IsActive = true,
                            CreatedOn = DateTime.Now
                        };
                        _dbContext.RoleAppMaps.Add(roleApp);
                    }
                    await _dbContext.SaveChangesAsync();
                    roleResult.RoleName = roleWithApp.RoleName;
                    roleResult.RoleID = roleWithApp.RoleID;
                    roleResult.AppIDList = roleWithApp.AppIDList;
                }
                else
                {
                    return roleResult;
                    //return Content(HttpStatusCode.BadRequest, "Role with same name already exist");
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/CreateRole : - ", ex);
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return roleResult;
        }

        public async Task<RoleWithApp> UpdateRole(RoleWithApp roleWithApp)
        {
            RoleWithApp roleResult = new RoleWithApp();
            try
            {
                Role role = (from tb in _dbContext.Roles
                             where tb.IsActive && tb.RoleName == roleWithApp.RoleName && tb.RoleID != roleWithApp.RoleID
                             select tb).FirstOrDefault();
                if (role == null)
                {
                    Role role1 = (from tb in _dbContext.Roles
                                  where tb.IsActive && tb.RoleID == roleWithApp.RoleID
                                  select tb).FirstOrDefault();
                    role1.RoleName = roleWithApp.RoleName;
                    role1.IsActive = true;
                    role1.ModifiedOn = DateTime.Now;
                    role1.ModifiedBy = roleWithApp.ModifiedBy;
                    await _dbContext.SaveChangesAsync();

                    List<RoleAppMap> OldRoleAppList = _dbContext.RoleAppMaps.Where(x => x.RoleID == roleWithApp.RoleID && x.IsActive).ToList();
                    List<RoleAppMap> NeedToRemoveRoleAppList = OldRoleAppList.Where(x => !roleWithApp.AppIDList.Any(y => y == x.AppID)).ToList();
                    List<int> NeedToAddAppList = roleWithApp.AppIDList.Where(x => !OldRoleAppList.Any(y => y.AppID == x)).ToList();

                    //Delete Old RoleApps which is not exist in new List
                    NeedToRemoveRoleAppList.ForEach(x =>
                    {
                        _dbContext.RoleAppMaps.Remove(x);
                    });
                    await _dbContext.SaveChangesAsync();

                    //Create New RoleApps
                    foreach (int AppID in NeedToAddAppList)
                    {
                        RoleAppMap roleApp = new RoleAppMap()
                        {
                            AppID = AppID,
                            RoleID = role1.RoleID,
                            IsActive = true,
                            CreatedOn = DateTime.Now,
                        };
                        _dbContext.RoleAppMaps.Add(roleApp);
                    }
                    await _dbContext.SaveChangesAsync();
                    roleResult.RoleName = roleWithApp.RoleName;
                    roleResult.RoleID = roleWithApp.RoleID;
                    roleResult.AppIDList = roleWithApp.AppIDList;
                }
                else
                {
                    return roleResult;
                    //return Content(HttpStatusCode.BadRequest, "Role with same name already exist");
                }

            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/UpdateRole : - ", ex);
                return null;
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return roleResult;
        }

        public async Task<RoleWithApp> DeleteRole(RoleWithApp roleWithApp)
        {
            RoleWithApp roleResult = new RoleWithApp();
            try
            {
                Role role1 = (from tb in _dbContext.Roles
                              where tb.IsActive && tb.RoleID == roleWithApp.RoleID
                              select tb).FirstOrDefault();
                if (role1 == null)
                {
                    return roleResult;
                }
                else
                {
                    //role1.IsActive = false;
                    //role1.ModifiedOn = DateTime.Now;
                    _dbContext.Roles.Remove(role1);
                    await _dbContext.SaveChangesAsync();

                    //Change the status of the RoleApps related to the role
                    List<RoleAppMap> RoleAppList = _dbContext.RoleAppMaps.Where(x => x.RoleID == roleWithApp.RoleID && x.IsActive).ToList();
                    RoleAppList.ForEach(x =>
                    {
                        //x.IsActive = false;
                        //x.ModifiedOn = DateTime.Now;
                        //x.ModifiedBy = roleWithApp.ModifiedBy;
                        _dbContext.RoleAppMaps.Remove(x);
                    });
                    await _dbContext.SaveChangesAsync();
                    roleResult.RoleName = role1.RoleName;
                    roleResult.RoleID = role1.RoleID;

                    return roleResult;
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/DeleteRole : - ", ex);
                return null;
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion

        #region App

        public List<App> GetAllApps()
        {
            try
            {
                var result = (from tb in _dbContext.Apps
                              where tb.IsActive
                              select tb).ToList();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllApps : - ", ex);
                return null;

            }
        }

        public async Task<App> CreateApp(App App)
        {
            App appResult = new App();
            try
            {
                App App1 = (from tb in _dbContext.Apps
                            where tb.IsActive && tb.AppName == App.AppName
                            select tb).FirstOrDefault();
                if (App1 == null)
                {
                    App.CreatedOn = DateTime.Now;
                    App.IsActive = true;
                    var result = _dbContext.Apps.Add(App);
                    await _dbContext.SaveChangesAsync();

                    appResult.AppName = App.AppName;
                    appResult.AppID = App.AppID;
                }
                else
                {
                    return appResult;
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/CreateApp : - ", ex);
                return null;
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return appResult;
        }

        public async Task<App> UpdateApp(App App)
        {
            App appResult = new App();
            try
            {
                App App1 = (from tb in _dbContext.Apps
                            where tb.IsActive && tb.AppName == App.AppName && tb.AppID != App.AppID
                            select tb).FirstOrDefault();
                if (App1 == null)
                {
                    App App2 = (from tb in _dbContext.Apps
                                where tb.IsActive && tb.AppID == App.AppID
                                select tb).FirstOrDefault();
                    App2.AppName = App.AppName;
                    App2.IsActive = true;
                    App2.ModifiedOn = DateTime.Now;
                    App2.ModifiedBy = App.ModifiedBy;
                    await _dbContext.SaveChangesAsync();
                    appResult.AppName = App.AppName;
                    appResult.AppID = App.AppID;
                }
                else
                {
                    return appResult;
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/UpdateApp : - ", ex);
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return appResult;
        }

        public async Task<App> DeleteApp(App App)
        {
            App appResult = new App();
            try
            {
                App App1 = (from tb in _dbContext.Apps
                            where tb.IsActive && tb.AppID == App.AppID
                            select tb).FirstOrDefault();
                if (App1 != null)
                {
                    _dbContext.Apps.Remove(App1);
                    await _dbContext.SaveChangesAsync();
                    appResult.AppName = App1.AppName;
                    appResult.AppID = App1.AppID;
                }
                else
                {
                    return appResult;
                }
                //App1.IsActive = false;
                //App1.ModifiedOn = DateTime.Now;
                //App1.ModifiedBy = App.ModifiedBy;

            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/DeleteApp : - ", ex);
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return appResult;
        }

        #endregion

        #region AppUsage

        public List<AppUsageView> GetAllAppUsages()
        {
            try
            {
                var result = (from tb in _dbContext.AppUsages
                              join tb1 in _dbContext.Users on tb.UserID equals tb1.UserID
                              join tb2 in _dbContext.UserRoleMaps on tb1.UserID equals tb2.UserID
                              join tb3 in _dbContext.Roles on tb2.RoleID equals tb3.RoleID
                              where tb.IsActive
                              select new AppUsageView()
                              {
                                  ID = tb.ID,
                                  UserID = tb.UserID,
                                  UserName = tb1.UserName,
                                  UserRole = tb3.RoleName,
                                  AppName = tb.AppName,
                                  UsageCount = tb.UsageCount,
                                  LastUsedOn = tb.LastUsedOn,
                                  IsActive = tb.IsActive,
                                  CreatedOn = tb.CreatedOn,
                                  CreatedBy = tb.CreatedBy,
                                  ModifiedOn = tb.ModifiedOn,
                                  ModifiedBy = tb.ModifiedBy
                              }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllAppUsages : - ", ex);
                return null;

            }
        }

        public List<AppUsageView> GetAppUsagesByUser(Guid UserID)
        {
            try
            {
                var result = (from tb in _dbContext.AppUsages
                              join tb1 in _dbContext.Users on tb.UserID equals tb1.UserID
                              join tb2 in _dbContext.UserRoleMaps on tb1.UserID equals tb2.UserID
                              join tb3 in _dbContext.Roles on tb2.RoleID equals tb3.RoleID
                              where tb.IsActive && tb.UserID == UserID
                              select new AppUsageView()
                              {
                                  ID = tb.ID,
                                  UserID = tb.UserID,
                                  UserName = tb1.UserName,
                                  UserRole = tb3.RoleName,
                                  AppName = tb.AppName,
                                  UsageCount = tb.UsageCount,
                                  LastUsedOn = tb.LastUsedOn,
                                  IsActive = tb.IsActive,
                                  CreatedOn = tb.CreatedOn,
                                  CreatedBy = tb.CreatedBy,
                                  ModifiedOn = tb.ModifiedOn,
                                  ModifiedBy = tb.ModifiedBy
                              }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAppUsagesByUser : - ", ex);
                return null;

            }
        }

        public async Task<AppUsage> CreateAppUsage(AppUsage AppUsage)
        {
            AppUsage AppUsageResult = new AppUsage();
            try
            {
                AppUsage AppUsage1 = (from tb in _dbContext.AppUsages
                                      where tb.IsActive && tb.UserID == AppUsage.UserID && tb.AppName == AppUsage.AppName
                                      select tb).FirstOrDefault();
                if (AppUsage1 == null)
                {
                    AppUsage.UsageCount = 1;
                    AppUsage.LastUsedOn = DateTime.Now;
                    AppUsage.CreatedOn = DateTime.Now;
                    AppUsage.IsActive = true;
                    var result = _dbContext.AppUsages.Add(AppUsage);
                    await _dbContext.SaveChangesAsync();
                    return result.Entity;
                }
                else
                {
                    AppUsage1.UsageCount += 1;
                    AppUsage1.LastUsedOn = DateTime.Now;
                    AppUsage1.ModifiedOn = DateTime.Now;
                    AppUsage1.ModifiedBy = AppUsage.ModifiedBy;
                    await _dbContext.SaveChangesAsync();
                    return AppUsage1;
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/CreateAppUsage : - ", ex);
                return null;
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        //public async Task<AppUsage> UpdateAppUsage(AppUsage AppUsage)
        //{
        //    AppUsage AppUsageResult = new AppUsage();
        //    try
        //    {
        //        AppUsage AppUsage1 = (from tb in _dbContext.AppUsages
        //                    where tb.IsActive && tb.AppUsageName == AppUsage.AppUsageName && tb.ID != AppUsage.ID
        //                    select tb).FirstOrDefault();
        //        if (AppUsage1 == null)
        //        {
        //            AppUsage AppUsage2 = (from tb in _dbContext.AppUsages
        //                        where tb.IsActive && tb.ID == AppUsage.ID
        //                        select tb).FirstOrDefault();
        //            AppUsage2.AppUsageName = AppUsage.AppUsageName;
        //            AppUsage2.IsActive = true;
        //            AppUsage2.ModifiedOn = DateTime.Now;
        //            AppUsage2.ModifiedBy = AppUsage.ModifiedBy;
        //            await _dbContext.SaveChangesAsync();
        //            AppUsageResult.AppUsageName = AppUsage.AppUsageName;
        //            AppUsageResult.ID = AppUsage.ID;
        //        }
        //        else
        //        {
        //            return AppUsageResult;
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //        ErrorLog.WriteToFile("Master/UpdateAppUsage : - ", ex);
        //        //return Content(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //    return AppUsageResult;
        //}

        //public async Task<AppUsage> DeleteAppUsage(AppUsage AppUsage)
        //{
        //    AppUsage AppUsageResult = new AppUsage();
        //    try
        //    {
        //        AppUsage AppUsage1 = (from tb in _dbContext.AppUsages
        //                    where tb.IsActive && tb.ID == AppUsage.ID
        //                    select tb).FirstOrDefault();
        //        if (AppUsage1 != null)
        //        {
        //            _dbContext.AppUsages.Remove(AppUsage1);
        //            await _dbContext.SaveChangesAsync();
        //            AppUsageResult.AppUsageName = AppUsage1.AppUsageName;
        //            AppUsageResult.ID = AppUsage1.ID;
        //        }
        //        else
        //        {
        //            return AppUsageResult;
        //        }
        //        //AppUsage1.IsActive = false;
        //        //AppUsage1.ModifiedOn = DateTime.Now;
        //        //AppUsage1.ModifiedBy = AppUsage.ModifiedBy;

        //    }
        //    catch (Exception ex)
        //    {

        //        ErrorLog.WriteToFile("Master/DeleteAppUsage : - ", ex);
        //        //return Content(HttpStatusCode.InternalServerError, ex.Message);
        //    }
        //    return AppUsageResult;
        //}

        #endregion

        #region SessionMaster

        public SessionMaster GetSessionMasterByProject(string ProjectName)
        {
            try
            {
                SessionMaster sessionMasters = (from tb in _dbContext.SessionMasters
                                                where tb.IsActive && tb.ProjectName.ToLower() == ProjectName.ToLower()
                                                select tb).FirstOrDefault();

                return sessionMasters;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetSessionMasterByProject : - ", ex);
                return null;
            }
        }

        public List<SessionMaster> GetAllSessionMasters()
        {
            try
            {
                var result = (from tb in _dbContext.SessionMasters
                              where tb.IsActive
                              select tb).ToList();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllSessionMasters : - ", ex);
                return null;

            }
        }

        public List<SessionMaster> GetAllSessionMastersByProject(string ProjectName)
        {
            try
            {
                var result = (from tb in _dbContext.SessionMasters
                              where tb.IsActive && tb.ProjectName.ToLower() == ProjectName.ToLower()
                              select tb).ToList();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllSessionMastersByProject : - ", ex);
                return null;

            }
        }


        public async Task<SessionMaster> CreateSessionMaster(SessionMaster SessionMaster)
        {
            SessionMaster SessionMasterResult = new SessionMaster();
            try
            {
                SessionMaster SessionMaster1 = (from tb in _dbContext.SessionMasters
                                                where tb.IsActive && tb.ProjectName == SessionMaster.ProjectName
                                                select tb).FirstOrDefault();
                if (SessionMaster1 == null)
                {
                    SessionMaster.CreatedOn = DateTime.Now;
                    SessionMaster.IsActive = true;
                    var result = _dbContext.SessionMasters.Add(SessionMaster);
                    await _dbContext.SaveChangesAsync();

                    SessionMasterResult.ID = SessionMaster.ID;
                    SessionMasterResult.ProjectName = SessionMaster.ProjectName;
                    SessionMasterResult.SessionTimeOut = SessionMaster.SessionTimeOut;
                }
                else
                {
                    return SessionMasterResult;
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/CreateSessionMaster : - ", ex);
                return null;
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return SessionMasterResult;
        }

        public async Task<SessionMaster> UpdateSessionMaster(SessionMaster SessionMaster)
        {
            SessionMaster SessionMasterResult = new SessionMaster();
            try
            {
                SessionMaster SessionMaster1 = (from tb in _dbContext.SessionMasters
                                                where tb.IsActive && tb.ProjectName == SessionMaster.ProjectName && tb.ID != SessionMaster.ID
                                                select tb).FirstOrDefault();
                if (SessionMaster1 == null)
                {
                    SessionMaster SessionMaster2 = (from tb in _dbContext.SessionMasters
                                                    where tb.IsActive && tb.ID == SessionMaster.ID
                                                    select tb).FirstOrDefault();
                    SessionMaster2.SessionTimeOut = SessionMaster.SessionTimeOut;
                    SessionMaster2.IsActive = true;
                    SessionMaster2.ModifiedOn = DateTime.Now;
                    SessionMaster2.ModifiedBy = SessionMaster.ModifiedBy;
                    await _dbContext.SaveChangesAsync();
                    SessionMasterResult.ID = SessionMaster.ID;
                    SessionMasterResult.ProjectName = SessionMaster.ProjectName;
                    SessionMasterResult.SessionTimeOut = SessionMaster.SessionTimeOut;
                }
                else
                {
                    return SessionMasterResult;
                }
            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/UpdateSessionMaster : - ", ex);
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return SessionMasterResult;
        }

        public async Task<SessionMaster> DeleteSessionMaster(SessionMaster SessionMaster)
        {
            SessionMaster SessionMasterResult = new SessionMaster();
            try
            {
                SessionMaster SessionMaster1 = (from tb in _dbContext.SessionMasters
                                                where tb.IsActive && tb.ID == SessionMaster.ID
                                                select tb).FirstOrDefault();
                if (SessionMaster1 != null)
                {
                    _dbContext.SessionMasters.Remove(SessionMaster1);
                    await _dbContext.SaveChangesAsync();
                    SessionMasterResult.ID = SessionMaster.ID;
                    SessionMasterResult.ProjectName = SessionMaster.ProjectName;
                    SessionMasterResult.SessionTimeOut = SessionMaster.SessionTimeOut;
                }
                else
                {
                    return SessionMasterResult;
                }
                //SessionMaster1.IsActive = false;
                //SessionMaster1.ModifiedOn = DateTime.Now;
                //SessionMaster1.ModifiedBy = SessionMaster.ModifiedBy;

            }
            catch (Exception ex)
            {

                ErrorLog.WriteToFile("Master/DeleteSessionMaster : - ", ex);
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return SessionMasterResult;
        }

        #endregion

        #region LogInAndChangePassword

        public async Task<UserLoginHistory> LoginHistory(Guid UserID, string Username)
        {
            try
            {
                UserLoginHistory loginData = new UserLoginHistory();
                loginData.UserID = UserID;
                loginData.UserName = Username;
                loginData.LoginTime = DateTime.Now;
                IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                _dbContext.UserLoginHistory.Add(loginData);
                await _dbContext.SaveChangesAsync();
                return loginData;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/LoginHistory : - ", ex);
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
                return null;
            }

        }

        public List<UserLoginHistory> GetAllUsersLoginHistory()
        {
            try
            {
                var UserLoginHistoryList = (from tb1 in _dbContext.UserLoginHistory
                                            orderby tb1.LoginTime descending
                                            select tb1).ToList();

                return UserLoginHistoryList;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllUsersLoginHistory : ", ex);
                return null;
            }
        }

        public List<UserLoginHistory> GetCurrentUserLoginHistory(Guid UserID)
        {
            try
            {
                var UserLoginHistoryList = (from tb1 in _dbContext.UserLoginHistory
                                            where tb1.UserID == UserID
                                            orderby tb1.LoginTime descending
                                            select tb1).ToList();
                return UserLoginHistoryList;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetCurrentUserLoginHistory : ", ex);
                return null;
            }
        }

        public async Task<UserLoginHistory> SignOut(Guid UserID)
        {
            try
            {
                var result = _dbContext.UserLoginHistory.Where(data => data.UserID == UserID).OrderByDescending(d => d.LoginTime).FirstOrDefault();
                result.LogoutTime = DateTime.Now;
                await _dbContext.SaveChangesAsync();
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/SignOut : - ", ex);
                return null;
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }

        }

        public List<UserLoginHistory> FilterLoginHistory(string UserName = null, DateTime? FromDate = null, DateTime? ToDate = null)
        {
            try
            {
                bool IsUserName = !string.IsNullOrEmpty(UserName);
                bool IsFromDate = FromDate.HasValue;
                bool IsToDate = ToDate.HasValue;
                var result = (from tb in _dbContext.UserLoginHistory
                              where (!IsUserName || tb.UserName.ToLower().Contains(UserName.ToLower()))
                              && (!IsFromDate || (tb.LoginTime.Date >= FromDate.Value.Date))
                              && (!IsToDate || (tb.LoginTime.Date <= ToDate.Value.Date))
                              select tb).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region ChangePassword

        public async Task<User> ChangePassword(ChangePassword changePassword)
        {
            User userResult = new User();
            try
            {
                User user = (from tb in _dbContext.Users
                             where tb.UserName == changePassword.UserName && tb.IsActive
                             select tb).FirstOrDefault();
                if (user != null)
                {
                    string DecryptedPassword = Decrypt(user.Password, true);
                    if (DecryptedPassword == changePassword.CurrentPassword)
                    {
                        //string DefaultPassword = ConfigurationManager.AppSettings["DefaultPassword"];
                        string DefaultPassword = "Exalca@123";
                        if (changePassword.NewPassword == DefaultPassword)
                        {
                            //return Content(HttpStatusCode.BadRequest, "New password should be different from default password.");
                            return userResult;
                        }
                        else
                        {
                            user.Password = Encrypt(changePassword.NewPassword, true);
                            user.IsActive = true;
                            user.ModifiedOn = DateTime.Now;
                            await _dbContext.SaveChangesAsync();
                            userResult = user;
                        }
                    }
                    else
                    {
                        //return Content(HttpStatusCode.BadRequest, "Current password is incorrect.");
                        return userResult;
                    }
                }
                else
                {
                    return userResult;
                    //return Content(HttpStatusCode.BadRequest, "The user name or password is incorrect.");
                }
            }

            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/ChangePassword : - ", ex);
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return userResult;
        }

        public async Task<TokenHistory> SendResetLinkToMail(EmailModel emailModel)
        {
            TokenHistory tokenHistoryResult = new TokenHistory();
            try
            {
                DateTime ExpireDateTime = DateTime.Now.AddMinutes(_tokenTimespan);
                User user = (from tb in _dbContext.Users
                             where tb.Email == emailModel.EmailAddress && tb.IsActive
                             select tb).FirstOrDefault();

                if (user != null)
                {
                    string code = Encrypt(user.UserID.ToString() + '|' + user.UserName + '|' + ExpireDateTime, true);

                    bool sendresult = await SendMail(HttpUtility.UrlEncode(code), user.UserName, user.Email, null, "", user.UserID.ToString(), emailModel.siteURL);
                    if (sendresult)
                    {
                        try
                        {
                            TokenHistory history1 = (from tb in _dbContext.TokenHistories
                                                     where tb.UserID == user.UserID && !tb.IsUsed
                                                     select tb).FirstOrDefault();
                            if (history1 == null)
                            {
                                TokenHistory history = new TokenHistory()
                                {
                                    UserID = user.UserID,
                                    Token = code,
                                    EmailAddress = user.Email,
                                    CreatedOn = DateTime.Now,
                                    ExpireOn = ExpireDateTime,
                                    IsUsed = false,
                                    Comment = "Token sent successfully"
                                };
                                var result = _dbContext.TokenHistories.Add(history);
                            }
                            else
                            {
                                //ErrorLog.WriteToFile("Master/SendLinkToMail : Token already present, updating new token to the user whose mail id is " +user.Email);
                                history1.Token = code;
                                history1.CreatedOn = DateTime.Now;
                                history1.ExpireOn = ExpireDateTime;
                            }
                            await _dbContext.SaveChangesAsync();

                            tokenHistoryResult = history1;
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.WriteToFile("Master/SendLinkToMail : Add record to TokenHistories - ", ex);
                        }
                        return tokenHistoryResult;
                        //return Content(HttpStatusCode.OK, string.Format("Reset password link sent successfully to {0}", user.Email));
                    }
                    else
                    {
                        return tokenHistoryResult;
                        //return Content(HttpStatusCode.BadRequest, "Sorry! There is some problem on sending mail");
                    }
                }
                else
                {
                    return tokenHistoryResult;
                    //return Content(HttpStatusCode.BadRequest, "Your email address is not registered!");
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/SendLinkToMail : - ", ex);
                return null;
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        public async Task<TokenHistory> ForgotPassword(ForgotPassword forgotPassword)
        {
            string[] decryptedArray = new string[3];
            string result = string.Empty;
            TokenHistory tokenHistoryResult = new TokenHistory();
            try
            {
                try
                {
                    result = Decrypt(forgotPassword.Token, true);
                }
                catch
                {
                    return tokenHistoryResult;
                    //return Content(HttpStatusCode.BadRequest, "Invalid token!");
                    //var errors = new string[] { "Invalid token!" };
                    //IHttpActionResult errorResult = GetErrorResult(new IdentityResult(errors));
                    //return errorResult;
                }
                if (result.Contains('|') && result.Split('|').Length == 3)
                {
                    decryptedArray = result.Split('|');
                }
                else
                {
                    return tokenHistoryResult;
                    //return Content(HttpStatusCode.BadRequest, "Invalid token!");
                }

                if (decryptedArray.Length == 3)
                {
                    DateTime date = DateTime.Parse(decryptedArray[2].Replace('+', ' '));
                    if (DateTime.Now > date)// Convert.ToDateTime(decryptedarray[2]))
                    {
                        return tokenHistoryResult;
                        //return Content(HttpStatusCode.BadRequest, "Reset password link expired!");
                    }
                    var DecryptedUserID = decryptedArray[0];

                    User user = (from tb in _dbContext.Users
                                 where tb.UserID.ToString() == DecryptedUserID && tb.IsActive
                                 select tb).FirstOrDefault();

                    if (user.UserName == decryptedArray[1] && forgotPassword.UserID == user.UserID)
                    {
                        try
                        {
                            TokenHistory history = _dbContext.TokenHistories.Where(x => x.UserID == user.UserID && !x.IsUsed && x.Token == forgotPassword.Token).Select(r => r).FirstOrDefault();
                            if (history == null)
                            {

                                if (user.Password != forgotPassword.NewPassword
                                   // && user.Pass1 != forgotPassword.NewPassword
                                  //  && user.Pass2 != forgotPassword.NewPassword
                                  //  && user.Pass3 != forgotPassword.NewPassword
                                    )
                                {

                                   // user.Pass3 = user.Pass2;
                                  //  user.Pass2 = user.Pass1;
                                  //  user.Pass1 = forgotPassword.NewPassword;
                                    user.Password = Encrypt(forgotPassword.NewPassword, true);
                                    user.IsActive = true;
                                   // user.ExpiringOn = DateTime.Now.AddDays(90);
                                    user.ModifiedOn = DateTime.Now;
                                    await _dbContext.SaveChangesAsync();
                                }
                                else
                                {
                                    history.Comment = "Password is already used";
                                    return history;
                                }

                                // Updating Password

                                // Updating TokenHistory
                                history.UsedOn = DateTime.Now;
                                history.IsUsed = true;
                                history.Comment = "Token Used successfully";
                                await _dbContext.SaveChangesAsync();

                                tokenHistoryResult = history;
                            }
                            else
                            {
                                return tokenHistoryResult;
                                //return Content(HttpStatusCode.BadRequest, "Token might have already used or wrong token");
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLog.WriteToFile("Master/ForgotPassword : Getting TokenHistory - ", ex);
                            return null;
                            //return Content(HttpStatusCode.InternalServerError, ex.Message);
                        }

                    }
                    else
                    {
                        return tokenHistoryResult;
                        //return Content(HttpStatusCode.BadRequest, "Invalid token!");
                    }


                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/ForgotPassword : - ", ex);
                return null;
                //return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return tokenHistoryResult;
        }

        #endregion

        #region sendMail

        public async Task<bool> SendMail(string code, string UserName, string toEmail, string password, string type, string userID, string siteURL)
        {
            try
            {
                //string hostName = ConfigurationManager.AppSettings["HostName"];
                //string SMTPEmail = ConfigurationManager.AppSettings["SMTPEmail"];
                ////string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                //string SMTPEmailPassword = ConfigurationManager.AppSettings["SMTPEmailPassword"];
                //string SMTPPort = ConfigurationManager.AppSettings["SMTPPort"];
                string hostName = "smtp.gmail.com";
                string SMTPEmail = "exalca.plant1@gmail.com";
                //string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string SMTPEmailPassword = "exalca@123";
                string SMTPPort = "587";
                var message = new MailMessage();
                string subject = "";
                StringBuilder sb = new StringBuilder();
                //string UserName = _dbContext.TBL_User_Master.Where(x => x.Email == toEmail).Select(y => y.UserName).FirstOrDefault();
                //UserName = string.IsNullOrEmpty(UserName) ? toEmail.Split('@')[0] : UserName;
                sb.Append(string.Format("Dear {0},<br/>", UserName));
                if (type == "ConfirmEmail")
                {

                    sb.Append("<p>Thank you for subscribing to Prestige Enterprise Signing Engine.</p>");
                    sb.Append("<p>Your sign in details have been authorised by us so you just need to verify your account by clicking <a href=\"" + siteURL + "/#/login?token=" + code + "&Id=" + userID + "\"" + ">here</a>. Once this has been done you will be redirected to the log in page where you can enter your credentials and start completing your profile.</p>");
                    sb.Append(String.Format("<p>User name: {0}</p>", toEmail));
                    sb.Append(String.Format("<p>Password: {0}</p>", password));
                    sb.Append("<i>Note: The verification link will expire in 2 hours.<i>");
                    sb.Append("<p>Regards,</p><p>Admin</p>");
                    subject = "Account verification";
                }
                else
                {
                    sb.Append("<p>We have received a request to reset your password, you can reset it now by clicking <a href=\"" + siteURL + "?token=" + code + "&Id=" + userID + "\"" + ">here</a>.<p></p></p>");
                    sb.Append("<p>Regards,</p><p>Admin</p>");
                    subject = "Reset password";
                }
                SmtpClient client = new SmtpClient();
                client.Port = Convert.ToInt32(SMTPPort);
                client.Host = hostName;
                client.EnableSsl = true;
                client.Timeout = 60000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(SMTPEmail, SMTPEmailPassword);
                MailMessage reportEmail = new MailMessage(SMTPEmail, toEmail, subject, sb.ToString());
                reportEmail.BodyEncoding = UTF8Encoding.UTF8;
                reportEmail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                reportEmail.IsBodyHtml = true;
                await client.SendMailAsync(reportEmail);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/SendMail : - ", ex);
                return false;
            }
        }

        public async Task<bool> SendMailToVendor(string toEmail, string password, string siteURL)
        {
            try
            {
                //string hostName = ConfigurationManager.AppSettings["HostName"];
                //string SMTPEmail = ConfigurationManager.AppSettings["SMTPEmail"];
                ////string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                //string SMTPEmailPassword = ConfigurationManager.AppSettings["SMTPEmailPassword"];
                //string SMTPPort = ConfigurationManager.AppSettings["SMTPPort"];
                var STMPDetailsConfig = _configuration.GetSection("STMPDetails");
                string hostName = STMPDetailsConfig["Host"];
                string SMTPEmail = STMPDetailsConfig["Email"];
                //string fromEmail = ConfigurationManager.AppSettings["FromEmail"];
                string SMTPEmailPassword = STMPDetailsConfig["Password"];
                string SMTPPort = STMPDetailsConfig["Port"];
                var message = new MailMessage();
                string subject = "";
                StringBuilder sb = new StringBuilder();
                //string UserName = _dbContext.TBL_User_Master.Where(x => x.Email == toEmail).Select(y => y.UserName).FirstOrDefault();
                //UserName = string.IsNullOrEmpty(UserName) ? toEmail.Split('@')[0] : UserName;
                sb.Append(string.Format("Dear {0},<br/>", toEmail));
                sb.Append("<p>Thank you for subscribing to BP Cloud.</p>");
                sb.Append("<p>Please Login by clicking <a href=\"" + siteURL + "/#/auth/login\">here</a></p>");
                sb.Append(string.Format("<p>User name: {0}</p>", toEmail));
                sb.Append(string.Format("<p>Password: {0}</p>", password));
                sb.Append("<p>Regards,</p><p>Admin</p>");
                subject = "BP Cloud Vendor Registration";
                SmtpClient client = new SmtpClient();
                client.Port = Convert.ToInt32(SMTPPort);
                client.Host = hostName;
                client.EnableSsl = true;
                client.Timeout = 60000;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;
                client.Credentials = new System.Net.NetworkCredential(SMTPEmail.Trim(), SMTPEmailPassword.Trim());
                MailMessage reportEmail = new MailMessage(SMTPEmail, toEmail, subject, sb.ToString());
                reportEmail.BodyEncoding = UTF8Encoding.UTF8;
                reportEmail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                reportEmail.IsBodyHtml = true;
                await client.SendMailAsync(reportEmail);
                return true;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/SendMail : - ", ex);
                throw ex;
            }
        }

        #endregion

        #region EncryptAndDecrypt

        public string Decrypt(string Password, bool UseHashing)
        {
            string EncryptionKey = "Exalca";
            byte[] KeyArray;
            byte[] ToEncryptArray = Convert.FromBase64String(Password);
            if (UseHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                KeyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(EncryptionKey));
                hashmd5.Clear();
            }
            else
            {
                KeyArray = UTF8Encoding.UTF8.GetBytes(EncryptionKey);
            }

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = KeyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 ToEncryptArray, 0, ToEncryptArray.Length);
            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public string Encrypt(string Password, bool useHashing)
        {
            //string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
            string EncryptionKey = "Exalca";
            byte[] KeyArray;
            byte[] ToEncryptArray = UTF8Encoding.UTF8.GetBytes(Password);
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                KeyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(EncryptionKey));
                hashmd5.Clear();
            }
            else
                KeyArray = UTF8Encoding.UTF8.GetBytes(EncryptionKey);

            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = KeyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray =
              cTransform.TransformFinalBlock(ToEncryptArray, 0,
              ToEncryptArray.Length);

            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        #endregion

        #region Userpreferences
        public UserPreference GetUserPrefercences(Guid userID)
        {
            try
            {
                UserPreference result = _dbContext.UserPreferences.FirstOrDefault(v => v.UserID == userID);
                return result;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetAllUserPrefercences : - ", ex);
                return null;

            }
        }

        public async Task<UserPreference> SetUserPreference(UserPreference UserPreference)
        {
            try
            {
                UserPreference UserPreference1 = new UserPreference();
                UserPreference existing = _dbContext.UserPreferences.FirstOrDefault(v => v.UserID == UserPreference.UserID);

                if (existing == null)
                {
                    UserPreference.CreatedOn = DateTime.Now;
                    UserPreference.ModifiedOn = DateTime.Now;
                    var result = _dbContext.UserPreferences.Add(UserPreference);
                }

                else
                {
                    existing.NavbarPrimaryBackground = UserPreference.NavbarPrimaryBackground;
                    existing.NavbarSecondaryBackground = UserPreference.NavbarSecondaryBackground;
                    existing.ToolbarBackground = UserPreference.ToolbarBackground;
                    existing.ModifiedOn = DateTime.Now;
                }

                await _dbContext.SaveChangesAsync();
                return UserPreference;

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/CreateUserPreference:-", ex);
                return null;
            }
        }


        #endregion


        public List<UserWithRole> GetSupportDeskUsersByRoleName(string RoleName)
        {
            try
            {
                List<UserWithRole> RoleWithAppList = new List<UserWithRole>();
                Role Role = (from tb in _dbContext.Roles
                             where tb.IsActive && tb.RoleName.ToLower() == RoleName.ToLower()
                             select tb).FirstOrDefault();
                var result = (from tb in _dbContext.Users
                              join tb1 in _dbContext.UserRoleMaps on tb.UserID equals tb1.UserID
                              where tb.IsActive && tb1.IsActive && tb1.RoleID == Role.RoleID
                              select new
                              {
                                  tb.UserID,
                                  tb.UserName,
                                  tb.Email,
                                  tb.ContactNumber,
                                  tb.Password,
                                  tb.IsActive,
                                  tb.CreatedOn,
                                  tb.ModifiedOn,
                                  tb1.RoleID,
                              }).ToList();
                List<UserWithRole> UserWithRoleList = new List<UserWithRole>();
                if (result != null)
                {
                    result.ForEach(record =>
                    {
                        UserWithRoleList.Add(new UserWithRole()
                        {
                            UserID = record.UserID,
                            UserName = record.UserName,
                            Email = record.Email,
                            ContactNumber = record.ContactNumber,
                            Password = Decrypt(record.Password, true),
                            IsActive = record.IsActive,
                            CreatedOn = record.CreatedOn,
                            ModifiedOn = record.ModifiedOn,
                            RoleID = record.RoleID
                        });

                    });
                }
                return UserWithRoleList;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("Master/GetSupportDeskUsersByRoleName : - ", ex);
                return null;
            }
        }

    }

}
