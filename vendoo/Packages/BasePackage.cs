using Oracle.ManagedDataAccess.Client;
using System.Data;
using vendoo.Models;

namespace vendoo.Packages
{
    public class BasePackage:ExtraPackage
    {
        private readonly string _connectionString;

        public BasePackage(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected OracleConnection GetConnection()
        {
            return new OracleConnection(_connectionString);
        }


        public bool RegisterUser(UserModel model)
        {
            using (OracleConnection connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (OracleCommand command = new OracleCommand("pkg_categories_operations.proc_add_user", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        string hashedPassword =HashPassword(model.Password);

                        command.Parameters.Add("p_name", OracleDbType.Varchar2).Value = model.Name;
                        command.Parameters.Add("p_lastname", OracleDbType.Varchar2).Value = model.LastName;
                        command.Parameters.Add("p_password", OracleDbType.Varchar2).Value = hashedPassword;
                        command.Parameters.Add("p_role_id", OracleDbType.Int32).Value = model.Role_id;

                        command.ExecuteNonQuery();
                        Console.WriteLine("Stored procedure executed successfully.");
                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }

        public TokenModel LoginUser(LoginModel model)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();

                    using (var command = new OracleCommand("pkg_categories_operations.proc_login_user", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_email", OracleDbType.Varchar2).Value = model.Email;
                        command.Parameters.Add("p_user_curs", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var hashedpassword = reader["password"].ToString();

                                if (ValidatePassword(model.Password, hashedpassword))
                                {
                                    return RetrieveUserInfo(connection, model.Email);
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                throw;
            }
        }
        private TokenModel RetrieveUserInfo(OracleConnection connection, string username)
        {
            using (var command = new OracleCommand("pkg_categories_operations.proc_get_user_info", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add("p_username", OracleDbType.Varchar2).Value = username;
                command.Parameters.Add("p_user_curs", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new TokenModel
                        {
                            Email = reader["email"].ToString(),
                            Id = Convert.ToInt32(reader["id"]),
                            Role = reader["role"].ToString()
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }


        public bool AddCategories(CategoriesModel model)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_categories", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = model.CategoryName;
                        command.ExecuteNonQuery();

                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }

        public bool AddCategories1StChild(CategoryModel model)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_1st_child", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = model.CategoryName;
                        command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = model.Category_ID;
                        command.ExecuteNonQuery();

                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }
        public bool AddCategories2ndChild(CategoryModel model)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    connection.Open();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_2nd_child", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = model.CategoryName;
                        command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = model.Category_ID;
                        command.ExecuteNonQuery();

                        return true;
                    }
                }
                catch (OracleException ex)
                {
                    Console.WriteLine($"Oracle error: {ex.Message}");
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General error: {ex.Message}");
                    return false;
                }
            }
        }


        private async Task<string> WriteFiles(IFormFile imageFile)
        {
            string imageFileName = string.Empty;

            try
            {
                if (imageFile != null)
                {
                    var imageExtension = Path.GetExtension(imageFile.FileName);

                    imageFileName = DateTime.Now.Ticks.ToString() + imageExtension;

                    var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "Upload", "Files");

                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    var filePath = Path.Combine(uploadFolder, imageFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return imageFileName;
        }

    }
}
