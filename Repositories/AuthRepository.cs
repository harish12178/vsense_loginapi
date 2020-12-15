using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.DirectoryServices;

using System.Threading.Tasks;
using VSign.DbContexts;
using VSign.Models;

namespace VSign.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AuthContext _dbContext;
        private readonly IMasterRepository _masterRepository;
        IConfiguration _configuration;
        public AuthRepository(AuthContext context, IMasterRepository masterRepository, IConfiguration configuration)
        {
            _dbContext = context;
            _masterRepository = masterRepository;
            _configuration = configuration;
        }

        public Client FindClient(string clientId)
        {
            try
            {
                var client = _dbContext.Clients.Find(clientId);

                return client;
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("AuthRepository/FindClient : - ", ex);
                return null;
            }

        }

        public async Task<AuthenticationResult> AuthenticateUser(string UserName, string Password)
        {
            try
            {
                string userId = String.Empty, role = String.Empty;
                //string IsLDAP = ConfigurationManager.AppSettings["IsLDAP"].ToString();
                string IsLDAP = _configuration.GetValue<string>("IsLDAP");
                List<string> MenuItemList = new List<string>();
                string MenuItemNames = "";
               // string Profile = "Empty";
                User user = null;
                string isChangePasswordRequired = "No";
                //string DefaultPassword = ConfigurationManager.AppSettings["DefaultPassword"];
                string DefaultPassword = _configuration.GetValue<string>("DefaultPassword");

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

                    if (IsLDAP.ToUpper() == "S")
                    {
                        LdapUser ldapUser = GetAuthenticatedLdapUser(UserName, Password);
                        isValidUser = ldapUser != null ? true : false;
                        if (isValidUser)
                        {
                            //MasterController MasterController = new MasterController();
                            await _masterRepository.LoginHistory(user.UserID, user.UserName);
                            Role userRole = (from tb1 in _dbContext.Roles
                                             join tb2 in _dbContext.UserRoleMaps on tb1.RoleID equals tb2.RoleID
                                             where tb2.UserID == user.UserID && tb1.IsActive && tb2.IsActive
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

                            var authenticationResult = new AuthenticationResult();
                            authenticationResult.UserID = user.UserID;
                            authenticationResult.UserName = ldapUser.UserName;
                            authenticationResult.DisplayName = ldapUser.DisplayName;
                            authenticationResult.EmailAddress = ldapUser.Email;
                            authenticationResult.UserRole = userRole != null ? userRole.RoleName : "";
                            authenticationResult.MenuItemNames = MenuItemNames;
                            authenticationResult.IsChangePasswordRequired = isChangePasswordRequired;
                            authenticationResult.Profile = "Empty";
                            return authenticationResult;
                        }
                        else
                        {
                            throw new Exception("The user name or password is incorrect.");
                        }
                    }
                    else
                    {
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
                            await _masterRepository.LoginHistory(user.UserID, user.UserName);
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

                            var authenticationResult = new AuthenticationResult();
                            authenticationResult.UserID = user.UserID;
                            authenticationResult.UserName = user.UserName;
                            authenticationResult.DisplayName = user.DisplayName;
                            authenticationResult.EmailAddress = user.Email;
                            authenticationResult.UserRole = userRole != null ? userRole.RoleName : "";
                            authenticationResult.MenuItemNames = MenuItemNames;
                            authenticationResult.IsChangePasswordRequired = isChangePasswordRequired;
                            authenticationResult.Profile = "Empty";
                           // authenticationResult.TourStatus = user.TourStatus;
                            return authenticationResult;
                        }
                        else
                        {
                            throw new Exception("The user name or password is incorrect.");
                        }
                    }
                }
                else
                {
                    throw new Exception("The user name or password is incorrect.");
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("AuthorizationServerProvider/GrantResourceOwnerCredentials :- ", ex);
                throw ex;
            }

        }

        public LdapUser GetAuthenticatedLdapUser(string userName, string password)
        {
            //string domain = ConfigurationManager.AppSettings["DomainName"].ToString();
            string domain = "exalca";
            string domainAndUsername = string.Empty;
            string SAMAccountName = string.Empty;
            if (userName.Contains(@"\"))
            {
                domainAndUsername = userName;
                SAMAccountName = Regex.Split(domainAndUsername, @"\")[1];
            }
            else
            {
                domainAndUsername = domain + @"\" + userName;
                SAMAccountName = userName;
            }

            //var _path = ConfigurationManager.AppSettings["LDAPConnectionString"].ToString();
            var _path = "LDAP://exalca.corp";
            System.DirectoryServices.DirectoryEntry entry = new System.DirectoryServices.DirectoryEntry(_path, domainAndUsername, password);
            System.DirectoryServices.DirectoryEntry dirEntry = new System.DirectoryServices.DirectoryEntry();
            LdapUser user = new LdapUser();

            try
            {
                // Bind to the native AdsObject to force authentication.
                Object obj = entry.NativeObject;
                DirectorySearcher search = new DirectorySearcher(entry);
                search.Filter = "(&(objectClass=user)(SAMAccountName=" + SAMAccountName + "))";
                SearchResult result = search.FindOne();
                if (result == null)
                {
                    return null;
                }
                else
                {
                    dirEntry = result.GetDirectoryEntry();
                    user.UserID = dirEntry.Guid;
                    user.Path = result.Path;
                    user.Email = GetProperty(result, "mail");
                    user.UserName = GetProperty(result, "samaccountname");
                    user.DisplayName = GetProperty(result, "displayname");
                    user.FirstName = GetProperty(result, "givenName");
                    user.LastName = GetProperty(result, "sn");
                    user.Mobile = GetProperty(result, "mobile");
                }

            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("AuthorizationServerProvider/GetAuthenticatedLdapUser :- ", ex);
                return null;
            }
            finally
            {
                entry.Dispose();
                dirEntry.Dispose();
            }
            return user;
        }

        public string GetProperty(SearchResult searchResult, string PropertyName)
        {
            return searchResult.Properties.Contains(PropertyName) ? searchResult.Properties[PropertyName][0].ToString() : null;
        }

        #region EncryptAndDecrypt
        public string Decrypt(string Password, bool UseHashing)
        {
            try
            {
                //string EncryptionKey = ConfigurationManager.AppSettings["EncryptionKey"];
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
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("AuthorizationServerProvider/Decrypt :- ", ex);
                return null;
            }

        }

        public string Encrypt(string Password, bool useHashing)
        {
            try
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
                byte[] resultArray = cTransform.TransformFinalBlock(ToEncryptArray, 0,
                  ToEncryptArray.Length);
                tdes.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteToFile("AuthorizationServerProvider/Encrypt :- ", ex);
                return null;
            }
        }

        #endregion

    }
}
