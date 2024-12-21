using Newtonsoft.Json;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using vendoo.Models;
using static System.Net.Mime.MediaTypeNames;

namespace vendoo.Packages
{
    public class BasePackage : ExtraPackage
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
                        string hashedPassword = HashPassword(model.Password);

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

                        command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = model.FirstChildName;
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
        public async Task<bool> AddCategories2ndChild(ChildCategoryModel model,string imagepath)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_2nd_child", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_sec_child_name", OracleDbType.Varchar2).Value = model.sec_child_name;
                        command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = model.Category_id;
                        command.Parameters.Add("p_description", OracleDbType.Varchar2).Value = model.Description;
                        command.Parameters.Add("p_price", OracleDbType.Int32).Value = model.Price;                       
                        command.Parameters.Add("p_image", OracleDbType.Varchar2).Value = imagepath;
                      
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


        public async Task<bool> AddItemDescription(ItemDescriptionModel model,List<string> imagepath)
        {
            using (var connection = GetConnection())
            {
                try
                {
                    await connection.OpenAsync();
                    using (var command = new OracleCommand("pkg_categories_operations.proc_add_3st_child", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add("p_product_name", OracleDbType.Varchar2).Value = model.Product_name;
                        command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = model.Category_id;
                        command.Parameters.Add("p_full_description", OracleDbType.Varchar2).Value = model.Full_Description;
                        command.Parameters.Add("p_price", OracleDbType.Int32).Value = model.Price;
                        command.Parameters.Add("p_contact", OracleDbType.Int32).Value = model.Contact;
                        string imagePathsJson = JsonConvert.SerializeObject(imagepath);
                        command.Parameters.Add("p_json", OracleDbType.Clob).Value = imagePathsJson;


                        await command.ExecuteNonQueryAsync();
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

        public List<CategoryModel> GetCategories1stChild(int id)
        {
            List<CategoryModel> categories = new List<CategoryModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_1stchild_categories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = id;
                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new CategoryModel
                            {
                                CategoryName = reader["category_name"].ToString(),
                                FirstChildName = reader["first_child_name"].ToString(),
                                Id = Convert.ToInt32(reader["id"])
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }

        public List<GetCategoryModel> GetFilteredItems(int id,string category_name,string? child_name,int? locationId,int? guestId, int? priceMin,int? priceMax)
        {
            List<GetCategoryModel> categories = new List<GetCategoryModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_filtered_categories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                                        
                    command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = id;
                    command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = category_name;
                    command.Parameters.Add("p_child_category_name", OracleDbType.Varchar2).Value = child_name;
                    command.Parameters.Add("p_location_id", OracleDbType.Int32).Value =locationId;
                    command.Parameters.Add("p_guest_id", OracleDbType.Int32).Value = guestId;
                    command.Parameters.Add("p_price_min", OracleDbType.Int32).Value = priceMin;
                    command.Parameters.Add("p_price_max", OracleDbType.Int32).Value =priceMax;

                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new GetCategoryModel
                            {
                                category_name = reader["category_name"] != DBNull.Value ? reader["category_name"].ToString() : string.Empty,
                                first_child_name = reader["first_child_name"] != DBNull.Value ? reader["first_child_name"].ToString() : string.Empty,
                                sec_child_name = reader["sec_child_name"] != DBNull.Value ? reader["sec_child_name"].ToString() : string.Empty,
                                Description = reader["description"] != DBNull.Value ? reader["description"].ToString() : string.Empty,
                                Price = reader["price"] != DBNull.Value ? Convert.ToInt32(reader["price"]) : 0,
                                Image = reader["image"] != DBNull.Value ? reader["image"].ToString() : string.Empty,
                                location_id = reader["location_id"]!=DBNull.Value? Convert.ToInt32(reader["location_id"]):0,
                                Guest_id= reader["guest_id"]!=DBNull.Value? Convert.ToInt32(reader["guest_id"]):0
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }
        public List<CategoriesModel> GetCategories()
        {
            List<CategoriesModel> categories = new List<CategoriesModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_all_categories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new CategoriesModel
                            {
                                CategoryName = reader["category_name"].ToString(),
                                Id = Convert.ToInt32(reader["id"])
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }
        public List<GetCategoryModel> GetCategories2ndChild(int id,string category_name)
        {
            List<GetCategoryModel> categories = new List<GetCategoryModel>();

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_child_categories", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_category_id", OracleDbType.Int32).Value = id;
                    command.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value =category_name;

                    command.Parameters.Add(cursorParameter);


                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var category = new GetCategoryModel
                            {
                                category_name = reader["category_name"]!=DBNull.Value? reader["category_name"].ToString():string.Empty,
                                first_child_name = reader["first_child_name"]!=DBNull.Value? reader["first_child_name"].ToString():string.Empty,
                                sec_child_name = reader["sec_child_name"]!=DBNull.Value? reader["sec_child_name"].ToString():string.Empty,
                                Description = reader["description"]!=DBNull.Value? reader["description"].ToString():string.Empty,
                                Price = reader["price"]!=DBNull.Value? Convert.ToInt32(reader["price"]):0,
                                Image = reader["image"]!=DBNull.Value? reader["image"].ToString():string.Empty
                            };
                            categories.Add(category);
                        }
                    }
                }
            }
            return categories;
        }
       
            public GetProductModel ItemDescription(int id)
            {
              GetProductModel products = null;

            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_all_products", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_id", OracleDbType.Int32).Value = id;
                    command.Parameters.Add(cursorParameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products = new GetProductModel
                            {
                                Product_name = reader["product_name"] != DBNull.Value ? reader["product_name"].ToString() : string.Empty,
                                Full_Description = reader["full_description"] != DBNull.Value ? reader["full_description"].ToString() : string.Empty,
                                Price = reader["price"] != DBNull.Value ? Convert.ToInt32(reader["price"]) : 0,
                                Contact = reader["contact"] != DBNull.Value ? Convert.ToInt32(reader["contact"]) : 0,
                                Image_path = reader["image_path"] != DBNull.Value ?JsonConvert.DeserializeObject<List<string>>(reader["image_path"].ToString()) :new List<string>()
                            };  
                        }
                    }
                }
            }
                return products;
            }

        public List<LocationModel> getLocations(int? id)
        {
            List<LocationModel> locations= new List<LocationModel>();


            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_locations", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add("p_id", OracleDbType.Int32).Value = id;
                    command.Parameters.Add(cursorParameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var location = new LocationModel
                            {
                                Location = reader["location"] != DBNull.Value ? reader["location"].ToString() : string.Empty,
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                            };
                            locations.Add(location);
                        }
                    }
                }
            }
            return locations;
        }

        public List<GuestNumberModel> getGuestNumber()
        {
            List<GuestNumberModel> locations = new List<GuestNumberModel>();


            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new OracleCommand("pkg_categories_operations.proc_get_guestnumber", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    OracleParameter cursorParameter = new OracleParameter
                    {
                        OracleDbType = OracleDbType.RefCursor,
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(cursorParameter);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var location = new GuestNumberModel
                            {
                                Id = reader["id"] != DBNull.Value ? Convert.ToInt32(reader["id"]) : 0,
                                GuestNumber = reader["guest_number"] != DBNull.Value ? Convert.ToInt32(reader["guest_number"]) : 0,
                            };
                            locations.Add(location);
                        }
                    }
                }
            }
            return locations;
        }
    }
    }

